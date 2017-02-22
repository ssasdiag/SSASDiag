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
                    tcAnalysis_SelectedIndexChanged(sender, e);
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
        private List<ProfilerTraceQuery> InitializeProfilerTraceAnalysisQueries()
        {
            List<ProfilerTraceQuery> q = new List<ProfilerTraceQuery>();
            q.Add(new ProfilerTraceQuery("", "", ProfilerQueryTypes.BaseQuery));

            // Basic details
            q.Add(new ProfilerTraceQuery("Basic trace summary",
                                         "SELECT * FROM\r\n(SELECT COUNT(*)[Queries Started] FROM [Table] WHERE EventClass = 9) a,\r\n(SELECT COUNT(*)[Queries Completed] FROM [Table] WHERE EventClass = 10) b,\r\n(SELECT SUM(Duration)[Total Query Duration], AVG(Duration)[Average Query Duration] FROM [Table] WHERE EventClass = 10) e,\r\n(SELECT(\r\n(SELECT MAX(Duration) FROM\r\n\t(SELECT TOP 50 PERCENT Duration FROM [Table] WHERE EventClass = 10 AND Duration > 0 ORDER BY Duration) AS BottomHalf)\r\n\t+\r\n\t(SELECT MIN(Duration) FROM\r\n\t(SELECT TOP 50 PERCENT Duration FROM [Table] WHERE EventClass = 10 AND Duration > 0 ORDER BY Duration DESC) AS TopHalf)\r\n)\t / 2 AS [Median Query Duration]) g,\r\n(SELECT COUNT(*)[Commands Started] FROM [Table] WHERE EventClass = 15) c,\r\n(SELECT COUNT(*)[Commands Completed] FROM [Table] WHERE EventClass = 16) d,\r\n(SELECT SUM(Duration) [Total Command Duration], AVG(Duration) [Average Command Duration] FROM [Table] WHERE EventClass = 16) f,\r\n(SELECT(\r\n\t(SELECT MAX(Duration) FROM\r\n\t(SELECT TOP 50 PERCENT Duration FROM [Table] WHERE EventClass = 16 AND Duration > 0 ORDER BY Duration) AS BottomHalf)\r\n\t+\r\n\t(SELECT MIN(Duration) FROM\r\n\t(SELECT TOP 50 PERCENT Duration FROM [Table] WHERE EventClass = 16 AND Duration > 0 ORDER BY Duration DESC) AS TopHalf)\r\n\t) / 2 AS [Median Command Duration]) h", ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));

            // Query FE/SE Stats
            q.Add(new ProfilerTraceQuery("Formula/Storage engine statistics",
                                         "SELECT QueryDuration, StorageEngineTime, QueryDuration - StorageEngineTime FormulaEngineTime, SEPct, 100 - SEPct FEPct, StartRow, EndRow, StartTime, EndTime, ConnectionID, DatabaseName, TextData, RequestParameters, RequestProperties, SPID, NTUserName, NTDomainName\r\nFROM [Table_QueryStats]\r\nORDER BY QueryDuration",
                                         ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.AllQueries));

            // Longest running queries
            q.Add(new ProfilerTraceQuery("Longest running queries captured",
                                         "SELECT TOP 100 Duration, CPUTime, StartTime, CurrentTime as EndTime, DatabaseName, TextData, NTUserName, NTDomainName, ApplicationName, ClientProcessID, SPID, RequestParameters, RequestProperties\r\nFROM [Table]\r\nWHERE EventClass = 10\r\nORDER BY Duration DESC",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Longest running queries captured",
                                         "SELECT TOP 100 Duration, CPUTime, StartTime, CurrentTime as EndTime, DatabaseName, TextData, NTUserName, NTDomainName, ApplicationName, ClientProcessID, SPID, RequestParameters, RequestProperties\r\nFROM [Table_v]\r\nWHERE EventClass = 10\r\nORDER BY Duration DESC", ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            

            // Longest running commands
            q.Add(new ProfilerTraceQuery("Longest running commands captured",
                                         "SELECT TOP 100 Duration, CPUTime, StartTime, CurrentTime as EndTime, DatabaseName, TextData, NTUserName, NTDomainName, ApplicationName, ClientProcessID, SPID, RequestParameters, RequestProperties\r\nFROM [Table]\r\nWHERE EventClass = 16\r\nORDER BY Duration DESC",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Longest running commands captured",
                                         "SELECT TOP 100 Duration, CPUTime, StartTime, CurrentTime as EndTime, DatabaseName, TextData, NTUserName, NTDomainName, ApplicationName, ClientProcessID, SPID, RequestParameters, RequestProperties\r\nFROM [Table_v]\r\nWHERE EventClass = 16\r\nORDER BY Duration DESC", ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            

            // Most collectively expensive events
            q.Add(new ProfilerTraceQuery("Most collectively expensive events",
                                         "SELECT TOP 100 COUNT(*) as ExecutionCount, SUM(Duration) TotalDuration, EventClass, EventSubclass, TextData\r\nFROM [Table]\r\nWHERE EventClass <> 42-- skip ExistingSession durations\r\nGROUP BY TextData, EventClass, EventSubclass\r\nHAVING SUM(Duration) > 0\r\nORDER BY SUM(Duration) DESC",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Most collectively expensive events",
                                         "SELECT TOP 100 COUNT(*) as ExecutionCount, SUM(Duration) TotalDuration, EventClass, EventClassName, EventSubclass, EventSubclassName, TextData\r\nFROM [Table_v]\r\nWHERE EventClass <> 42-- skip ExistingSession durations\r\nGROUP BY TextData, EventClass, EventClassName, EventSubclass, EventSubclassName\r\nHAVING SUM(Duration) > 0\r\nORDER BY SUM(Duration) DESC",
                                         ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.AllQueries));

            // Most collectively expensive queries
            q.Add(new ProfilerTraceQuery("Most collectively expensive queries",
                                         "SELECT TOP 100 COUNT(*) as ExecutionCount, SUM(Duration) TotalDuration, EventClass, EventSubclass, TextData\r\nFROM [Table]\r\nWHERE EventClass = 10\r\nGROUP BY TextData, EventClass, EventSubclass\r\nHAVING SUM(Duration) > 0\r\nORDER BY SUM(Duration) DESC",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Most collectively expensive queries",
                                         "SELECT TOP 100 COUNT(*) as ExecutionCount, SUM(Duration) TotalDuration, EventClass, EventClassName, EventSubclass, EventSubclassName, TextData\r\nFROM [Table_v]\r\nWHERE EventClass = 10\r\nGROUP BY TextData, EventClass, EventClassName, EventSubclass, EventSubclassName\r\nHAVING SUM(Duration) > 0\r\nORDER BY SUM(Duration) DESC",
                                         ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.AllQueries));

            // Most collectively expensive commands
            q.Add(new ProfilerTraceQuery("Most collectively expensive commands",
                                         "SELECT TOP 100 COUNT(*) as ExecutionCount, SUM(Duration) TotalDuration, EventClass, EventSubclass, TextData\r\nFROM [Table]\r\nWHERE EventClass = 16\r\nGROUP BY TextData, EventClass, EventSubclass\r\nHAVING SUM(Duration) > 0\r\nORDER BY SUM(Duration) DESC",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Most collectively expensive commands",
                                         "SELECT TOP 100 COUNT(*) as ExecutionCount, SUM(Duration) TotalDuration, EventClass, EventClassName, EventSubclass, EventSubclassName, TextData\r\nFROM [Table_v]\r\nWHERE EventClass = 16\r\nGROUP BY TextData, EventClass, EventClassName, EventSubclass, EventSubclassName\r\nHAVING SUM(Duration) > 0\r\nORDER BY SUM(Duration) DESC",
                                         ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.AllQueries));

            // Errors
            q.Add(new ProfilerTraceQuery("Queries/commands with errors",
                                         "SELECT TextData, sys.fn_varbintohexstr(CONVERT(VARBINARY(8), Error)) [Error Code], DatabaseName, EventClass, NTUserName, NTDomainName, a.ConnectionID, ClientProcessID, SPID, ApplicationName\r\nFROM [Table] a,\r\n\t(\r\n\t\tSELECT RowNumber, ConnectionID, StartTime\r\n\t\tFROM [Table]\r\n\t\tWHERE EventClass = 17\r\n\t) b\r\nWHERE EventClass in (9, 10, 15, 16, 17) AND\r\n\ta.ConnectionID = b.ConnectionID AND\r\n\ta.StartTime >= b.StartTime AND\r\n\ta.RowNumber <= b.RowNumber",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));
            q.Add(new ProfilerTraceQuery("Queries/commands with errors",
                                         "SELECT TextData, sys.fn_varbintohexstr(CONVERT(VARBINARY(8), Error)) [Error Code], DatabaseName, EventClass, EventClassName, NTUserName, NTDomainName, a.ConnectionID, ClientProcessID, SPID, ApplicationName\r\nFROM [Table_v] a,\r\n\t(\r\n\t\tSELECT RowNumber, ConnectionID, StartTime\r\n\t\tFROM [Table]\r\n\t\tWHERE EventClass = 17\r\n\t) b\r\nWHERE EventClass in (9, 10, 15, 16, 17) AND\r\n\ta.ConnectionID = b.ConnectionID AND\r\n\ta.StartTime >= b.StartTime AND\r\n\ta.RowNumber <= b.RowNumber",
                                         ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.AllQueries));

            // Most impactful queries/commands
            q.Add(new ProfilerTraceQuery("Most impactful queries/commands",
                                         "-- Returns a ranking of all queries and commands in the trace by the number of other queries and commands that overlap the query or command in question.\r\n-- The highest ranked among these may be likely offending queries or commands that affected other queries or commands on the server.\r\n\r\nselect top 100 b.[Overlapping Query Count], a.RowNumber, EventClass, EventClassName, StartTime, CurrentTime EndTime, Duration, CPUTime, ConnectionID, NTUserName, NTDomainName, DatabaseName, TextData, ClientProcessID, ApplicationName from [Table_v] a,\r\n(\r\n\tselect count(*)[Overlapping Query Count], OriginalQueryConnection, OriginalQueryEndRow from\r\n\t(\r\n\t\tselect a.StartTime, a.CurrentTime EndTime, a.ConnectionID,\r\n\t\tb.RowNumber OriginalQueryEndRow, b.StartTime OriginalQueryStart, b.CurrentTime OriginalQueryEnd, b.ConnectionID OriginalQueryConnection\r\n\t\tfrom [Table] a,\r\n\t\t(\r\n\t\t\tselect RowNumber, ConnectionID, StartTime, CurrentTime from [Table] where eventclass in (10, 16)\r\n\t\t) b\r\n\t\twhere\r\n\t\t(\r\n\t\t\t(\r\n\t\t\t\t(a.StartTime >= b.StartTime and a.StartTime <= b.CurrentTime)\r\n\t\t\t\tor(a.CurrentTime <= b.CurrentTime and a.CurrentTime >= b.StartTime)\r\n\t\t\t\tor(a.StartTime <= b.StartTime and a.CurrentTime >= b.CurrentTime)\r\n\t\t\t)\r\n\t\t\tand a.ConnectionID<> b.ConnectionID and a.EventClass in (10, 16)\r\n\t\t)\r\n\t) OverlappingQueries\r\n\tgroup by OriginalQueryConnection, OriginalQueryEndRow\r\n) b\r\nwhere a.RowNumber = b.OriginalQueryEndRow\r\norder by[Overlapping Query Count] desc",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            q.Add(new ProfilerTraceQuery("Most impactful queries/commands",
                                         "-- Returns a ranking of all queries and commands in the trace by the number of other queries and commands that overlap the query or command in question.\r\n-- The highest ranked among these may be likely offending queries or commands that affected other queries or commands on the server.\r\n\r\nselect top 100 b.[Overlapping Query Count], a.RowNumber, EventClass, StartTime, CurrentTime EndTime, Duration, CPUTime, ConnectionID, NTUserName, NTDomainName, DatabaseName, TextData, ClientProcessID, ApplicationName from [Table] a,\r\n(\r\n\tselect count(*)[Overlapping Query Count], OriginalQueryConnection, OriginalQueryEndRow from\r\n\t(\r\n\t\tselect a.StartTime, a.CurrentTime EndTime, a.ConnectionID,\r\n\t\tb.RowNumber OriginalQueryEndRow, b.StartTime OriginalQueryStart, b.CurrentTime OriginalQueryEnd, b.ConnectionID OriginalQueryConnection\r\n\t\tfrom [Table] a,\r\n\t\t(\r\n\t\t\tselect RowNumber, ConnectionID, StartTime, CurrentTime from [Table] where eventclass in (10, 16)\r\n\t\t) b\r\n\t\twhere\r\n\t\t(\r\n\t\t\t(\r\n\t\t\t\t(a.StartTime >= b.StartTime and a.StartTime <= b.CurrentTime)\r\n\t\t\t\tor(a.CurrentTime <= b.CurrentTime and a.CurrentTime >= b.StartTime)\r\n\t\t\t\tor(a.StartTime <= b.StartTime and a.CurrentTime >= b.CurrentTime)\r\n\t\t\t)\r\n\t\t\tand a.ConnectionID<> b.ConnectionID and a.EventClass in (10, 16)\r\n\t\t)\r\n\t) OverlappingQueries\r\n\tgroup by OriginalQueryConnection, OriginalQueryEndRow\r\n) b\r\nwhere a.RowNumber = b.OriginalQueryEndRow\r\norder by[Overlapping Query Count] desc"
                                         , ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));

            // Queries/Commands not completed during trace
            q.Add(new ProfilerTraceQuery("Queries/commands not completed",
                                         "SELECT a.[Time Captured in Trace], b.RowNumber, b.EventClassName, b.EventClass, b.StartTime, b.DatabaseName, b.TextData, b.ConnectionID, b.NTUserName, b.NTDomainName, b.ClientProcessID, b.ApplicationName, b.EventSubclassName, b.EventSubclass, SPID, RequestParameters, RequestProperties\r\nFROM\r\n(\r\n\tSELECT MAX(RowNumber) RowNumber,\r\n\tDATEDIFF(\"ms\", MAX(StartTime),\r\n\t\t(\r\n\t\t\tSELECT MAX(CurrentTime)\r\n\t\t\tFROM [Table]\r\n\t\t)\r\n\t)[Time Captured in Trace]\r\n\tFROM [Table_v] a\r\n\tWHERE EventClass IN(9, 10, 15, 16) AND NOT StartTime IS NULL\r\n\tGROUP BY a.StartTime, ConnectionID, TextData\r\n\tHAVING COUNT(*) = 1\r\n) a,\r\n[Table_v] b\r\nWHERE b.RowNumber = a.RowNumber\r\nAND b.EventClass NOT IN(10, 16)\r\nORDER BY[Time Captured in Trace] DESC",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            q.Add(new ProfilerTraceQuery("Queries/commands not completed",
                                         "SELECT a.[Time Captured in Trace], b.RowNumber, b.EventClass, b.StartTime, b.DatabaseName, CONVERT(NVARCHAR(MAX), b.TextData) TextData, b.ConnectionID, b.NTUserName, b.NTDomainName, b.ClientProcessID, b.ApplicationName, b.EventSubclass, SPID, RequestParameters, RequestProperties\r\nFROM\r\n(\r\n\tSELECT MAX(RowNumber) RowNumber,\r\n\tDATEDIFF(\"ms\", MAX(StartTime),\r\n\t\t(\r\n\t\t\tSELECT MAX(CurrentTime)\r\n\t\t\tFROM [Table]\r\n\t\t)\r\n\t)[Time Captured in Trace]\r\n\tFROM [Table] a\r\n\tWHERE EventClass IN(9, 10, 15, 16) AND NOT StartTime IS NULL\r\n\tGROUP BY a.StartTime, ConnectionID, CONVERT(NVARCHAR(MAX), b.TextData)\r\n\tHAVING COUNT(*) = 1\r\n) a,\r\n[Table] b\r\nWHERE b.RowNumber = a.RowNumber\r\nAND b.EventClass NOT IN(10, 16)\r\nORDER BY[Time Captured in Trace] DESC",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithQueryStats));

            // Possible runaway sessions
            q.Add(new ProfilerTraceQuery("Possible runaway sessions",
                                         "-- These sessions were active when the trace started and never showed any begin or end events.\r\n-- This can indicate the sessions were already active and remained active without ever completing.\r\n-- If heavy load isn't otherwise detectable in a trace, check with users of possible runaway sessions.\r\n\r\nselect 'Existing Session' as EventClassName, StartTime, Duration as [Session Duration at TraceStart], ConnectionID, NTUserName, NTDomainName, DatabaseName, SPID, TextData as [Session Roles], ClientProcessID, ApplicationName, RequestProperties as [Session Properties]\r\nfrom [Table]\r\nwhere connectionid not in\r\n(\r\n\tselect connectionid from\r\n(\r\n\t\tselect connectionID, eventclass from [Table]\r\n\t\tgroup by connectionid, eventclass\r\n) a\r\n\twhere a.eventclass in (1, 2, 15, 16, 9, 10) and not ConnectionID is null\r\n)\r\nand not databasename is null\r\nand eventclass = 42\r\norder by rownumber",
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

        // For these next three functions I apologize for the heavy use of InvokeRequired and repetitive code to Invoke() or not depending on if required.  
        // These functions can be called from UI thread or not and either way need to update UI without failure so for now this is how I achieve that.
        // In the future I may work to ensure they are only ever called from off-UI thread and then remove the InvokeRequired to just always Invoke UI updates.
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
                if (InvokeRequired)
                    ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Confirmed event class/subclass view is " + (bProfilerEventClassSublcassViewPresent ? "present.\r\n" : "not present.\r\n"))));
                else
                    ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Confirmed event class/subclass view is " + (bProfilerEventClassSublcassViewPresent ? "present.\r\n" : "not present.\r\n"));
                cmd.CommandText = "SELECT TOP 1 name FROM sys.views WHERE name = N'" + AnalysisTraceID + "_QueryStats'";
                if (cmd.ExecuteScalar() != null)
                    bProfilerQueryStatsPresent = true;
                else
                    bProfilerQueryStatsPresent = false;
                if (InvokeRequired)
                {
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
                else
                {
                    ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Confirmed query statistics view is " + (bProfilerQueryStatsPresent ? "present.\r\n" : "not present.\r\n"));
                    cmbProfilerAnalyses.DataSource = ProfilerTraceAnalysisQueries.Where(q => q.QueryType == (bProfilerEventClassSublcassViewPresent && bProfilerQueryStatsPresent ? ProfilerQueryTypes.AllQueries :
                                                                                                        bProfilerEventClassSublcassViewPresent ? ProfilerQueryTypes.QueriesWithEventClassSubclassNames :
                                                                                                        bProfilerQueryStatsPresent ? ProfilerQueryTypes.QueriesWithQueryStats :
                                                                                                        ProfilerQueryTypes.BaseQuery)
                                                                                             || q.Key == "").ToList();
                    cmbProfilerAnalyses.Refresh();
                }
            }
        }
        private void AttachProfilerTraceDB()
        {
            if (InvokeRequired)
                tcAnalysis.Invoke(new System.Action(()=>tcAnalysis.SelectedTab.Controls["ProfilerTraceStatusTextbox"].Text += (ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Attaching profiler trace database...\r\n"));
            else
                tcAnalysis.SelectedTab.Controls["ProfilerTraceStatusTextbox"].Text += (ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Attaching profiler trace database...\r\n";

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
                    if (InvokeRequired)
                    {
                        Invoke(new System.Action(() =>
                        {
                            lblAnalysisQueries.Visible = cmbProfilerAnalyses.Visible = cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = txtProfilerAnalysisQuery.Visible = true;
                            ttStatus.SetToolTip(chkDettachProfilerAnalysisDBWhenDone, "Profiler traces were imported into a trace database in the file:\r\n" + AnalysisTraceID
                                                                                + ".mdf\r\n\r\nLocated at:\r\n" + m_analysisPath + "\\Analysis\r\n\r\n"
                                                                                + "Uncheck this checkbox if the scenario requires further analysis.\r\n\r\n"
                                                                                + "Note:  While attached the SQL data source at [" + connSqlDb.DataSource + "] locks these files from deletion while started.");
                            ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Attached trace database [" + AnalysisTraceID + "]\r\nto SQL instance [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]\r\nfor analysis at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
                        }));
                    }
                    else
                    {
                        lblAnalysisQueries.Visible = cmbProfilerAnalyses.Visible = cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = txtProfilerAnalysisQuery.Visible = true;
                        ttStatus.SetToolTip(chkDettachProfilerAnalysisDBWhenDone, "Profiler traces were imported into a trace database in the file:\r\n" + AnalysisTraceID
                                                                            + ".mdf\r\n\r\nLocated at:\r\n" + m_analysisPath + "\\Analysis\r\n\r\n"
                                                                            + "Uncheck this checkbox if the scenario requires further analysis.\r\n\r\n"
                                                                            + "Note:  While attached the SQL data source at [" + connSqlDb.DataSource + "] locks these files from deletion while started.");
                        ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Attached trace database [" + AnalysisTraceID + "]\r\nto SQL instance [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]\r\nfor analysis at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
                    }
                    ValidateProfilerTraceViews();
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
                    if (InvokeRequired)
                        Invoke(new System.Action(() => cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = false));
                    else
                        cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = false;

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
                                    if (InvokeRequired)
                                    {
                                        Invoke(new System.Action(() =>
                                        {
                                            lblAnalysisQueries.Visible = cmbProfilerAnalyses.Visible = cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = txtProfilerAnalysisQuery.Visible = true;
                                            ttStatus.SetToolTip(chkDettachProfilerAnalysisDBWhenDone, "Profiler traces were imported into a trace database in the file:\r\n" + AnalysisTraceID
                                                                            + ".mdf\r\n\r\nLocated at:\r\n" + m_analysisPath + "\\Analysis\r\n\r\n"
                                                                            + "Uncheck this checkbox if the scenario requires further analysis.\r\n\r\n"
                                                                            + "Note:  While attached the SQL data source at [" + connSqlDb.DataSource + "] locks these files from deletion while started.");
                                            ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Attached trace database [" + AnalysisTraceID + "]\r\nto SQL instance [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]\r\nfor analysis at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
                                        }));
                                    }
                                    else
                                    {
                                        lblAnalysisQueries.Visible = cmbProfilerAnalyses.Visible = cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = txtProfilerAnalysisQuery.Visible = true;
                                        ttStatus.SetToolTip(chkDettachProfilerAnalysisDBWhenDone, "Profiler traces were imported into a trace database in the file:\r\n" + AnalysisTraceID
                                                                        + ".mdf\r\n\r\nLocated at:\r\n" + m_analysisPath + "\\Analysis\r\n\r\n"
                                                                        + "Uncheck this checkbox if the scenario requires further analysis.\r\n\r\n"
                                                                        + "Note:  While attached the SQL data source at [" + connSqlDb.DataSource + "] locks these files from deletion while started.");
                                        ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Attached trace database [" + AnalysisTraceID + "]\r\nto SQL instance [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]\r\nfor analysis at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
                                    }
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
                                if (InvokeRequired)
                                    Invoke(new System.Action(()=> ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Unable to load trace data to SQL table.  No local instance able to host the data is available.\r\n")));
                                else
                                    ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Unable to load trace data to SQL table.  No local instance able to host the data is available.\r\n");
                                return;
                            }
                        }
                    }
                    else if (ex.Message.Contains("Unable to open the physical file") || ex.Message.Contains("The path specified by"))
                    {
                        if (InvokeRequired)
                            ProfilerTraceStatusTextBox.Invoke(new System.Action(()=>ProfilerTraceStatusTextBox.Text = (ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Trace file is not yet imported to database table for analysis.  Import to perform analysis.\r\n"));
                        else
                            ProfilerTraceStatusTextBox.Text = (ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Trace file is not yet imported to database table for analysis.  Import to perform analysis.\r\n";
                        return;
                    }
                    else
                    {
                        if (InvokeRequired)
                            ProfilerTraceStatusTextBox.Invoke(new System.Action(()=> ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Unable to attach to database due to exception:\r\n" + ex.Message)));
                        else
                            ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Unable to attach to database due to exception:\r\n" + ex.Message);
                    }
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
                Thread.Sleep(1000); // Sometimes it fails if we just immediately went to single user mode...  Shouldn't have to but add a tiny wait.
                cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') EXEC master.dbo.sp_detach_db @dbname = N'" + AnalysisTraceID + "'", connSqlDb);
                cmd.ExecuteNonQuery();
                if (InvokeRequired)
                    Invoke(new System.Action(() =>
                    {
                        lblAnalysisQueries.Visible = cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = false;
                        ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Detached trace database [" + AnalysisTraceID + "]\r\nfrom SQL instance [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]\r\nat " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
                    }));
                else
                {
                    lblAnalysisQueries.Visible = cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = false;
                    ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Detached trace database [" + AnalysisTraceID + "]\r\nfrom SQL instance [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]\r\nat " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
                }
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
                        cmbProfilerAnalyses.Text == "Queries/commands with errors")
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
                // This is just placeholder, need to format output...
                
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
            if ((sender as MenuItem).Text == "Find all queries/commands overlappning with selection")
                ; // TODO
            
        }
        #endregion Profiler Trace Analysis       

        #endregion AnalysisUI
    }
}