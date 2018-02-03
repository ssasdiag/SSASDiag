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
        #region AnalysisLocals

        List<TabPage> HiddenTabPages = new List<TabPage>();
        System.Windows.Forms.Timer AnalysisMessagePumpTimer = new System.Windows.Forms.Timer();

        #endregion AnalysisLocals

        private void tcCollectionAnalysisTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcCollectionAnalysisTabs.SelectedIndex == 1 && tbAnalysis.Enabled)
            {
                if (ProfilerTraceAnalysisQueries == null)
                {
                    ProfilerTraceAnalysisQueries = InitializeProfilerTraceAnalysisQueries();
                    cmbProfilerAnalyses.DataSource = ProfilerTraceAnalysisQueries;
                    cmbProfilerAnalyses.DisplayMember = "Name";
                }

                if (txtFolderZipForAnalysis.Text != "")
                {
                    AnalysisTraceID = m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace("_SSASDiagOutput", "_SSASDiag").Replace(".zip", "");
                    if (Directory.Exists(txtFolderZipForAnalysis.Text) || File.Exists(txtFolderZipForAnalysis.Text))
                        PopulateAnalysisTabs();
                }
                else
                    lblInitialAnalysisPrompt.Visible = true;
            }
            else
                Text = "SSAS Diagnostics Tool v" + Application.ProductVersion;
        }
        private void btnAnalysisFolder_Click(object sender, EventArgs e)
        {
            BrowseForFolder bff = new BrowseForFolder();
            bff.Filters.Add("zip");
            bff.Filters.Add("trc");
            bff.Filters.Add("blg");
            bff.Filters.Add("mdf");
            bff.Filters.Add("ini");
            bff.Filters.Add("etl");
            bff.Filters.Add("evtx");
            bff.Filters.Add("mdmp");
            string strPath = Environment.CurrentDirectory as string;
            strPath = bff.SelectFolder("Choose an SSASDiag folder or zip file for analysis of all its components.\r\nOR\r\n"
                      + "AS profiler trace file or db, PerfMon log, crash dump, network trace, or config file.", txtFolderZipForAnalysis.Text == "" ? strPath : txtFolderZipForAnalysis.Text, Handle);
            if (strPath != null && strPath != "")
            {
                txtFolderZipForAnalysis.Text = m_analysisPath = strPath;
                LogFeatureUse("Analysis File Opened", strPath.Substring(strPath.LastIndexOf("\\") + 1));
                PopulateAnalysisTabs();
            }
            AnalysisMessagePumpTimer.Interval = 1000;
        }
        private void PopulateAnalysisTabs()
        {
            StatusFloater.lblTime.Text = "00:00";
            StatusFloater.lblTime.Visible = true;
            AnalysisQueryExecutionPumpTimer.Interval = 1000;
            AnalysisQueryExecutionPumpTimer.Start();
            StatusFloater.Left = Left + Width / 2 - StatusFloater.Width / 2;
            StatusFloater.Top = Top + Height / 2 - StatusFloater.Height / 2;
            StatusFloater.lblStatus.Text = "Initializing and attaching analysis database(s)...";
            StatusFloater.PerformLayout();
            StatusFloater.Show(this);

            Text = "SSAS Diagnostics Analysis: " + txtFolderZipForAnalysis.Text.Substring(txtFolderZipForAnalysis.Text.LastIndexOf("\\") + 1);
            tcAnalysis.Visible = false;
            lblInitialAnalysisPrompt.Visible = false;
            if (connSqlDb.State != ConnectionState.Closed)
                connSqlDb.Close();
            if (File.Exists(m_analysisPath))
            {
                if (!m_analysisPath.EndsWith(".zip"))
                    AnalysisTraceID = m_analysisPath.Substring(0, m_analysisPath.LastIndexOf(".")).Substring(m_analysisPath.LastIndexOf("\\") + 1).TrimEnd("0123456789".ToCharArray());
            }
            else
                AnalysisTraceID = GetAnalysisIDFromLog();

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
                else
                    CompleteAnalysisTabsPopulationAfterZipExtraction();
            }
        }

        void CompleteAnalysisTabsPopulationAfterZipExtraction()
        {
            string mdfPath = "";
            if ((File.Exists(m_analysisPath) && m_analysisPath.EndsWith("\\msmdsrv.ini")) || File.Exists(m_analysisPath + "\\msmdsrv.ini"))
            {
                tcAnalysis.TabPages.Add(new TabPage("Configuration") { ImageIndex = 0, Name = "Configuration" });
                tcAnalysis.TabPages["Configuration"].Controls.Add(GetStatusTextBox(File.ReadAllText(m_analysisPath + "\\msmdsrv.ini")));
            }
            bool dirhasdumps = false;
            if (!File.Exists(m_analysisPath))
            {
                if (Directory.GetFiles(m_analysisPath, "*.mdmp", SearchOption.TopDirectoryOnly).Count() > 0)
                    dirhasdumps = true;
                else
                    foreach (string dir in Directory.EnumerateDirectories(m_analysisPath))
                    if (!dir.Contains("\\$RECYCLE.BIN") &&
                        !dir.Contains("\\System Volume Information") &&
                        Directory.GetFiles(dir, "*.mdmp", SearchOption.AllDirectories).Count() > 0)
                    {
                        dirhasdumps = true;
                        break;
                    }
            }
            if ((File.Exists(m_analysisPath) && m_analysisPath.EndsWith(".mdmp")) || dirhasdumps || 
                (Directory.Exists(m_analysisPath) && Directory.GetFiles(m_analysisPath + "\\Analysis", "SSASDiag_MemoryDump_Analysis_*.mdf", SearchOption.TopDirectoryOnly).Count() > 0))
            {
                if (ValidateProfilerTraceDBConnectionStatus())
                {
                    ucASDumpAnalyzer DumpAnalyzer = new ucASDumpAnalyzer(m_analysisPath, connSqlDb);
                    DumpAnalyzer.Dock = DockStyle.Fill;
                    tcAnalysis.TabPages.Add(new TabPage("Memory Dumps") { ImageIndex = 1, Name = "Memory Dumps" });
                    tcAnalysis.TabPages["Memory Dumps"].Controls.Add(DumpAnalyzer);
                }
            }
            if ((File.Exists(m_analysisPath) && m_analysisPath.EndsWith(".evtx")) ||
                File.Exists(m_analysisPath + "\\" + AnalysisTraceID + "_Application.evtx") ||
                File.Exists(m_analysisPath + "\\" + AnalysisTraceID + "_System.evtx"))
            {
                tcAnalysis.TabPages.Add(new TabPage("Event Logs") { ImageIndex = 2, Name = "Event Logs" });
                tcAnalysis.TabPages["Event Logs"].Controls.Add(GetStatusTextBox("Check back soon for automated analysis of event logs."));
            }
            if ((File.Exists(m_analysisPath) && m_analysisPath.EndsWith(".etl")) ||
                (File.Exists(m_analysisPath) && m_analysisPath.EndsWith(".cap")) ||
                File.Exists(m_analysisPath + "\\" + AnalysisTraceID + ".etl") ||
                File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.diag.log"))
            {
                tcAnalysis.TabPages.Add(new TabPage("Network Trace") { ImageIndex = 3, Name = "Network Trace" });
                RichTextBox txtNetworkAnalysis = GetStatusTextBox();
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
                SetupSQLTextbox();
                LogFeatureUse("Profiler Analysis", "Initializing analysis tab");
                splitProfilerAnalysis.Visible = false;
                ProfilerTraceStatusTextBox.Text = "";
                tcAnalysis.TabPages.Add(HiddenTabPages.Where(t => t.Text == "Profiler Trace").First());
                HiddenTabPages.Remove(HiddenTabPages.Where(t => t.Text == "Profiler Trace").First());
                if (!Validate2017ManagementComponents())
                {
                    ProfilerTraceStatusTextBox.Text = "SQL 2017 Management Studio components required.\r\nComplete install from https://go.microsoft.com/fwlink/?LinkID=840946 and then open Profiler Trace Analysis again.";
                    btnImportProfilerTrace.Visible = false;
                }
                else
                {
                    btnImportProfilerTrace.Visible = true;
                    string sqlForTraces = Properties.Settings.Default["SqlForProfilerTraceAnalysis"] as string;
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
            // Prefer loading some tabs before others (Order of pref: Configuration (default if it exists anyway by alpha ordering as first tab), Network, Profiler, Memory Dump, then the others are all just placeholders so ignored.
            if (!tcAnalysis.TabPages.ContainsKey("Configuration"))
            {
                if (tcAnalysis.TabPages.ContainsKey("tbProfilerTraces"))
                    tcAnalysis.SelectedTab = tcAnalysis.TabPages["tbProfilerTraces"];
                else if (tcAnalysis.TabPages.ContainsKey("Network Trace"))
                    tcAnalysis.SelectedTab = tcAnalysis.TabPages["Network Trace"];
                else if (tcAnalysis.TabPages.ContainsKey("Memory Dumps"))
                    tcAnalysis.SelectedTab = tcAnalysis.TabPages["Memory Dumps"];


            }
                
            StatusFloater.Visible = false;
            tcAnalysis.Visible = true;
        }


        string GetAnalysisIDFromLog()
        {
            try
            {
                string[] logs = Directory.GetFiles(m_analysisPath, "SSASDiag.log", SearchOption.TopDirectoryOnly);
                if (logs.Length > 0)
                {
                    string[] lines = File.ReadAllLines(logs[0]).Where(s => s.Trim() != "").ToArray();
                    if (lines.Length >= 2 && lines[1].StartsWith("Initialized service for trace with ID: "))
                        return lines[1].Substring("Initialized service for trace with ID: ".Length);
                    else
                        return m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace("_SSASDiagOutput", "_SSASDiag");
                }
                else
                    return m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace("_SSASDiagOutput", "_SSASDiag");
            }
            catch
            {
                return m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace("_SSASDiagOutput", "_SSASDiag");
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
                z.UseZip64WhenSaving = Ionic.Zip.Zip64Option.Always;
                z.ParallelDeflateThreshold = -1;
                if (z.Entries.Where(f => f.FileName.Substring(f.FileName.LastIndexOf("/") + 1) == file.Substring(file.LastIndexOf("\\") + 1)).Count() == 0)
                {
                    try
                    {
                        z.AddFile(file, AnalysisTraceID + "/Analysis");
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
        public RichTextBox GetStatusTextBox(string Text = "")
        {
            RichTextBox txtStatus = new RichTextBox();
            txtStatus.Name = "StatusTextbox";
            txtStatus.Multiline = true;
            txtStatus.ReadOnly = true;
            txtStatus.BackColor = SystemColors.ControlText;
            txtStatus.ForeColor = Color.LightSkyBlue;
            txtStatus.Font = (HiddenTabPages.Find(t => t.Name == "tbProfilerTraces") == null ? tcAnalysis.TabPages["tbProfilerTraces"] : HiddenTabPages.Find(t => t.Name == "tbProfilerTraces")).Font;
            txtStatus.Dock = DockStyle.Fill;
            txtStatus.WordWrap = false;
            txtStatus.ScrollBars = RichTextBoxScrollBars.Both;
            txtStatus.Text = Text;
            return txtStatus;
        }
        private void SelectivelyExtractAnalysisDataFromZip()
        {
            StatusFloater.lblStatus.Text = "Extracting zipped files for analysis...";
            BackgroundWorker bgSelectivelyExtractAnalysisDataFromZip = new BackgroundWorker();
            bgSelectivelyExtractAnalysisDataFromZip.DoWork += BgSelectivelyExtractAnalysisDataFromZip_DoWork;
            bgSelectivelyExtractAnalysisDataFromZip.RunWorkerCompleted += BgSelectivelyExtractAnalysisDataFromZip_RunWorkerCompleted;
            bgSelectivelyExtractAnalysisDataFromZip.RunWorkerAsync();
        }

        private void BgSelectivelyExtractAnalysisDataFromZip_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CompleteAnalysisTabsPopulationAfterZipExtraction();
            StatusFloater.lblSubStatus.Text = "";
            StatusFloater.lblTime.Visible = false;
            AnalysisMessagePumpTimer.Stop();
        }

        private void BgSelectivelyExtractAnalysisDataFromZip_DoWork(object sender, DoWorkEventArgs e)
        {
            Ionic.Zip.ZipFile z = new Ionic.Zip.ZipFile(m_analysisPath);
            z.UseZip64WhenSaving = Ionic.Zip.Zip64Option.Always;
            z.ParallelDeflateThreshold = -1;
            // Always extract directly into the current running location.
            // This ensures we don't accidentally fill up a temp drive or something with large files.
            AnalysisTraceID = m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace("_SSASDiagOutput", "_SSASDiag").Replace(".zip", "");
            m_analysisPath = m_analysisPath.Substring(0, m_analysisPath.LastIndexOf("\\") + 1) + AnalysisTraceID;

            if (!Directory.Exists(m_analysisPath))
                Directory.CreateDirectory(m_analysisPath);
            if (!Directory.Exists(m_analysisPath + "\\Analysis"))
                Directory.CreateDirectory(m_analysisPath + "\\Analysis");

            ExtractFileToPath(z, m_analysisPath, "SSASDiag.log");
            AnalysisTraceID = GetAnalysisIDFromLog();

            if (z.Entries.Where(f => f.FileName.Contains(".mdf")).Count() > 0)
            {
                try
                {
                    foreach (Ionic.Zip.ZipEntry ze in z.Entries.Where(f => f.FileName.Contains(".mdf") || f.FileName.Contains(".ldf")).ToList())
                        ExtractFileToPath(z, m_analysisPath + "\\Analysis", ze.FileName.Substring(ze.FileName.LastIndexOf("/") + 1));
                }
                catch (Exception ex)
                {
                    // Continue, since if we fail writing these files, it means they do already exist, and we can probably just attach subsequently without failure.
                    LogException(ex);
                }
            }
            else
            if (z.Entries.Where(f => f.FileName == AnalysisTraceID + "/Analysis/" + AnalysisTraceID + ".mdf").Count() == 0 && z.Entries.Where(f => f.FileName.Contains(".trc")).Count() > 0)
                ExtractAllFilesOfType(z, ".trc");
            if (z.Entries.Where(f => f.FileName == AnalysisTraceID + "/Analysis/" + AnalysisTraceID + "_NetworkAnalysis.log").Count() > 0)
                ExtractFileToPath(z, m_analysisPath + "\\Analysis", AnalysisTraceID + "_NetworkAnalysis.log");
            else
            {
                if (z.Entries.Where(f => f.FileName == AnalysisTraceID + ".etl").Count() > 0)
                    ExtractFileToPath(z, m_analysisPath, AnalysisTraceID + ".etl");
                if (z.Entries.Where(f => f.FileName == AnalysisTraceID + ".cab").Count() > 0)
                    ExtractFileToPath(z, m_analysisPath, AnalysisTraceID + ".cab");
            }
            if (z.Entries.Where(f => f.FileName.Contains(".blg")).Count() > 0)
                ExtractAllFilesOfType(z, ".blg");
            if (z.Entries.Where(f=>f.FileName.Contains("SSASDiag_MemoryDump_Analysis_")).Count() == 0
                && (z.Entries.Where(f => f.FileName.Contains(".mdmp")).Count() > 0))
                ExtractAllFilesOfType(z, ".mdmp");
            if (z.Entries.Where(f => f.FileName.Contains(".evtx")).Count() > 0)
                ExtractAllFilesOfType(z, ".evtx");
            if (z.Entries.Where(f => f.FileName == "msmdsrv.ini").Count() > 0)
                ExtractFileToPath(z, m_analysisPath, "msmdsrv.ini");
            z.Dispose();

            StatusFloater.Invoke(new System.Action(() =>
            {
                StatusFloater.Hide();
            }));
        }

        private void ExtractFileToPath(Ionic.Zip.ZipFile z, string OutputPath, string FileName)
        {
            z.UseZip64WhenSaving = Ionic.Zip.Zip64Option.Always;
            StatusFloater.Invoke(new System.Action(() => StatusFloater.lblSubStatus.Text = FileName));
            FileStream fs;
            if (!File.Exists(OutputPath + "\\" + FileName) && z.Count(ze=>ze.FileName.Contains(FileName)) > 0)
            {
                fs = new FileStream(OutputPath + "\\" + FileName, FileMode.Create);
                Ionic.Zip.ZipEntry ze = z.Entries.Where(f => f.FileName.Contains(FileName)).First();
                ze.Extract(fs);
                fs.Flush(true);
                fs.Close();
            }
        }
        private void ExtractAllFilesOfType(Ionic.Zip.ZipFile z, string ext)
        {
            foreach (Ionic.Zip.ZipEntry e in z.Entries.Where(f => f.FileName.Contains(ext)))
            {
                StatusFloater.Invoke(new System.Action(() => StatusFloater.lblSubStatus.Text = e.FileName.Substring(e.FileName.LastIndexOf("/") + 1)));
                if (!File.Exists(m_analysisPath + "\\" + e.FileName.Substring(e.FileName.LastIndexOf("/") + 1)))
                {
                    FileStream fs = new FileStream(m_analysisPath + "\\" + e.FileName.Substring(e.FileName.LastIndexOf("/") + 1), FileMode.Create);
                    e.Extract(fs);
                    fs.Close();
                }
            }
        }
        private void btnAnalyzeNetworkTrace_Click(object sender, EventArgs e)
        {
            if (!File.Exists(m_analysisPath) && !Directory.Exists(m_analysisPath))
                Directory.CreateDirectory(m_analysisPath);
            string AnalysisDir = m_analysisPath.EndsWith(".etl") || m_analysisPath.EndsWith(".cap") ? m_analysisPath.Substring(0, m_analysisPath.LastIndexOf("\\")) + "\\Analysis\\" : m_analysisPath + "\\Analysis\\";
            if (!Directory.Exists(AnalysisDir))
                Directory.CreateDirectory(AnalysisDir);

            TextBox txtNetworkAnalysis = tcAnalysis.TabPages["Network Trace"].Controls["StatusTextbox"] as TextBox;
            tcAnalysis.TabPages["Network Trace"].Controls[0].Visible = false;
            txtNetworkAnalysis.Text = "Analysis of network trace started...";
            AnalysisMessagePumpTimer.Interval = 1000;
            AnalysisMessagePumpTimer.Start();

            new Thread(new ThreadStart(new System.Action(() =>
            {
                LogFeatureUse("Network Trace Analysis", "Started");
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = Program.TempPath + "sqlna.exe";
                p.StartInfo.Arguments = "\"" + (!(m_analysisPath.EndsWith(".etl") || m_analysisPath.EndsWith(".cap")) ? m_analysisPath + "\\" + (AnalysisTraceID + ".etl") : m_analysisPath) + "\" /output \"" + AnalysisDir + AnalysisTraceID + "_NetworkAnalysis.log\"";
                p.Start();
                string sOut = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                List<string> sNetworkAnalysis = new List<string>(File.ReadAllLines(AnalysisDir + AnalysisTraceID + "_NetworkAnalysis.log"));
                sNetworkAnalysis.RemoveRange(0, 6);
                sNetworkAnalysis.RemoveRange(sNetworkAnalysis.Count - 6, 6);
                for (int i = 0; i < sNetworkAnalysis.Count; i++)
                    sNetworkAnalysis[i] = sNetworkAnalysis[i].TrimStart(' ');
                File.WriteAllLines(AnalysisDir + AnalysisTraceID + "_NetworkAnalysis.log", sNetworkAnalysis);
                txtNetworkAnalysis.Invoke(new System.Action(() => txtNetworkAnalysis.Text = string.Join("\r\n", sNetworkAnalysis.ToArray())));
                AddFileFromFolderIfAnlyzingZip(AnalysisDir + AnalysisTraceID + "_NetworkAnalysis.log");
                AddFileFromFolderIfAnlyzingZip(AnalysisDir + AnalysisTraceID + "_NetworkAnalysis.diag.log");
                if (txtFolderZipForAnalysis.Text.EndsWith(".zip"))
                {
                    foreach (string file in Directory.EnumerateFiles(m_analysisPath, AnalysisTraceID + "*.etl"))
                        File.Delete(file);
                    foreach (string file in Directory.EnumerateFiles(m_analysisPath, AnalysisTraceID + "*.cab"))
                        File.Delete(file);
                }
                LogFeatureUse("Network Trace Analysis", "Completed");
                AnalysisMessagePumpTimer.Stop();
            }))).Start();
        }
        
    }
}