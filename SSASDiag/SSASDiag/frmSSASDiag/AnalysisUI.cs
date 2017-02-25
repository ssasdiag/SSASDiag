using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace SSASDiag
{
    public partial class frmSSASDiag : Form
    {
        #region AnalysisUI

        #region AnalysisLocals
        ContextMenu mnuProfilerAnalysisContext;
        bool bCancelProfilerImport = false;
        System.Windows.Forms.Timer AnalysisMessagePumpTimer = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer AnalysisQueryExecutionPumpTimer = new System.Windows.Forms.Timer();
        Process ASProfilerTraceImporterProcess;
        SqlCommand ProfilerAnalysisQueryCmd;
        DateTime EndOfTrace = DateTime.MinValue;
        bool bProfilerEventClassSublcassViewPresent = false, bProfilerQueryStatsPresent = false;
        #endregion AnalysisLocals

        private void tcCollectionAnalysisTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcCollectionAnalysisTabs.SelectedIndex == 1 && tbAnalysis.Enabled)
            {
                if (dc != null && !bProfilerTraceDbAttached)
                {
                    AnalysisTraceID = dc.TraceID;
                    if (Directory.Exists(Environment.CurrentDirectory + "\\" + dc.TraceID))
                    {
                        m_analysisPath = txtFolderZipForAnalysis.Text = Environment.CurrentDirectory + "\\" + dc.TraceID;
                        PopulateAnalysisTabs();
                    }
                    else
                    {
                        if (File.Exists(Environment.CurrentDirectory + "\\" + dc.TraceID + ".zip"))
                        {
                            m_analysisPath = txtFolderZipForAnalysis.Text = Environment.CurrentDirectory + "\\" + dc.TraceID + ".zip";
                            PopulateAnalysisTabs();
                        }
                    }
                }
            }
        }
        private void btnAnalysisFolder_Click(object sender, EventArgs e)
        {
            BrowseForFolder bff = new BrowseForFolder();
            bff.Filters.Add("zip");
            bff.Filters.Add("trc");
            bff.Filters.Add("blg");
            bff.Filters.Add("mdf");
            bff.Filters.Add("ini");
            bff.Filters.Add("evtx");
            bff.Filters.Add("mdmp");
            string strPath = Environment.CurrentDirectory as string;
            strPath = bff.SelectFolder("Choose an SSASDiag folder or zip file for analysis of all its components.\r\nOR\r\nChoose an SSAS profiler trace file or database, performance monitor log, crash dump, network trace, or config file.", txtFolderZipForAnalysis.Text == "" ? strPath : txtFolderZipForAnalysis.Text, this.Handle);
            if (strPath != null && strPath != "")
            {
                txtFolderZipForAnalysis.Text = m_analysisPath = strPath;
                PopulateAnalysisTabs();
            }
            AnalysisMessagePumpTimer.Interval = 1000;
        }
        private void PopulateAnalysisTabs()
        {
            if (File.Exists(m_analysisPath))
                AnalysisTraceID = m_analysisPath.Substring(0, m_analysisPath.LastIndexOf(".")).Substring(m_analysisPath.LastIndexOf("\\") + 1).TrimEnd("0123456789".ToCharArray());
            else
                AnalysisTraceID = m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace("_SSASDiagOutput", "_SSASDiag");

            foreach (TabPage t in tcAnalysis.TabPages)
            {
                if (t.Name == "tbProfilerTraces")
                    HiddenTabPages.Add(t);
            }
            tcAnalysis.TabPages.Clear();
            btnImportProfilerTrace.Visible = true;

            if (m_analysisPath != null)
            {
                if (m_analysisPath.EndsWith(".zip"))
                    SelectivelyExtractAnalysisDataFromZip();

                if ((File.Exists(m_analysisPath) && m_analysisPath.EndsWith("\\msmdsrv.ini")) || File.Exists(m_analysisPath + "\\msmdsrv.ini"))
                {
                    tcAnalysis.TabPages.Add(new TabPage("Configuration") { ImageIndex = 0, Name = "Configuration" });
                    tcAnalysis.TabPages["Configuration"].Controls.Add(GetStatusTextBox("Check back soon for automated analysis of configuration details."));
                }
                if ((File.Exists(m_analysisPath) && m_analysisPath.EndsWith(".mdmp")) || 
                    (!File.Exists(m_analysisPath) && Directory.GetFiles(m_analysisPath, "*.mdmp", SearchOption.AllDirectories).Count() > 0))
                {
                    tcAnalysis.TabPages.Add(new TabPage("Crash Dumps") { ImageIndex = 1, Name = "Crash Dumps" });
                    tcAnalysis.TabPages["Crash Dumps"].Controls.Add(GetStatusTextBox("Check back soon for automated analysis of crash dumps."));
                }
                if ((File.Exists(m_analysisPath) && m_analysisPath.EndsWith(".evtx")) || 
                    File.Exists(m_analysisPath + "\\" + AnalysisTraceID + "_Application.evtx") ||
                    File.Exists(m_analysisPath + "\\" + AnalysisTraceID + "_System.evtx"))
                {
                    tcAnalysis.TabPages.Add(new TabPage("Event Logs") { ImageIndex = 2, Name = "Event Logs" });
                    tcAnalysis.TabPages["Event Logs"].Controls.Add(GetStatusTextBox("Check back soon for automated analysis of event logs."));
                }
                if ((File.Exists(m_analysisPath) && m_analysisPath.EndsWith(".etl")) ||
                    File.Exists(m_analysisPath + "\\" + AnalysisTraceID + ".etl") || 
                    File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.diag.log"))
                {
                    tcAnalysis.TabPages.Add(new TabPage("Network Trace") { ImageIndex = 3, Name = "Network Trace" });
                    TextBox txtNetworkAnalysis = GetStatusTextBox();
                    Button btnAnalyzeNetworkTrace = new Button() { Text = "Analyze Trace", Name = "btnAnalyzeNetworkTrace", Left = tcAnalysis.Width / 2 - 54, Width = 108, Top = 80, Visible = false };
                    tcAnalysis.TabPages["Network Trace"].Controls.Add(btnAnalyzeNetworkTrace);
                    btnAnalyzeNetworkTrace.Click += btnAnalyzeNetworkTrace_Click;
                    tcAnalysis.TabPages["Network Trace"].Controls.Add(txtNetworkAnalysis);

                    if (File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log"))
                    {
                        txtNetworkAnalysis.Text = File.ReadAllText(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log");
                        AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log");
                        AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.diag.log");
                    }
                    else
                    {
                        btnAnalyzeNetworkTrace.Visible = true;
                    }
                }
                if ((File.Exists(m_analysisPath) && m_analysisPath.EndsWith(".blg")) || File.Exists(m_analysisPath + "\\" + AnalysisTraceID + ".blg"))
                {
                    tcAnalysis.TabPages.Add(new TabPage("Performance Logs") { ImageIndex = 4, Name = "Performance Logs" });
                    tcAnalysis.TabPages["Performance Logs"].Controls.Add(GetStatusTextBox("Check back soon for automated analysis of performance logs."));
                }
                if (
                        (File.Exists(m_analysisPath) && m_analysisPath.EndsWith(".trc")) ||
                        (File.Exists(m_analysisPath) && m_analysisPath.EndsWith(".mdf")) ||
                        (
                            !File.Exists(m_analysisPath) &&
                            (Directory.GetFiles(m_analysisPath, AnalysisTraceID + "*.trc").Count() > 0 || 
                             File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf"))
                        )
                   )
                {
                    btnImportProfilerTrace.Visible = true;
                    splitProfilerAnalysis.Visible = false;
                    ProfilerTraceStatusTextBox.Text = "";
                    tcAnalysis.TabPages.Add(HiddenTabPages.Where(t => t.Text == "Profiler Traces").First());
                    HiddenTabPages.Remove(HiddenTabPages.Where(t => t.Text == "Profiler Traces").First());
                    string sqlForTraces = Properties.Settings.Default["SqlForProfilerTraceAnalysis"] as string;
                    string mdfPath = "";
                    if (m_analysisPath.EndsWith(".trc"))
                        mdfPath = m_analysisPath.Substring(0, m_analysisPath.LastIndexOf("\\") + 1) + "Analysis" + m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\")).Replace(".trc", "").TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) + ".mdf";
                    else
                        mdfPath = m_analysisPath;
                    if (File.Exists(mdfPath) || File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf"))
                    {   
                        AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf");
                        AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf");
                        ProfilerTraceStatusTextBox.AppendText("Using trace data loaded into SQL .mdf at " + m_analysisPath + (m_analysisPath.EndsWith(".mdf") ? ".\r\n" : "\\Analysis\\.\r\n"));
                        new Thread(new ThreadStart(() => AttachProfilerTraceDB())).Start();
                        splitProfilerAnalysis.Visible = true;
                        btnImportProfilerTrace.Visible = false;
                    }
                    else
                    {
                        ProfilerTraceStatusTextBox.Text = "Trace file is not yet imported to database table for analysis.  Import to perform analysis.";
                    }
                }
            }
        }
        private void AnalysisMessagePumpTimer_Tick(object sender, EventArgs e)
        {
            if (tcAnalysis.SelectedTab.Name == "tbProfilerTraces")
            {
                ProfilerTraceStatusTextBox.AppendText(".");
            }
            if (tcAnalysis.SelectedTab.Name == "Network Trace")
            {
                (tcAnalysis.SelectedTab.Controls["StatusTextBox"] as TextBox).AppendText(".");
            }
        }
        private void AddFileFromFolderIfAnlyzingZip(string file)
        {
            if (txtFolderZipForAnalysis.Text.EndsWith(".zip"))
            {
                Ionic.Zip.ZipFile z = new Ionic.Zip.ZipFile(txtFolderZipForAnalysis.Text);
                if (z.Entries.Where(f => f.FileName.Substring(f.FileName.LastIndexOf("/") + 1) == file.Substring(file.LastIndexOf("\\") + 1)).Count() == 0)
                {
                    try
                    {
                        z.AddFile(file, "Analysis");
                        z.Save();
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                        // This may fail if a file is locked, but in that case we will skip it.
                    }
                }
            }
        }
        private TextBox GetStatusTextBox(string Text = "")
        {
            TextBox txtStatus = new TextBox();
            txtStatus.Name = "StatusTextbox";
            txtStatus.Multiline = true;
            txtStatus.ReadOnly = true;
            txtStatus.BackColor = SystemColors.ControlText;
            txtStatus.ForeColor = Color.LightSkyBlue;
            txtStatus.Font = (HiddenTabPages.Find(t => t.Name == "tbProfilerTraces") == null ? tcAnalysis.TabPages["tbProfilerTraces"] : HiddenTabPages.Find(t => t.Name == "tbProfilerTraces")).Font;
            txtStatus.Dock = DockStyle.Fill;
            txtStatus.WordWrap = false;
            txtStatus.ScrollBars = ScrollBars.Both;
            txtStatus.Text = Text;
            return txtStatus;
        }
        private void SelectivelyExtractAnalysisDataFromZip()
        {
            Ionic.Zip.ZipFile z = new Ionic.Zip.ZipFile(m_analysisPath);
            // Always extract directly into the current running location.
            // This ensures we don't accidentally fill up a temp drive or something with large files.
            AnalysisTraceID = m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace("_SSASDiagOutput", "_SSASDiag").Replace(".zip", "");
            m_analysisPath = m_analysisPath.Substring(0, m_analysisPath.LastIndexOf("\\") + 1) + AnalysisTraceID;
            
            if (!Directory.Exists(m_analysisPath))
                Directory.CreateDirectory(m_analysisPath);
            if (!Directory.Exists(m_analysisPath + "\\Analysis"))
                Directory.CreateDirectory(m_analysisPath + "\\Analysis");

            if (z.Entries.Where(f => f.FileName == "Analysis\\" + AnalysisTraceID + ".mdf").Count() > 0)
            {
                try
                {
                    z.Entries.Where(f => f.FileName == "Analysis\\" + AnalysisTraceID + ".mdf").First().Extract(m_analysisPath + "\\Analysis", Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                    z.Entries.Where(f => f.FileName == "Analysis\\" + AnalysisTraceID + ".ldf").First().Extract(m_analysisPath + "\\Analysis", Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                }
                catch (Exception ex)
                {
                    // Continue, since if we fail writing these files, it means they do already exist, and we can probably just attach subsequently without failure.
                    LogException(ex);
                }
            }
            else
               if (z.Entries.Where(f => f.FileName.Contains(".trc")).Count() > 0)
                foreach (Ionic.Zip.ZipEntry e in z.Entries.Where(f => f.FileName.Contains(".trc")))
                    e.Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
            if (z.Entries.Where(f => f.FileName == "Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log").Count() > 0)
                z.Entries.Where(f => f.FileName == "Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log").First().Extract(m_analysisPath + "\\Analysis", Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
            else
            {
                if (z.Entries.Where(f => f.FileName == AnalysisTraceID + ".etl").Count() > 0)
                    z.Entries.Where(f => f.FileName == AnalysisTraceID + ".etl").First().Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                if (z.Entries.Where(f => f.FileName == AnalysisTraceID + ".cab").Count() > 0)
                    z.Entries.Where(f => f.FileName == AnalysisTraceID + ".cab").First().Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
            }
            if (z.Entries.Where(f => f.FileName.Contains(".blg")).Count() > 0)
                foreach (Ionic.Zip.ZipEntry e in z.Entries.Where(f => f.FileName.Contains(".blg")))
                    e.Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
            if (z.Entries.Where(f => f.FileName.Contains(".mdmp")).Count() > 0)
                foreach (Ionic.Zip.ZipEntry e in z.Entries.Where(f => f.FileName.Contains(".mdmp")))
                    e.Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
            if (z.Entries.Where(f => f.FileName.Contains(".evtx")).Count() > 0)
                foreach (Ionic.Zip.ZipEntry e in z.Entries.Where(f => f.FileName.Contains(".evtx")))
                    e.Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
            if (z.Entries.Where(f => f.FileName == "msmdsrv.ini").Count() > 0)
                z.Entries.Where(f => f.FileName == "msmdsrv.ini").First().Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
            z.Dispose();
        }
        private void btnAnalyzeNetworkTrace_Click(object sender, EventArgs e)
        {
            if (!File.Exists(m_analysisPath) && !Directory.Exists(m_analysisPath))
                Directory.CreateDirectory(m_analysisPath);
            if (!Directory.Exists(m_analysisPath + "\\Analysis"))
                Directory.CreateDirectory(m_analysisPath + "\\Analysis");

            TextBox txtNetworkAnalysis = tcAnalysis.TabPages["Network Trace"].Controls["StatusTextbox"] as TextBox;
            tcAnalysis.TabPages["Network Trace"].Controls[0].Visible = false;
            txtNetworkAnalysis.Text = "Analysis of network trace started...";
            AnalysisMessagePumpTimer.Interval = 1000;
            AnalysisMessagePumpTimer.Start();

            new Thread(new ThreadStart(new System.Action(() =>
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\sqlna.exe";
                p.StartInfo.Arguments = "\"" + m_analysisPath + "\\" + AnalysisTraceID + ".etl\" /output \"" + m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log\"";
                p.Start();
                string sOut = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                List<string> sNetworkAnalysis = new List<string>(File.ReadAllLines(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log"));
                sNetworkAnalysis.RemoveRange(0, 6);
                sNetworkAnalysis.RemoveRange(sNetworkAnalysis.Count - 6, 6);
                for (int i = 0; i < sNetworkAnalysis.Count; i++)
                    sNetworkAnalysis[i] = sNetworkAnalysis[i].TrimStart(' ');
                File.WriteAllLines(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log", sNetworkAnalysis);
                txtNetworkAnalysis.Invoke(new System.Action(() => txtNetworkAnalysis.Text = string.Join("\r\n", sNetworkAnalysis.ToArray())));
                AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log");
                AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.diag.log");
                if (txtFolderZipForAnalysis.Text.EndsWith(".zip"))
                {
                    foreach (string file in Directory.EnumerateFiles(m_analysisPath, AnalysisTraceID + "*.etl"))
                        File.Delete(file);
                    foreach (string file in Directory.EnumerateFiles(m_analysisPath, AnalysisTraceID + "*.cab"))
                        File.Delete(file);
                }
                AnalysisMessagePumpTimer.Stop();
            }))).Start();
        }

        #region Profiler Trace Analysis
        enum ProfilerQueryTypes
        {
            BaseQuery = 1,
            QueriesWithEventClassSubclassNames,
            QueriesWithQueryStats,
            AllQueries            
        }
        class ProfilerTraceQuery
        {
            public ProfilerTraceQuery(string name, string query, string desc, ProfilerQueryTypes queryType)
            { Name = name; Query = query; QueryType = queryType; Description = desc; }
            public ProfilerTraceQuery(ProfilerTraceQuery p, ProfilerQueryTypes t)
            {
                Name = p.Name;
                Query = p.Query;
                Description = p.Description;
                QueryType = t;
            }
            public string Name { get; set; }
            public string Query { get; set; }
            public string Description { get; set; } 
            public ProfilerQueryTypes QueryType { get; set; }
        }
        private string ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(string qry)
        {
            return qry.Replace("[Table_v]", "[Table]").Replace("EventClassName, ", "").Replace("EventSubclassName, ", "").Replace("EventClassName", "").Replace("EventSubclassName", "");
        }
        private List<ProfilerTraceQuery> InitializeProfilerTraceAnalysisQueries()
        {
            List<ProfilerTraceQuery> q = new List<ProfilerTraceQuery>();
            q.Add(new ProfilerTraceQuery("", "", "", ProfilerQueryTypes.BaseQuery));

            // Basic details
            q.Add(new ProfilerTraceQuery("Basic trace summary",
                                         Properties.Resources.QueryBasicTraceSummary, 
                                         "The basic trace summary gives a very high level overview of the contents of the trace.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));

            // Query FE/SE Stats
            q.Add(new ProfilerTraceQuery("Formula/Storage engine statistics",
                                         Properties.Resources.QueryFESEStats, 
                                         "Statistics calculated showing percentage of time spent in formula engine (calculations) vs. storage engine (IO) if Query Subcube events available.",
                                         ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.AllQueries));

            // Longest running queries
            q.Add(new ProfilerTraceQuery("Longest running queries captured",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryLongestRunningQueries),
                                         "Reports the longest running queries in the trace.  Includes calculated durations for queries started but not completed in the trace, up to the point of capture stop.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Longest running queries captured",
                                         Properties.Resources.QueryLongestRunningQueries,
                                         "Reports the longest running queries in the trace.  Includes calculated durations for queries started but not completed in the trace, up to the point of capture stop.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));


            // Longest running queries
            q.Add(new ProfilerTraceQuery("Longest running commands captured",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryLongestRunningCommands),
                                         "Reports the longest running commands in the trace.  Includes calculated durations for commands started but not completed in the trace, up to the point of capture stop.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Longest running commands captured",
                                         Properties.Resources.QueryLongestRunningCommands,
                                         "Reports the longest running commands in the trace.  Includes calculated durations for commands started but not completed in the trace, up to the point of capture stop.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));


            // Most collectively expensive events
            q.Add(new ProfilerTraceQuery("Most collectively expensive events",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryMostCollectivelyExpensiveEvents),
                                         "Summarizes identical events' durations to show the most cumulatively expensive type of activity in the trace.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Most collectively expensive events",
                                         Properties.Resources.QueryMostCollectivelyExpensiveEvents,
                                         "Summarizes identical events' durations to show the most cumulatively expensive type of activity in the trace.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Most collectively expensive queries
            q.Add(new ProfilerTraceQuery("Most collectively expensive queries",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryMostCollectivelyExpensiveQueries),
                                         "Summarizes identical queries to show the most cummulatively expensive queries in the trace.  Sometimes fast but frequently run queries may still be the culprit in a server encountering degradation.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Most collectively expensive queries",
                                         Properties.Resources.QueryMostCollectivelyExpensiveQueries,
                                         "Summarizes identical queries to show the most cummulatively expensive queries in the trace.  Sometimes fast but frequently run queries may still be the culprit in a server encountering degradation.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Most collectively expensive commands
            q.Add(new ProfilerTraceQuery("Most collectively expensive commands",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryMostCollectivelyExpensiveCommands),
                                         "Summarizes identical queries to show the most cummulatively expensive commands in the trace.  Sometimes fast but frequently run jobs may still be the culprit in a server encountering degradation.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Most collectively expensive commands",
                                         Properties.Resources.QueryMostCollectivelyExpensiveCommands,
                                         "Summarizes identical queries to show the most cummulatively expensive commands in the trace.  Sometimes fast but frequently run jobs may still be the culprit in a server encountering degradation.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Errors
            q.Add(new ProfilerTraceQuery("Queries/commands with errors",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryQueriesCommandsWithErrors),
                                         "Reports queries and commands with errors, and related error rows.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Queries/commands with errors",
                                         Properties.Resources.QueryQueriesCommandsWithErrors,
                                         "Reports queries and commands with errors, and related error rows.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Most impactful queries/commands
            q.Add(new ProfilerTraceQuery("Most impactful queries/commands",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryMostImpactfulQueriesCommands),
                                         "Calculates the \"most impactful\" queries and commands based on the number of other queries and commands that overlap.  Includes queries and commands that start but do not complete within the trace, or start before the trace but complete during its capture.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Most impactful queries/commands",
                                         Properties.Resources.QueryMostImpactfulQueriesCommands,
                                         "Calculates the \"most impactful\" queries and commands based on the number of other queries and commands that overlap.  Includes queries and commands that start but do not complete within the trace, or start before the trace but complete during its capture.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Queries/Commands not completed during trace
            q.Add(new ProfilerTraceQuery("Queries/commands not completed",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryQueriesCommandsNotCompleted),
                                         "Explicitly finds queries and commands started but not completed withing the trace.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Queries/commands not completed",
                                         Properties.Resources.QueryQueriesCommandsNotCompleted,
                                         "Explicitly finds queries and commands started but not completed withing the trace.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Possible runaway sessions
            q.Add(new ProfilerTraceQuery("Possible runaway sessions",
                                         Properties.Resources.QueryPossibleRunawaySessions,
                                         "Lists sessions with no command or query begin or end events found in the trace.  These may be executing runaway requests started before the trace and not completed during the trace either.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));

            return q;
        }
        private void btnImportProfilerTrace_Click(object sender, EventArgs e)
        {
            if (btnImportProfilerTrace.Text == "Import and &Analyze")
            {
                bCancelProfilerImport = false;
                tbCollection.ForeColor = SystemColors.ControlDark;
                tcCollectionAnalysisTabs.Refresh();
                tbCollection.Enabled = false;
                btnAnalysisFolder.Enabled = false;
                btnImportProfilerTrace.Text = "&Cancel Import";
                if (!ValidateProfilerTraceDBConnectionStatus())
                {
                    btnImportProfilerTrace.Text = "Import and &Analyze";
                    return;
                }
                AnalysisMessagePumpTimer.Interval = 1000;
                AnalysisMessagePumpTimer.Start();
                try
                {
                    BackgroundWorker bg = new BackgroundWorker();
                    bg.DoWork += bgImportProfilerTrace;
                    bg.RunWorkerCompleted += bgImportProfilerTraceComplete;
                    bg.RunWorkerAsync(new object[] { ProfilerTraceStatusTextBox, "Initial Catalog=" + AnalysisTraceID + ";Persist Security Info=False;" + connSqlDb.ConnectionString });
                }
                catch (SqlException ex)
                {
                    LogException(ex);
                    if (ex.Message == "ExecuteNonQuery requires an open and available Connection. The connection's current state is closed.")
                    {
                        ProfilerTraceStatusTextBox.Text = "Trace file is not yet imported to database table for analysis.  No SQL Server was available to perform import.";
                        btnImportProfilerTrace.Visible = true;
                    }
                    else
                        ProfilerTraceStatusTextBox.Text = "Error loading profiler trace: \r\n" + ex.Message;
                }
            }
            else
            {
                bCancelProfilerImport = true;
                btnImportProfilerTrace.Enabled = false;
                ProfilerTraceStatusTextBox.AppendText("\r\nUser cancelled loading of trace to table.  Dropping trace database...");
                BackgroundWorker bgCancelTrace = new BackgroundWorker();
                bgCancelTrace.DoWork += BgCancelTrace_DoWork;
                bgCancelTrace.RunWorkerCompleted += BgCancelTrace_RunWorkerCompleted;
                bgCancelTrace.RunWorkerAsync();                
            }
        }
        private void bgImportProfilerTrace(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (File.Exists(m_analysisPath))
                    m_analysisPath = m_analysisPath.Substring(0, m_analysisPath.LastIndexOf("\\"));
                        
                if (!File.Exists(m_analysisPath) && !Directory.Exists(m_analysisPath))
                    Directory.CreateDirectory(m_analysisPath);
                if (!Directory.Exists(m_analysisPath + "\\Analysis"))
                    Directory.CreateDirectory(m_analysisPath + "\\Analysis");

                string sSvcUser = "";
                ServiceController[] services = ServiceController.GetServices();
                foreach (ServiceController s in services.OrderBy(ob => ob.DisplayName))
                    if (s.DisplayName.Contains("SQL Server ("))
                    {
                        SelectQuery sQuery = new SelectQuery("select name, startname, pathname from Win32_Service where name = \"" + s.ServiceName + "\"");
                        ManagementObjectSearcher mgmtSearcher = new ManagementObjectSearcher(sQuery);
                        
                        foreach (ManagementObject svc in mgmtSearcher.Get())
                            sSvcUser = svc["startname"] as string;
                        if (sSvcUser.Contains(".")) sSvcUser = sSvcUser.Replace(".", Environment.UserDomainName);
                        if (sSvcUser == "LocalSystem") sSvcUser = "NT AUTHORITY\\SYSTEM";
                    }

                DirectoryInfo dirInfo = new DirectoryInfo(m_analysisPath + "\\Analysis");
                DirectorySecurity dirSec = dirInfo.GetAccessControl();
                dirSec.AddAccessRule(new FileSystemAccessRule(sSvcUser, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
                dirInfo.SetAccessControl(dirSec);

                if (AnalysisTraceID == "")
                    AnalysisTraceID = m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace("_SSASDiagOutput", "_SSASDiag");
                SqlCommand cmd = new SqlCommand(Properties.Resources.CreateDBSQLScript.
                                    Replace("<mdfpath/>", m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf").
                                    Replace("<ldfpath/>", m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf").
                                    Replace("<dbname/>", AnalysisTraceID)
                                    , connSqlDb);
                int ret = cmd.ExecuteNonQuery();

                ProfilerTraceStatusTextBox.Invoke(new System.Action(()=>
                    ProfilerTraceStatusTextBox.Text = "Importing profiler trace to database [" + AnalysisTraceID + "] on SQL instance: [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]."));
                string connstr = (e.Argument as object[])[1] as string;
                if (!connstr.Contains("Initial Catalog")) connstr += (connstr.EndsWith(";") ? "" : ";") + "Initial Catalog=" + AnalysisTraceID + ";";
                if (connstr.Contains("Initial Catalog=;")) connstr = connstr.Replace("Initial Catalog=;", "Initial Catalog=" + AnalysisTraceID + ";");

                ASProfilerTraceImporterProcess = new Process();
                ASProfilerTraceImporterProcess.StartInfo.UseShellExecute = false;
                ASProfilerTraceImporterProcess.StartInfo.CreateNoWindow = true;
                ASProfilerTraceImporterProcess.StartInfo.RedirectStandardOutput = true;
                ASProfilerTraceImporterProcess.StartInfo.FileName = Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\ASProfilerTraceImporterCmd.exe";
                ASProfilerTraceImporterProcess.StartInfo.Arguments = "\"" + Directory.GetFiles(m_analysisPath, AnalysisTraceID + "*.trc")[0] + "\" \"" + connstr + "\" \"" + AnalysisTraceID + "\"";
                ASProfilerTraceImporterProcess.Start();
                while (!ASProfilerTraceImporterProcess.HasExited)
                {
                    string sOut = ASProfilerTraceImporterProcess.StandardOutput.ReadLine();
                    System.Diagnostics.Trace.WriteLine(sOut);
                    if (sOut != null)
                        if (sOut.StartsWith("Loaded "))
                            ProfilerTraceStatusTextBox.Invoke(new System.Action(() =>
                            {
                                List<string> loadedline = ProfilerTraceStatusTextBox.Lines.Where(l => l.StartsWith("Loaded ")).ToList();
                                if (loadedline.Count > 0)
                                    ProfilerTraceStatusTextBox.Text = ProfilerTraceStatusTextBox.Text.Replace(loadedline.First(), sOut);
                                else
                                    ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text == "" ? "" : "\r\n") + sOut);
                            }
                            ));
                        else if (sOut == "Database prepared for analysis." || sOut == "Import of profiler trace cancelled.")
                            break;
                        else
                            ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text == "" ? "" : "\r\n") + sOut)));
                }
                if (!bCancelProfilerImport)
                {
                    ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Adding profiler database to collection data...")));
                    DettachProfilerTraceDB(false);
                    AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf");
                    AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf");
                    if (txtFolderZipForAnalysis.Text.EndsWith(".zip"))
                    {
                        ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText("\r\nDeleting redundant profiler trace files from data extraction location...")));
                        foreach (string file in Directory.EnumerateFiles(m_analysisPath, AnalysisTraceID + "*.trc"))
                            File.Delete(file);
                    }
                    AttachProfilerTraceDB();
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                if (ex.Message == "ExecuteNonQuery requires an open and available Connection. The connection's current state is closed.")
                {
                    ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.Text = "Trace file is not yet imported to database table for analysis.  No SQL Server was available to perform import.\r\n"));
                    btnImportProfilerTrace.Invoke(new System.Action(()=> btnImportProfilerTrace.Visible = true));
                }
                else
                    ProfilerTraceStatusTextBox.Invoke(new System.Action(()=> ProfilerTraceStatusTextBox.Text = "Error loading trace: " + ex.Message + "\r\n"));
            }
        }
        private void bgImportProfilerTraceComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("Import Trace Complete");
            if (!bCancelProfilerImport)  // We take care of cleanup and completion in cancellation worker if we're cancelled.
            {
                if (File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf"))
                {
                    btnImportProfilerTrace.Visible = false;
                    splitProfilerAnalysis.Visible = true;
                }
                tbCollection.ForeColor = SystemColors.ControlText;
                tcCollectionAnalysisTabs.Refresh();
                tbCollection.Enabled = true;
                btnAnalysisFolder.Enabled = true;
                btnImportProfilerTrace.Enabled = true;
                btnImportProfilerTrace.Text = "Import and &Analyze";
                AnalysisMessagePumpTimer.Stop();
            }
        }
        private void BgCancelTrace_DoWork(object sender, DoWorkEventArgs e)
        {
            EventWaitHandle doneWithInit = new EventWaitHandle(false, EventResetMode.ManualReset, "ASProfilerTraceImporterCmdCancelSignal");
            doneWithInit.Set();
            ASProfilerTraceImporterProcess.WaitForExit();
            connSqlDb.ChangeDatabase("master");
            doneWithInit.Close();
            SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') DROP DATABASE [" + AnalysisTraceID + "]", connSqlDb);
            cmd.ExecuteNonQuery();
            connSqlDb.Close();
            if (m_analysisPath.EndsWith(".mdf"))
            {
                File.Delete(m_analysisPath);
                File.Delete(m_analysisPath.Replace(".mdf", ".ldf"));
            }
            else
            {
                File.Delete(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf");
                File.Delete(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf");
            }
        }
        private void BgCancelTrace_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProfilerTraceStatusTextBox.AppendText("\r\nDropped trace database and deleted files successfully.\r\nTrace file is not yet imported to database table for analysis.  Import to perform analysis.\r\n");
            AnalysisMessagePumpTimer.Stop();
            tbCollection.ForeColor = SystemColors.ControlText;
            tcCollectionAnalysisTabs.Refresh();
            tbCollection.Enabled = true;
            btnAnalysisFolder.Enabled = true;
            btnImportProfilerTrace.Enabled = true;
            btnImportProfilerTrace.Text = "Import and &Analyze";
        }
        private void cmbProfilerAnalyses_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                txtProfilerAnalysisQuery.Text = (cmbProfilerAnalyses.DataSource as List<ProfilerTraceQuery>).First(kv => kv.Name == cmbProfilerAnalyses.Text).Query.Replace("[Table", "[" + AnalysisTraceID);
                txtProfilerAnalysisDescription.Text = (cmbProfilerAnalyses.DataSource as List<ProfilerTraceQuery>).First(kv => kv.Name == cmbProfilerAnalyses.Text).Description;

                if (txtProfilerAnalysisQuery.Text != "")
                {
                    Enabled = false;
                    BackgroundWorker bgLoadProfilerAnalysis = new BackgroundWorker();
                    bgLoadProfilerAnalysis.DoWork += BgLoadProfilerAnalysis_DoWork;
                    bgLoadProfilerAnalysis.RunWorkerCompleted += BgLoadProfilerAnalysis_RunWorkerCompleted;
                    StatusFloater.lblStatus.Text = "Running analysis query. (Esc to cancel...)";
                    StatusFloater.Left = this.Left + this.Width / 2 - StatusFloater.Width / 2;
                    StatusFloater.Top = this.Top + this.Height / 2 - StatusFloater.Height / 2;
                    StatusFloater.lblTime.Visible = true;
                    StatusFloater.lblTime.Text = "00:00";
                    StatusFloater.EscapePressed = false;
                    lblProfilerAnalysisStatusRight.Text = lblProfilerAnalysisStatusLeft.Text = lblProfilerAnalysisStatusCenter.Text = "";
                    AnalysisQueryExecutionPumpTimer.Interval = 1000;
                    AnalysisQueryExecutionPumpTimer.Start();
                    if (!StatusFloater.Visible)
                        StatusFloater.Show(this);

                    this.SuspendLayout();
                    bgLoadProfilerAnalysis.RunWorkerAsync();
                }
                else
                {
                    dgdProfilerAnalyses.DataSource = null;
                    dgdProfilerAnalyses.Refresh();
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        private void BgLoadProfilerAnalysis_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                connSqlDb.ChangeDatabase(AnalysisTraceID);
                ProfilerAnalysisQueryCmd = new SqlCommand(txtProfilerAnalysisQuery.Text, connSqlDb);
                ProfilerAnalysisQueryCmd.CommandTimeout = 0;
                SqlDataReader dr = ProfilerAnalysisQueryCmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);
                dgdProfilerAnalyses.Invoke(new System.Action(() =>
                    {
                        dgdProfilerAnalyses.AutoGenerateColumns = true;
                        dgdProfilerAnalyses.DataSource = null;
                        dgdProfilerAnalyses.Columns.Clear();
                        dgdProfilerAnalyses.DataSource = dt;
                        dgdProfilerAnalyses.Refresh();
                        lblProfilerAnalysisStatusRight.Text = dt.Rows.Count + " row" + (dt.Rows.Count > 1 ? "s" : "") + " returned.";
                        Int64 TotalDuration = 0;
                        DateTime minStart = DateTime.MaxValue, maxEnd = DateTime.MinValue;
                        if (dgdProfilerAnalyses.Columns["Duration"] != null || dgdProfilerAnalyses.Columns["StartTime"] != null || dgdProfilerAnalyses.Columns["EndTime"] != null || dgdProfilerAnalyses.Columns["CurrentTime"] != null)
                        {
                            foreach (DataGridViewRow r in dgdProfilerAnalyses.Rows)
                            {
                                if (dgdProfilerAnalyses.Columns["Duration"] != null && r.Cells["Duration"].FormattedValue as string != "") TotalDuration += Convert.ToInt64(r.Cells["Duration"].Value);
                                if (dgdProfilerAnalyses.Columns["StartTime"] != null && r.Cells["StartTime"].FormattedValue as string != "")
                                    if (Convert.ToDateTime(r.Cells["StartTime"].Value) < minStart)
                                        minStart = Convert.ToDateTime(r.Cells["StartTime"].Value);
                                DateTime rowTime = DateTime.MinValue;
                                if (dgdProfilerAnalyses.Columns["EndTime"] != null && r.Cells["EndTime"].FormattedValue as string != "")
                                    if (!DateTime.TryParse(r.Cells["EndTime"].Value.ToString(), out rowTime) || rowTime != DateTime.MinValue)
                                        if (rowTime == DateTime.MinValue)  // When we have a row that never finished
                                        {
                                            maxEnd = EndOfTrace;
                                            r.Cells["EndTime"].Style.ForeColor = Color.Red;
                                            r.Cells["Duration"].Style.ForeColor = Color.Red;
                                            r.Cells["Duration"].ToolTipText = "This duration is calculated only until the end of the trace since the request never completed.";
                                        }
                                        else
                                            maxEnd = rowTime;
                                if (dgdProfilerAnalyses.Columns["CurrentTime"] != null && r.Cells["CurrentTime"].FormattedValue as string != "")
                                    if (!DateTime.TryParse(r.Cells["CurrentTime"].Value.ToString(), out rowTime) || rowTime != DateTime.MinValue)
                                        if (rowTime == DateTime.MinValue)  // When we have a row that never finished
                                        {
                                            maxEnd = EndOfTrace;
                                            r.Cells["CurrentTime"].Style.ForeColor = Color.Red;
                                            r.Cells["Duration"].Style.ForeColor = Color.Red;
                                            r.Cells["Duration"].ToolTipText = "This duration is calculated only until the end of the trace since the request never completed.";
                                        }
                                        else
                                            maxEnd = rowTime;
                            }
                            if (TotalDuration > 0)
                                lblProfilerAnalysisStatusLeft.Text = "Total time: " + TimeSpan.FromMilliseconds(TotalDuration).ToString("hh\\:mm\\:ss") + ", Avg: " + TimeSpan.FromMilliseconds(Convert.ToDouble(TotalDuration / dt.Rows.Count)).ToString("hh\\:mm\\:ss");
                            if (minStart < DateTime.MaxValue && maxEnd > DateTime.MinValue)
                            {
                                lblProfilerAnalysisStatusCenter.Text = "Covers " + minStart.ToString("yyyy-MM-dd HH:mm:ss") + " to " + maxEnd.ToString("yyyy-MM-dd HH:mm:ss");
                                lblProfilerAnalysisStatusCenter.Left = Width / 2 - lblProfilerAnalysisStatusCenter.Width / 2;
                            }
                        }
                        lblProfilerAnalysisStatusRight.Left = Width - lblProfilerAnalysisStatusRight.Width - 41;

                        // Finally setup context menus after loading analysis query.
                        mnuProfilerAnalysisContext = new ContextMenu();
                        mnuProfilerAnalysisContext.MenuItems.Add(new MenuItem("Copy", ProfilerAnalysisContextMenu_Click));
                        mnuProfilerAnalysisContext.MenuItems.Add(new MenuItem("Copy with Headers", ProfilerAnalysisContextMenu_Click));

                        if (dt.Columns.Contains("RowNumber") || dt.Columns.Contains("StartRow") || dt.Columns.Contains("EndRow"))
                        {
                            mnuProfilerAnalysisContext.MenuItems.Add(new MenuItem("-"));
                            mnuProfilerAnalysisContext.MenuItems.Add(new MenuItem(string.Format("Find all queries/commands overlapping with selection"), ProfilerAnalysisContextMenu_Click));
                            if (cmbProfilerAnalyses.Text.ToLower().Contains("quer") && bProfilerQueryStatsPresent)
                                mnuProfilerAnalysisContext.MenuItems.Add(new MenuItem(string.Format("Lookup query statistics for selected queries"), ProfilerAnalysisContextMenu_Click));
                            mnuProfilerAnalysisContext.MenuItems.Add(new MenuItem(string.Format("Lookup detail rows for selected queries/commands"), ProfilerAnalysisContextMenu_Click));
                        }
                    }));
            }
            catch (Exception ex)
            {
                LogException(ex);
                if (ex.Message.Contains("cancelled by user"))
                {
                    connSqlDb.Close();
                    connSqlDb.Open();
                }
            }
        }
        private void AnalysisQueryExecutionPumpTimer_Tick(object sender, EventArgs e)
        {
            string curTime = StatusFloater.lblTime.Text;
            string[] timeparts = StatusFloater.lblTime.Text.Split(':');
            TimeSpan newTime = (TimeSpan.FromMinutes(Convert.ToDouble(timeparts[0])) + TimeSpan.FromSeconds(Convert.ToDouble(timeparts[1])).Add(TimeSpan.FromSeconds(1)));
            StatusFloater.lblTime.Text = newTime.ToString("mm\\:ss");
            if (StatusFloater.EscapePressed)
                ProfilerAnalysisQueryCmd.Cancel();
        }
        private void BgLoadProfilerAnalysis_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Invoke(new System.Action(() =>
                {
                    if (StatusFloater.EscapePressed)
                    {
                        cmbProfilerAnalyses.SelectedIndex = 0;
                        lblProfilerAnalysisStatusRight.Text = "Last query was cancelled.";
                        lblProfilerAnalysisStatusRight.Left = Width - lblProfilerAnalysisStatusRight.Width - 41;
                    }
                    Enabled = true;
                    StatusFloater.Visible = false;
                    StatusFloater.lblTime.Visible = false;
                    StatusFloater.EscapePressed = false;
                    dgdProfilerAnalyses.ClearSelection();
                    AnalysisQueryExecutionPumpTimer.Stop();
                    this.ResumeLayout();
                }));
        }
        private bool ValidateProfilerTraceDBConnectionStatus()
        {
            if (connSqlDb.State != ConnectionState.Open)
            {
                string sqlForTraces = Properties.Settings.Default["SqlForProfilerTraceAnalysis"] as string;
                string exMsg = "";
                if (sqlForTraces != "")
                {
                    connSqlDb = new SqlConnection("Data Source=" + sqlForTraces + ";Integrated Security=true;Connection Timeout=2;");
                    try
                    {
                        connSqlDb.Open();
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                        sqlForTraces = "";
                        exMsg = ex.Message;
                    }
                }
                if (sqlForTraces == "")
                {
                    frmSimpleSQLServerPrompt sqlprompt = new frmSimpleSQLServerPrompt();
                    sqlprompt.cmbServer.Text = sqlForTraces;
                    if (sqlprompt.ShowDialog(this) == DialogResult.OK)
                    {
                        Properties.Settings.Default["SqlForProfilerTraceAnalysis"] = sqlForTraces = sqlprompt.cmbServer.Text;
                        Properties.Settings.Default.Save();
                        connSqlDb = new SqlConnection("Data Source=" + sqlprompt.cmbServer.Text + ";Integrated Security=true;Persist Security Info=false;");
                        try { connSqlDb.Open(); }
                        catch (Exception ex) { LogException(ex); }
                    }
                    else
                    {
                        ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Failure attaching to trace database: " + exMsg + "\r\n");
                        return false;
                    }
                }
            }
            return true;
        }
        private void ValidateProfilerTraceViews()
        {
            SqlConnection conn = new SqlConnection(connSqlDb.ConnectionString.Replace("Connection Timeout=2;", ""));
            conn.Open();
            conn.ChangeDatabase(AnalysisTraceID);
            SqlCommand cmd = new SqlCommand("SELECT TOP 1 name FROM sys.views WHERE name = N'" + AnalysisTraceID + "_v'", conn);
            if (cmd.ExecuteScalar() != null)
                bProfilerEventClassSublcassViewPresent = true;
            else
                bProfilerEventClassSublcassViewPresent = false;
            ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith(".") || ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Confirmed event class/subclass view is " + (bProfilerEventClassSublcassViewPresent ? "present." : "not present."))));
            cmd.CommandText = "SELECT TOP 1 name FROM sys.views WHERE name = N'" + AnalysisTraceID + "_QueryStats'";
            if (cmd.ExecuteScalar() != null)
                bProfilerQueryStatsPresent = true;
            else
                bProfilerQueryStatsPresent = false;
            cmd.CommandText = "SELECT MAX(CurrentTime) FROM [" + AnalysisTraceID + "]";
            new Thread(new ThreadStart(() =>
                {
                    if (cmd.Connection.State == ConnectionState.Open)
                    {
                        // This isn't immediately required and we save 1-2s by doing it off this thread.
                        EndOfTrace = Convert.ToDateTime(cmd.ExecuteScalar());
                        System.Diagnostics.Trace.WriteLine("End of trace [" + AnalysisTraceID + "] noted at " + EndOfTrace);
                        conn.Close();
                    }
                })).Start(); 
            Invoke(new System.Action(() =>
                {
                    ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Confirmed query statistics view is " + (bProfilerQueryStatsPresent ? "present.\r\n" : "not present."))));
                    cmbProfilerAnalyses.DataSource = ProfilerTraceAnalysisQueries.Where(q => q.QueryType == (bProfilerEventClassSublcassViewPresent && bProfilerQueryStatsPresent ? ProfilerQueryTypes.AllQueries :
                                                                                                        bProfilerEventClassSublcassViewPresent ? ProfilerQueryTypes.QueriesWithEventClassSubclassNames :
                                                                                                        bProfilerQueryStatsPresent ? ProfilerQueryTypes.QueriesWithQueryStats :
                                                                                                        ProfilerQueryTypes.BaseQuery)
                                                                                                || q.Name == "").ToList();
                    cmbProfilerAnalyses.Refresh();
                }));
        }
        private void AttachProfilerTraceDB()
        {
            splitProfilerAnalysis.Invoke(new System.Action(() => splitProfilerAnalysis.Visible = splitProfilerAnalysis.Enabled = btnAnalysisFolder.Enabled = false));
            tcAnalysis.Invoke(new System.Action(()=>ProfilerTraceStatusTextBox.Text += (ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") || ProfilerTraceStatusTextBox.Text == "" ? "" : "\r\n") + "Attaching profiler trace database...\r\n"));
            ValidateProfilerTraceDBConnectionStatus();

            string mdfPath = m_analysisPath.EndsWith(".mdf") ? 
                                m_analysisPath.Substring(0, m_analysisPath.LastIndexOf("\\") + 1) : 
                                m_analysisPath.EndsWith(".trc") ?
                                    mdfPath = m_analysisPath.Substring(0, m_analysisPath.LastIndexOf("\\") + 1) + "Analysis\\" :
                                    m_analysisPath + "\\Analysis\\";
            if (connSqlDb.State == ConnectionState.Open)
            {
                connSqlDb.ChangeDatabase("master");
                SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') ALTER DATABASE [" + AnalysisTraceID + "] SET MULTI_USER", connSqlDb);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand("IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') CREATE DATABASE [" + AnalysisTraceID + "] ON (FILENAME = N'" + mdfPath + AnalysisTraceID + ".mdf'),"
                                                + "(FILENAME = N'" + mdfPath + AnalysisTraceID + ".ldf') "
                                                + "FOR ATTACH", connSqlDb);
                try
                {
                    cmd.ExecuteNonQuery();
                    bProfilerTraceDbAttached = true;
                    Invoke(new System.Action(() =>
                        {
                            splitProfilerAnalysis.Visible = true;
                            ttStatus.SetToolTip(chkDettachProfilerAnalysisDBWhenDone, "Profiler traces were imported into a trace database in the file:\r\n" + AnalysisTraceID
                                                                                + ".mdf\r\n\r\nLocated at:\r\n" + mdfPath + "\\Analysis\r\n\r\n"
                                                                                + "Uncheck this checkbox if the scenario requires further analysis.\r\n\r\n"
                                                                                + "Note:  While attached the SQL data source at [" + connSqlDb.DataSource + "] locks these files from deletion while started.");
                            ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Attached trace database [" + AnalysisTraceID + "]\r\nto SQL instance [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]\r\nfor analysis at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
                        }));

                    ValidateProfilerTraceViews();
                }
                catch (SqlException ex)
                {
                    LogException(ex);
                    Invoke(new System.Action(() => cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = false));

                    if (ex.Message.Contains("cannot be opened because it is version"))
                    {
                        MessageBox.Show("Unable to attach to database since it was created with a later version of SQL than the selected server.", "Select another instance", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        frmSimpleSQLServerPrompt sqlprompt = new frmSimpleSQLServerPrompt();
                        while (true)
                        {
                            if (sqlprompt.ShowDialog(this) == DialogResult.OK)
                            {
                                Properties.Settings.Default["SqlForProfilerTraceAnalysis"] = sqlprompt.cmbServer.Text;
                                Properties.Settings.Default.Save();
                                connSqlDb = new SqlConnection("Data Source=" + sqlprompt.cmbServer.Text + ";Integrated Security=true;Persist Security Info=false;");
                                connSqlDb.Open();
                                try
                                {
                                    cmd.Connection = connSqlDb;
                                    cmd.ExecuteNonQuery();
                                    bProfilerTraceDbAttached = true;
                                    Invoke(new System.Action(() =>
                                        {
                                            splitProfilerAnalysis.Visible = true;
                                            ttStatus.SetToolTip(chkDettachProfilerAnalysisDBWhenDone, "Profiler traces were imported into a trace database in the file:\r\n" + AnalysisTraceID
                                                                            + ".mdf\r\n\r\nLocated at:\r\n" + mdfPath + "\\Analysis\r\n\r\n"
                                                                            + "Uncheck this checkbox if the scenario requires further analysis.\r\n\r\n"
                                                                            + "Note:  While attached the SQL data source at [" + connSqlDb.DataSource + "] locks these files from deletion while started.");
                                            ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Attached trace database [" + AnalysisTraceID + "]\r\nto SQL instance [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]\r\nfor analysis at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
                                        }));
                                    ValidateProfilerTraceViews();
                                    break;
                                }
                                catch
                                {
                                    LogException(ex);
                                    MessageBox.Show("Unable to attach to database since it was created with a later version of SQL than the selected server.", "Select another instance", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                Invoke(new System.Action(()=> ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Unable to load trace data to SQL table.  No local instance able to host the data is available.\r\n")));
                                return;
                            }
                        }
                    }
                    else if (ex.Message.Contains("Unable to open the physical file") || ex.Message.Contains("The path specified by"))
                    {
                        ProfilerTraceStatusTextBox.Invoke(new System.Action(()=>ProfilerTraceStatusTextBox.Text = (ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Trace file is not yet imported to database table for analysis.  Import to perform analysis.\r\n"));
                        return;
                    }
                    else
                        ProfilerTraceStatusTextBox.Invoke(new System.Action(()=> ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Unable to attach to database due to exception:\r\n" + ex.Message)));
                }
            }
            btnAnalysisFolder.Invoke(new System.Action(() => btnAnalysisFolder.Enabled = splitProfilerAnalysis.Enabled = true));
        }
        private void DettachProfilerTraceDB(bool bClearText = true)
        {
            try
            {
                bProfilerTraceDbAttached = false;
                // Dettach without blocking for existing sessions...
                connSqlDb.ChangeDatabase("master");
                SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') ALTER DATABASE [" + AnalysisTraceID + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", connSqlDb);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') EXEC master.dbo.sp_detach_db @dbname = N'" + AnalysisTraceID + "'", connSqlDb);
                cmd.ExecuteNonQuery();
                connSqlDb.Close();
                Invoke(new System.Action(() =>
                    {
                        splitProfilerAnalysis.Visible = false;
                        btnImportProfilerTrace.Visible = true;
                        if (bClearText)
                            ProfilerTraceStatusTextBox.Text = "";
                    }));
            }
            catch (Exception ex) { LogException(ex); }  // could fail if service stopped, no biggie just move on...
        }
        private void dgdProfilerAnalyses_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.ColumnIndex != -1 && e.RowIndex != -1)
                {

                    if (dgdProfilerAnalyses.SelectedCells.Count == 0)
                        dgdProfilerAnalyses.CurrentCell = dgdProfilerAnalyses[e.ColumnIndex, e.RowIndex];
                    Rectangle rect = dgdProfilerAnalyses.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    mnuProfilerAnalysisContext.Show(dgdProfilerAnalyses, new Point(rect.X, rect.Y));
                }
            }
        }
        private bool ProcessCopyMenuClicks(object sender)
        {
            string sOut = "";
            int PriorRowNum = -1;
            if ((sender as MenuItem).Text == "Copy")
            {
                List<DataGridViewCell> cells = new List<DataGridViewCell>();
                foreach (DataGridViewCell c in dgdProfilerAnalyses.SelectedCells)
                    cells.Add(c);
                cells = cells.OrderBy(c => c.ColumnIndex).OrderBy(c => c.RowIndex).ToList();
                foreach (DataGridViewCell c in cells)
                {
                    if (PriorRowNum != c.RowIndex && PriorRowNum != -1)
                        sOut += "\r\n" + (c.Value is DBNull ? "NULL" : c.Value + "");
                    else
                        if (PriorRowNum == -1)
                        sOut = (c.Value is DBNull ? "NULL" : c.Value + "");
                    else
                        sOut += ", " + (c.Value is DBNull ? "NULL" : c.Value + "");
                    PriorRowNum = c.RowIndex;
                }
                Clipboard.SetText(sOut);
                return true;
            }
            if ((sender as MenuItem).Text == "Copy with Headers")
            {
                List<DataGridViewCell> cells = new List<DataGridViewCell>();
                foreach (DataGridViewCell c in dgdProfilerAnalyses.SelectedCells)
                    cells.Add(c);
                cells = cells.OrderBy(c => c.ColumnIndex).OrderBy(c => c.RowIndex).ToList();
                string headers = "", sLine = "", priorHeaders = "";
                foreach (DataGridViewCell c in cells)
                {
                    if (PriorRowNum != c.RowIndex || PriorRowNum == -1)
                    {
                        if (PriorRowNum != -1)
                        {
                            sOut += "\r\n" + (headers != priorHeaders ? headers + "\r\n" : "") + sLine;
                            priorHeaders = headers;
                            sLine = "";
                            headers = "";
                        }
                        headers = dgdProfilerAnalyses.Columns[c.ColumnIndex].HeaderCell.Value + "";
                        sLine = (c.Value is DBNull ? "NULL" : c.Value + "");
                    }
                    else
                    {
                        headers += ", " + dgdProfilerAnalyses.Columns[c.ColumnIndex].HeaderCell.Value + "";
                        sLine += ", " + (c.Value is DBNull ? "NULL" : c.Value + "");
                    }
                    PriorRowNum = c.RowIndex;
                }
                sOut += "\r\n" + (headers != priorHeaders ? headers + "\r\n" : "") + sLine;
                Clipboard.SetText(sOut.TrimStart(new char[] { '\r', '\n' }));
                return true;
            }
            return false;
        }
        private void ProfilerAnalysisContextMenu_Click(object sender, EventArgs e)
        {
            if (ProcessCopyMenuClicks(sender))
                return;

            List<int?> rows = new List<int?>();
            foreach (DataGridViewCell c in dgdProfilerAnalyses.SelectedCells)
            {
                if  (
                        (
                            (sender as MenuItem).Text.ToLower().Contains("query statistics") &&
                            dgdProfilerAnalyses.Columns.Contains("EventClass") &&
                            dgdProfilerAnalyses.Rows[c.RowIndex].Cells["EventClass"].Value as int? == 9
                        ) ||
                        !(sender as MenuItem).Text.ToLower().Contains("query statistics")
                    )
                {
                    if (dgdProfilerAnalyses.Columns.Contains("RowNumber"))
                        rows.Add(dgdProfilerAnalyses.Rows[c.RowIndex].Cells["RowNumber"].Value as int?);
                    else if (dgdProfilerAnalyses.Columns.Contains("EndRow") && dgdProfilerAnalyses.Rows[c.RowIndex].Cells["EndRow"].Value != null)
                        rows.Add(dgdProfilerAnalyses.Rows[c.RowIndex].Cells["EndRow"].Value as int?);
                    else if (dgdProfilerAnalyses.Columns.Contains("StartRow"))
                        rows.Add(dgdProfilerAnalyses.Rows[c.RowIndex].Cells["StartRow"].Value as int?);
                }
            }
            rows = rows.Distinct().ToList();
            if (rows.Count == 0)
                return;
            string strQry = "";
            cmbProfilerAnalyses.SelectedIndex = 0;
            if ((sender as MenuItem).Text == "Find all queries/commands overlapping with selection")
            {
                foreach (int row in rows)
                {
                    strQry += (strQry == "" ? ("select a.RowNumber, a.Duration, a.EventClass, a.EventClassName, a.CurrentTime, a.StartTime, a.ConnectionID, a.NTUserName, a.NTDomainName, a.DatabaseName, a.TextData, a.ClientProcessID, a.ApplicationName, a.CPUTime, a.EventSubclass, a.SPID, convert(nvarchar(max), a.RequestParameters), convert(nvarchar(max), a.RequestProperties)\r\nfrom [Table_v] a,\r\n(select StartTime, CurrentTime from [Table] where RowNumber = " + row + ") b\r\nwhere a.eventclass in (10, 16)\r\nand a.CurrentTime >= b.StartTime and a.CurrentTime <= b.CurrentTime").Replace("[Table", "[" + AnalysisTraceID)
                                 : ("\r\nunion\r\nselect a.RowNumber, a.Duration, a.EventClass, a.EventClassName, a.CurrentTime, a.StartTime, a.ConnectionID, a.NTUserName, a.NTDomainName, a.DatabaseName, a.TextData, a.ClientProcessID, a.ApplicationName, a.CPUTime, a.EventSubclass, a.SPID, convert(nvarchar(max), a.RequestParameters), convert(nvarchar(max), a.RequestProperties)\r\nfrom [Table_v] a,\r\n(select StartTime, CurrentTime from [Table] where RowNumber = " + row + ") b\r\nwhere a.eventclass in (10, 16)\r\nand a.CurrentTime >= b.StartTime and a.CurrentTime <= b.CurrentTime").Replace("[Table", "[" + AnalysisTraceID));
                }
                txtProfilerAnalysisQuery.Text = "--All queries started or finished during the execution of the quer" + (rows.Count > 1 ? "ies" : "y" ) + " at row" + (rows.Count > 1 ? "s " : " ") + String.Join(", ", rows.ToArray()) + ".\r\n\r\n" + strQry + "\r\norder by duration desc, starttime desc";
                if (txtProfilerAnalysisQuery.Text != "")
                {
                    BackgroundWorker bgLoadProfilerAnalysis = new BackgroundWorker();
                    bgLoadProfilerAnalysis.DoWork += BgLoadProfilerAnalysis_DoWork;
                    bgLoadProfilerAnalysis.RunWorkerCompleted += BgLoadProfilerAnalysis_RunWorkerCompleted;
                    StatusFloater.lblStatus.Text = "Running analysis query...";
                    StatusFloater.Left = this.Left + this.Width / 2 - StatusFloater.Width / 2;
                    StatusFloater.Top = this.Top + this.Height / 2 - StatusFloater.Height / 2;
                    StatusFloater.Show(this);
                    this.SuspendLayout();
                    bgLoadProfilerAnalysis.RunWorkerAsync();
                }
                else
                {
                    dgdProfilerAnalyses.DataSource = null;
                    dgdProfilerAnalyses.Refresh();
                }
            }
            else if ((sender as MenuItem).Text == "Lookup query statistics for selected queries")
            {
                foreach (int row in rows)
                {
                    string strBase = Properties.Resources.QueryFESEStats.Replace("[Table", "[" + AnalysisTraceID).Replace("ORDER BY QueryDuration", "") + "WHERE StartRow = " + row + " OR EndRow = " + row;
                    strQry += (strQry == "" ? strBase : "\r\nunion\r\n" + strBase);
                }
                txtProfilerAnalysisQuery.Text = strQry;
                if (txtProfilerAnalysisQuery.Text != "")
                {
                    BackgroundWorker bgLoadProfilerAnalysis = new BackgroundWorker();
                    bgLoadProfilerAnalysis.DoWork += BgLoadProfilerAnalysis_DoWork;
                    bgLoadProfilerAnalysis.RunWorkerCompleted += BgLoadProfilerAnalysis_RunWorkerCompleted;
                    StatusFloater.lblStatus.Text = "Running analysis query...";
                    StatusFloater.Left = this.Left + this.Width / 2 - StatusFloater.Width / 2;
                    StatusFloater.Top = this.Top + this.Height / 2 - StatusFloater.Height / 2;
                    StatusFloater.Show(this);
                    this.SuspendLayout();
                    bgLoadProfilerAnalysis.RunWorkerAsync();
                }
                else
                {
                    dgdProfilerAnalyses.DataSource = null;
                    dgdProfilerAnalyses.Refresh();
                }
            }
            else if ((sender as MenuItem).Text == "Lookup detail rows for selected queries/commands")
            {
                foreach (int row in rows)
                {
                    string strBase = "";
                    if (!bProfilerQueryStatsPresent)
                        strBase = ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.DrillThroughQueryAllRowsForQueryOrCommand).Replace("<RowNumber/>", Convert.ToString(row));
                    else
                        strBase = Properties.Resources.DrillThroughQueryAllRowsForQueryOrCommand.Replace("[Table", "[" + AnalysisTraceID).Replace("<RowNumber/>", Convert.ToString(row));
                    strQry += (strQry == "" ? strBase : "\r\nunion\r\n" + strBase);
                }
                txtProfilerAnalysisQuery.Text = strQry + "\r\norder by RowNumber";
                if (txtProfilerAnalysisQuery.Text != "")
                {
                    BackgroundWorker bgLoadProfilerAnalysis = new BackgroundWorker();
                    bgLoadProfilerAnalysis.DoWork += BgLoadProfilerAnalysis_DoWork;
                    bgLoadProfilerAnalysis.RunWorkerCompleted += BgLoadProfilerAnalysis_RunWorkerCompleted;
                    StatusFloater.lblStatus.Text = "Running analysis query...";
                    StatusFloater.Left = this.Left + this.Width / 2 - StatusFloater.Width / 2;
                    StatusFloater.Top = this.Top + this.Height / 2 - StatusFloater.Height / 2;
                    StatusFloater.Show(this);
                    this.SuspendLayout();
                    bgLoadProfilerAnalysis.RunWorkerAsync();
                }
                else
                {
                    dgdProfilerAnalyses.DataSource = null;
                    dgdProfilerAnalyses.Refresh();
                }
            }
        }
        #endregion Profiler Trace Analysis       

        #endregion AnalysisUI
    }
}