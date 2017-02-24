using Microsoft.AnalysisServices;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Data.SqlClient;
using System.Configuration;

namespace SSASDiag
{
    public partial class frmSSASDiag : Form
    {
        #region AnalysisUI

        #region AnalysisLocals
        System.Windows.Forms.Timer AnalysisMessagePumpTimer = new System.Windows.Forms.Timer();
        bool bProfilerEventClassSublcassViewPresent = false, bProfilerQueryStatsPresent = false;
        #endregion AnalysisLocals

        private void tcCollectionAnalysisTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcCollectionAnalysisTabs.SelectedIndex == 1 && tbAnalysis.Enabled)
            {
                if (dc != null)
                {
                    AnalysisTraceID = dc.TraceID;
                    dgdProfilerAnalyses.DataSource = null;
                    dgdProfilerAnalyses.Columns.Clear();
                    dgdProfilerAnalyses.Refresh();
                    cmbProfilerAnalyses.Visible = txtProfilerAnalysisQuery.Visible = false;
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
                if (tcAnalysis.SelectedTab != null && tcAnalysis.SelectedTab.Name == "tbProfilerTraces")
                {
                    tcAnalysis_SelectedIndexChanged(sender, e);
                    dgdProfilerAnalyses.DataSource = null;
                    dgdProfilerAnalyses.Columns.Clear();
                    dgdProfilerAnalyses.Refresh();
                    cmbProfilerAnalyses.Visible = txtProfilerAnalysisQuery.Visible = false;
                }
            }
            if (tcCollectionAnalysisTabs.SelectedIndex == 0)
            {
                if(bProfilerTraceDbAttached && chkDettachProfilerAnalysisDBWhenDone.Checked && connSqlDb.State == ConnectionState.Open)
                    new Thread(new ThreadStart(new System.Action(() => DettachProfilerTraceDB()))).Start();
            }
        }
        private void tcAnalysis_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcAnalysis.SelectedTab != null)
            {
                if (tcAnalysis.SelectedTab.Name == "tbProfilerTraces" && !bProfilerTraceDbAttached && chkDettachProfilerAnalysisDBWhenDone.Checked && !btnImportProfilerTrace.Visible)
                {
                    new Thread(new ThreadStart(new System.Action(() => AttachProfilerTraceDB()))).Start();
                }
                else if (bProfilerTraceDbAttached && chkDettachProfilerAnalysisDBWhenDone.Checked && connSqlDb.State == ConnectionState.Open)
                    new Thread(new ThreadStart(new System.Action(() => DettachProfilerTraceDB()))).Start();
                else if (bProfilerTraceDbAttached && !chkDettachProfilerAnalysisDBWhenDone.Checked && connSqlDb.State == ConnectionState.Open)
                    bProfilerTraceDbAttached = cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = true;
                else
                    bProfilerTraceDbAttached = cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = false;
            }
        }
        private void btnAnalysisFolder_Click(object sender, EventArgs e)
        {
            BrowseForFolder bff = new BrowseForFolder();
            bff.Filters.Add("zip");
            string strPath = Environment.CurrentDirectory as string;
            strPath = bff.SelectFolder("Choose a folder or zip file for analysis:", txtFolderZipForAnalysis.Text == "" ? strPath : txtFolderZipForAnalysis.Text, this.Handle);
            if (strPath != null && strPath != "")
            {
                txtFolderZipForAnalysis.Text = m_analysisPath = strPath;
                PopulateAnalysisTabs();
            }
            AnalysisMessagePumpTimer.Interval = 1000;
            AnalysisMessagePumpTimer.Tick += AnalysisMessagePumpTimer_Tick;
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
                        System.Diagnostics.Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
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
            m_analysisPath = Environment.CurrentDirectory + "\\" + m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace(".zip", "");
            AnalysisTraceID = m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace("_SSASDiagOutput", "_SSASDiag");

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
                    System.Diagnostics.Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
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
        private void PopulateAnalysisTabs()
        {
            AnalysisTraceID = m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace("_SSASDiagOutput", "_SSASDiag");
            foreach (TabPage t in tcAnalysis.TabPages)
                HiddenTabPages.Add(t);
            tcAnalysis.TabPages.Clear();
            lblAnalysisQueries.Visible = cmbProfilerAnalyses.Visible = txtProfilerAnalysisQuery.Visible = false;
            btnImportProfilerTrace.Visible = true;

            if (m_analysisPath != null)
            {
                if (m_analysisPath.EndsWith(".zip"))
                    SelectivelyExtractAnalysisDataFromZip();

                if (File.Exists(m_analysisPath + "\\msmdsrv.ini"))
                {
                    tcAnalysis.TabPages.Add(new TabPage("Configuration") { ImageIndex = 0, Name = "Configuration" });
                    tcAnalysis.TabPages["Configuration"].Controls.Add(GetStatusTextBox("Check back soon for automated analysis of configuration details."));
                }
                if (Directory.GetFiles(m_analysisPath, "*.mdmp", SearchOption.AllDirectories).Count() > 0)
                {
                    tcAnalysis.TabPages.Add(new TabPage("Crash Dumps") { ImageIndex = 1, Name = "Crash Dumps" });
                    tcAnalysis.TabPages["Crash Dumps"].Controls.Add(GetStatusTextBox("Check back soon for automated analysis of crash dumps."));
                }
                if (File.Exists(m_analysisPath + "\\" + AnalysisTraceID + "_Application.evtx") ||
                    File.Exists(m_analysisPath + "\\" + AnalysisTraceID + "_System.evtx"))
                {
                    tcAnalysis.TabPages.Add(new TabPage("Event Logs") { ImageIndex = 2, Name = "Event Logs" });
                    tcAnalysis.TabPages["Event Logs"].Controls.Add(GetStatusTextBox("Check back soon for automated analysis of event logs."));
                }
                if (File.Exists(m_analysisPath + "\\" + AnalysisTraceID + ".etl") || File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.diag.log"))
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
                if (File.Exists(m_analysisPath + "\\" + AnalysisTraceID + ".blg"))
                {
                    tcAnalysis.TabPages.Add(new TabPage("Performance Logs") { ImageIndex = 4, Name = "Performance Logs" });
                    tcAnalysis.TabPages["Performance Logs"].Controls.Add(GetStatusTextBox("Check back soon for automated analysis of performance logs."));
                }
                if (Directory.GetFiles(m_analysisPath, AnalysisTraceID + "*.trc").Count() > 0 || File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf"))
                {
                    btnImportProfilerTrace.Visible = true;
                    tcAnalysis.TabPages.Add(HiddenTabPages.Where(t => t.Text == "Profiler Traces").First());
                    HiddenTabPages.Remove(HiddenTabPages.Where(t => t.Text == "Profiler Traces").First());
                    string sqlForTraces = Properties.Settings.Default["SqlForProfilerTraceAnalysis"] as string;

                    if (File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf"))
                    {
                        AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf");
                        AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf");
                        ProfilerTraceStatusTextBox.AppendText("Using trace data loaded into SQL .mdf at " + m_analysisPath + "\\Analysis\\.\r\n");
                        cmbProfilerAnalyses.Visible = txtProfilerAnalysisQuery.Visible = lblAnalysisQueries.Visible = cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = true;
                        btnImportProfilerTrace.Visible = false;
                    }
                    else
                    {
                        ProfilerTraceStatusTextBox.Text = "Trace file is not yet imported to database table for analysis.  Import to perform analysis.";
                    }
                }
            }
        }
        private void btnAnalyzeNetworkTrace_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(m_analysisPath))
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
            public ProfilerTraceQuery(string key, string value, ProfilerQueryTypes queryType)
            { Key = key; Value = value; QueryType = queryType; }
            public ProfilerTraceQuery(ProfilerTraceQuery p, ProfilerQueryTypes t)
            {
                Key = p.Key;
                Value = p.Value;
                QueryType = t;
            }
            public string Key { get; set; }
            public string Value { get; set; }
            public ProfilerQueryTypes QueryType { get; set; }
        }
        private string ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(string qry)
        {
            return qry.Replace("[Table_v]", "[Table]").Replace("EventClassName, ", "").Replace("EventSubclassName, ", "").Replace("EventClassName", "").Replace("EventSubclassName", "");
        }
        private List<ProfilerTraceQuery> InitializeProfilerTraceAnalysisQueries()
        {
            List<ProfilerTraceQuery> q = new List<ProfilerTraceQuery>();
            q.Add(new ProfilerTraceQuery("", "", ProfilerQueryTypes.BaseQuery));

            // Basic details
            q.Add(new ProfilerTraceQuery("Basic trace summary",
                                         Properties.Resources.QueryBasicTraceSummary, ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));

            // Query FE/SE Stats
            q.Add(new ProfilerTraceQuery("Formula/Storage engine statistics",
                                         Properties.Resources.QueryFESEStats, ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.AllQueries));

            // Longest running queries
            q.Add(new ProfilerTraceQuery("Longest running queries captured",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryLongestRunningQueries), ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Longest running queries captured",
                                         Properties.Resources.QueryLongestRunningQueries, ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));


            // Longest running queries
            q.Add(new ProfilerTraceQuery("Longest running commands captured",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryLongestRunningCommands), ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Longest running commands captured",
                                         Properties.Resources.QueryLongestRunningCommands, ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));


            // Most collectively expensive events
            q.Add(new ProfilerTraceQuery("Most collectively expensive events",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryMostCollectivelyExpensiveEvents), ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Most collectively expensive events",
                                         Properties.Resources.QueryMostCollectivelyExpensiveEvents, ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Most collectively expensive queries
            q.Add(new ProfilerTraceQuery("Most collectively expensive queries",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryMostCollectivelyExpensiveQueries), ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Most collectively expensive queries",
                                         Properties.Resources.QueryMostCollectivelyExpensiveQueries, ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Most collectively expensive commands
            q.Add(new ProfilerTraceQuery("Most collectively expensive commands",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryMostCollectivelyExpensiveCommands), ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Most collectively expensive commands",
                                         Properties.Resources.QueryMostCollectivelyExpensiveCommands, ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Errors
            q.Add(new ProfilerTraceQuery("Queries/commands with errors",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryQueriesCommandsWithErrors), ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Queries/commands with errors",
                                         Properties.Resources.QueryQueriesCommandsWithErrors, ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Most impactful queries/commands
            q.Add(new ProfilerTraceQuery("Most impactful queries/commands",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryMostImpactfulQueriesCommands), ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Most impactful queries/commands",
                                         Properties.Resources.QueryMostImpactfulQueriesCommands, ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Queries/Commands not completed during trace
            q.Add(new ProfilerTraceQuery("Queries/commands not completed",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryQueriesCommandsNotCompleted), ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Queries/commands not completed",
                                         Properties.Resources.QueryQueriesCommandsNotCompleted, ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Possible runaway sessions
            q.Add(new ProfilerTraceQuery("Possible runaway sessions",
                                         Properties.Resources.QueryPossibleRunawaySessions,
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));

            return q;
        }
        private void btnImportProfilerTrace_Click(object sender, EventArgs e)
        {
            btnImportProfilerTrace.Enabled = false;
            if (!ValidateProfilerTraceDBConnectionStatus())
            {
                btnImportProfilerTrace.Enabled = true;
                return;
            }
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
                System.Diagnostics.Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
                if (ex.Message == "ExecuteNonQuery requires an open and available Connection. The connection's current state is closed.")
                {
                    ProfilerTraceStatusTextBox.Text = "Trace file is not yet imported to database table for analysis.  No SQL Server was available to perform import";
                    btnImportProfilerTrace.Visible = true;
                }
                else
                    ProfilerTraceStatusTextBox.Text = "Error loading profiler trace: \r\n" + ex.Message;
            }
        }
        private void bgImportProfilerTrace(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (!Directory.Exists(m_analysisPath))
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
                //AddItemToStatus("Added full control for SSAS service account " + sSvcUser + " to the output directory.");

                if (AnalysisTraceID == "")
                    AnalysisTraceID = m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace("_SSASDiagOutput", "_SSASDiag");
                SqlCommand cmd = new SqlCommand(Properties.Resources.CreateDBSQLScript.
                                    Replace("<mdfpath/>", m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf").
                                    Replace("<ldfpath/>", m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf").
                                    Replace("<dbname/>", AnalysisTraceID)
                                    , connSqlDb);
                int ret = cmd.ExecuteNonQuery();

                TextBox ProfilerTraceStatusTextBox = (e.Argument as object[])[0] as TextBox;
                ProfilerTraceStatusTextBox.Invoke(new System.Action(()=>
                    ProfilerTraceStatusTextBox.Text = "Importing profiler trace to database [" + AnalysisTraceID + "] on SQL instance: [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]."));
                string connstr = (e.Argument as object[])[1] as string;
                if (!connstr.Contains("Initial Catalog")) connstr += (connstr.EndsWith(";") ? "" : ";") + "Initial Catalog=" + AnalysisTraceID + ";";
                if (connstr.Contains("Initial Catalog=;")) connstr = connstr.Replace("Initial Catalog=;", "Initial Catalog=" + AnalysisTraceID + ";");

                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\ASProfilerTraceImporterCmd.exe";
                p.StartInfo.Arguments = "\"" + Directory.GetFiles(m_analysisPath, AnalysisTraceID + "*.trc")[0] + "\" \"" + connstr + "\" \"" + AnalysisTraceID + "\"";
                p.Start();
                while (!p.HasExited)
                {
                    string sOut = p.StandardOutput.ReadLine();
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
                        else if (sOut == "Database prepared for analysis.")
                            break;
                        else
                            ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text == "" ? "" : "\r\n") + sOut)));
                }
                System.Diagnostics.Trace.WriteLine(p.HasExited);
                ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Adding profiler database to collection data...")));
                DettachProfilerTraceDB();
                AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf");
                AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf");
                if (txtFolderZipForAnalysis.Text.EndsWith(".zip"))
                {
                    ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText("\r\nDeleting redundant profiler trace files from data extraction location...")));
                    foreach (string file in Directory.EnumerateFiles(m_analysisPath, AnalysisTraceID + "*.trc"))
                        File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
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
            if (File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf"))
            {
                btnImportProfilerTrace.Visible = false;
                cmbProfilerAnalyses.Visible = true;
                txtProfilerAnalysisQuery.Visible = true;
                tcAnalysis_SelectedIndexChanged(sender, e);
            }
            btnImportProfilerTrace.Enabled = true;
            AnalysisMessagePumpTimer.Stop();
        }
        private void cmbProfilerAnalyses_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtProfilerAnalysisQuery.Text = (cmbProfilerAnalyses.DataSource as List<ProfilerTraceQuery>).First(kv => kv.Key == cmbProfilerAnalyses.Text).Value.Replace("[Table", "[" + AnalysisTraceID);
            
            if (txtProfilerAnalysisQuery.Text != "")
            {
                BackgroundWorker bgLoadProfilerAnalysis = new BackgroundWorker();
                bgLoadProfilerAnalysis.DoWork += BgLoadProfilerAnalysis_DoWork;
                bgLoadProfilerAnalysis.RunWorkerCompleted += BgLoadProfilerAnalysis_RunWorkerCompleted;
                StatusFloater.lblStatus.Text = "Running analysis query...";
                StatusFloater.Left = this.Left + this.Width / 2 - StatusFloater.Width / 2;
                StatusFloater.Top = this.Top + this.Height / 2 - StatusFloater.Height / 2;
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
        private void BgLoadProfilerAnalysis_DoWork(object sender, DoWorkEventArgs e)
        {
            connSqlDb.ChangeDatabase(AnalysisTraceID);
            SqlCommand cmd = new SqlCommand(txtProfilerAnalysisQuery.Text, connSqlDb);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dgdProfilerAnalyses.Invoke(new System.Action(() =>
                {
                    dgdProfilerAnalyses.AutoGenerateColumns = true;
                    dgdProfilerAnalyses.DataSource = null;
                    dgdProfilerAnalyses.Columns.Clear();
                    dgdProfilerAnalyses.DataSource = dt;
                    dgdProfilerAnalyses.Refresh();
                }));
        }
        private void BgLoadProfilerAnalysis_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Invoke(new System.Action(() =>
                {
                    StatusFloater.Visible = false;
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
                        System.Diagnostics.Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
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
                        catch (Exception ex) { System.Diagnostics.Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace); }
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
            if (connSqlDb.State == ConnectionState.Open)
            {
                connSqlDb.ChangeDatabase(AnalysisTraceID);
                SqlCommand cmd = new SqlCommand("SELECT TOP 1 name FROM sys.views WHERE name = N'" + AnalysisTraceID + "_v'", connSqlDb);
                if (cmd.ExecuteScalar() != null)
                    bProfilerEventClassSublcassViewPresent = true;
                else
                    bProfilerEventClassSublcassViewPresent = false;
                ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Confirmed event class/subclass view is " + (bProfilerEventClassSublcassViewPresent ? "present.\r\n" : "not present.\r\n"))));
                cmd.CommandText = "SELECT TOP 1 name FROM sys.views WHERE name = N'" + AnalysisTraceID + "_QueryStats'";
                if (cmd.ExecuteScalar() != null)
                    bProfilerQueryStatsPresent = true;
                else
                    bProfilerQueryStatsPresent = false;
                Invoke(new System.Action(() =>
                    {
                        ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Confirmed query statistics view is " + (bProfilerQueryStatsPresent ? "present.\r\n" : "not present.\r\n"))));
                        cmbProfilerAnalyses.DataSource = ProfilerTraceAnalysisQueries.Where(q => q.QueryType == (bProfilerEventClassSublcassViewPresent && bProfilerQueryStatsPresent ? ProfilerQueryTypes.AllQueries :
                                                                                                            bProfilerEventClassSublcassViewPresent ? ProfilerQueryTypes.QueriesWithEventClassSubclassNames :
                                                                                                            bProfilerQueryStatsPresent ? ProfilerQueryTypes.QueriesWithQueryStats :
                                                                                                            ProfilerQueryTypes.BaseQuery)
                                                                                                 || q.Key == "").ToList();
                        cmbProfilerAnalyses.Refresh();
                    }));
            }
        }
        private void AttachProfilerTraceDB()
        {
            tcAnalysis.Invoke(new System.Action(()=>tcAnalysis.SelectedTab.Controls["ProfilerTraceStatusTextbox"].Text += (ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Attaching profiler trace database...\r\n"));
            ValidateProfilerTraceDBConnectionStatus();

            if (connSqlDb.State == ConnectionState.Open)
            {
                connSqlDb.ChangeDatabase("master");
                SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') ALTER DATABASE [" + AnalysisTraceID + "] SET MULTI_USER", connSqlDb);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand("IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') CREATE DATABASE [" + AnalysisTraceID + "] ON (FILENAME = N'" + m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf'),"
                                                + "(FILENAME = N'" + m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf') "
                                                + "FOR ATTACH", connSqlDb);
                try
                {
                    cmd.ExecuteNonQuery();
                    bProfilerTraceDbAttached = true;
                    Invoke(new System.Action(() =>
                        {
                            lblAnalysisQueries.Visible = cmbProfilerAnalyses.Visible = cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = txtProfilerAnalysisQuery.Visible = true;
                            ttStatus.SetToolTip(chkDettachProfilerAnalysisDBWhenDone, "Profiler traces were imported into a trace database in the file:\r\n" + AnalysisTraceID
                                                                                + ".mdf\r\n\r\nLocated at:\r\n" + m_analysisPath + "\\Analysis\r\n\r\n"
                                                                                + "Uncheck this checkbox if the scenario requires further analysis.\r\n\r\n"
                                                                                + "Note:  While attached the SQL data source at [" + connSqlDb.DataSource + "] locks these files from deletion while started.");
                            ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Attached trace database [" + AnalysisTraceID + "]\r\nto SQL instance [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]\r\nfor analysis at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
                        }));

                    ValidateProfilerTraceViews();
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
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
                                            lblAnalysisQueries.Visible = cmbProfilerAnalyses.Visible = cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = txtProfilerAnalysisQuery.Visible = true;
                                            ttStatus.SetToolTip(chkDettachProfilerAnalysisDBWhenDone, "Profiler traces were imported into a trace database in the file:\r\n" + AnalysisTraceID
                                                                            + ".mdf\r\n\r\nLocated at:\r\n" + m_analysisPath + "\\Analysis\r\n\r\n"
                                                                            + "Uncheck this checkbox if the scenario requires further analysis.\r\n\r\n"
                                                                            + "Note:  While attached the SQL data source at [" + connSqlDb.DataSource + "] locks these files from deletion while started.");
                                            ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Attached trace database [" + AnalysisTraceID + "]\r\nto SQL instance [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]\r\nfor analysis at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
                                        }));
                                    ValidateProfilerTraceViews();
                                    break;
                                }
                                catch
                                {
                                    System.Diagnostics.Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
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
        }
        private void DettachProfilerTraceDB()
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
                Invoke(new System.Action(() =>
                    {
                        lblAnalysisQueries.Visible = cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = false;
                        ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Detached trace database [" + AnalysisTraceID + "]\r\nfrom SQL instance [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]\r\nat " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
                    }));
            }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace); }  // could fail if service stopped, no biggie just move on...
        }
        private void dgdProfilerAnalyses_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.ColumnIndex != -1 && e.RowIndex != -1)
                {
                    ContextMenu m = new ContextMenu();
                    m.MenuItems.Add(new MenuItem("Copy", ProfilerAnalysisContextMenu_Click));
                    m.MenuItems.Add(new MenuItem("Copy with Headers", ProfilerAnalysisContextMenu_Click));
                    
                    if (dgdProfilerAnalyses.SelectedCells.Count == 0)
                        dgdProfilerAnalyses.CurrentCell = dgdProfilerAnalyses[e.ColumnIndex, e.RowIndex];

                    if (cmbProfilerAnalyses.Text == "Longest running queries captured" ||
                        cmbProfilerAnalyses.Text == "Longest running commands captured" ||
                        cmbProfilerAnalyses.Text == "Most collectively expensive queries" ||
                        cmbProfilerAnalyses.Text == "Most collectively expensive commands" ||
                        cmbProfilerAnalyses.Text == "Queries/commands with errors" ||
                        cmbProfilerAnalyses.Text == "Most impactful queries/commands" ||
                        cmbProfilerAnalyses.Text == "Queries/commands not completed" ||
                        cmbProfilerAnalyses.Text == "Formula/Storage engine statistics"
                        )
                    {
                        m.MenuItems.Add(new MenuItem("-"));
                        m.MenuItems.Add(new MenuItem(string.Format("Find all queries/commands overlapping with selection", e.RowIndex), ProfilerAnalysisContextMenu_Click));
                    }

                    Rectangle rect = dgdProfilerAnalyses.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    m.Show(dgdProfilerAnalyses, new Point(rect.X, rect.Y));
                }
            }
        }
        private void ProfilerAnalysisContextMenu_Click(object sender, EventArgs e)
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
            }
            if ((sender as MenuItem).Text == "Find all queries/commands overlapping with selection")
            {
                string strQry = "";
                //string rows = "";
                List<int?> rows = new List<int?>();
                foreach (DataGridViewCell c in dgdProfilerAnalyses.SelectedCells)
                {
                    if (dgdProfilerAnalyses.Columns.Contains("RowNumber"))
                        rows.Add(dgdProfilerAnalyses.Rows[c.RowIndex].Cells["RowNumber"].Value as int?);
                    else if (dgdProfilerAnalyses.Columns.Contains("EndRow") && dgdProfilerAnalyses.Rows[c.RowIndex].Cells["EndRow"].Value != null)
                        rows.Add(dgdProfilerAnalyses.Rows[c.RowIndex].Cells["EndRow"].Value as int?);
                    else if (dgdProfilerAnalyses.Columns.Contains("StartRow"))
                        rows.Add(dgdProfilerAnalyses.Rows[c.RowIndex].Cells["StartRow"].Value as int?);

                }
                cmbProfilerAnalyses.SelectedIndex = 0;
                rows = rows.Distinct().ToList();
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
            
        }
        #endregion Profiler Trace Analysis       

        #endregion AnalysisUI
    }
}