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
                LogFeatureUse("Profiler Analysis", "Opening analysis tab");
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
            if (connSqlDb.State != ConnectionState.Closed)
                connSqlDb.Close();
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
                    splitProfilerAnalysis.Visible = false;
                    ProfilerTraceStatusTextBox.Text = "";
                    tcAnalysis.TabPages.Add(HiddenTabPages.Where(t => t.Text == "Profiler Traces").First());
                    HiddenTabPages.Remove(HiddenTabPages.Where(t => t.Text == "Profiler Traces").First());
                    if (!Validate2016ManagementComponents())
                    {
                        ProfilerTraceStatusTextBox.Text = "SQL 2016 Management Studio components required.\r\nComplete install from https://go.microsoft.com/fwlink/?LinkID=840946 and then open Profiler Trace Analysis again.";
                        btnImportProfilerTrace.Visible = false;
                    }
                    else
                    {
                        btnImportProfilerTrace.Visible = true;
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
            string AnalysisDir = m_analysisPath.EndsWith(".etl") ? m_analysisPath.Substring(0, m_analysisPath.LastIndexOf("\\")) + "\\Analysis\\" : m_analysisPath + "\\Analysis\\";
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
                p.StartInfo.FileName = Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\sqlna.exe";
                p.StartInfo.Arguments = "\"" + (!m_analysisPath.EndsWith(".etl") ? m_analysisPath + "\\" + (AnalysisTraceID + ".etl") : m_analysisPath) + "\" /output \"" + AnalysisDir + AnalysisTraceID + "_NetworkAnalysis.log\"";
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