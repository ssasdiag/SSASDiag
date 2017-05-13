using System;
using System.IO;
using System.Net.Security;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.DirectoryServices.AccountManagement;


namespace SSASDiag
{
    public partial class frmSSASDiag : Form
    {
        #region locals

        string m_instanceVersion, m_instanceType, m_instanceEdition, m_analysisPath = "";
        CDiagnosticsCollector dc;
        frmStatusFloater StatusFloater = new frmStatusFloater();
        string m_LogDir = "", m_ConfigDir = "", AnalysisTraceID = "";  
        List<ComboBoxServiceDetailsItem> LocalInstances = new List<ComboBoxServiceDetailsItem>();
        Image imgPlay = Properties.Resources.play, imgPlayLit = Properties.Resources.play_lit, imgPlayHalfLit = Properties.Resources.play_half_lit,
            imgStop = Properties.Resources.stop_button_th, imgStopLit = Properties.Resources.stop_button_lit, imgStopHalfLit = Properties.Resources.stop_button_half_lit;
        bool bClosing = false, bFullyInitialized = false;
        Dictionary<string, string> Args = new Dictionary<string, string>();

        #endregion locals

        public frmSSASDiag()
        {
            InitializeComponent();
        }

        private void InitializeArgs()
        {
            string[] argArray = Environment.GetCommandLineArgs();
            for (int i = 0; i < argArray.Length; i++)
            {
                try
                {
                    if (argArray[i].StartsWith("/") || argArray[i].StartsWith("-"))
                        Args.Add(argArray[i].TrimStart(new char[] { '-', '/' }).ToLower(),
                            (argArray.Length > i + 1
                             && !(argArray[i + 1].StartsWith("/") || argArray[i + 1].StartsWith("-"))
                             ? argArray[i + 1]
                             : ""));
                    else
                    {
                        if (i > 0 && !argArray[i - 1].StartsWith("/") && !argArray[i - 1].StartsWith("-") && !Args.ContainsKey("filename") && File.Exists(argArray[i]))
                            Args.Add("filename", argArray[i]);
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }
        }

        #region frmSSASDiagEvents
        private void frmSSASDiag_Load(object sender, EventArgs e)
        {
            InitializeArgs();
            SetupDebugTrace();

            if (!(Environment.OSVersion.Version.Major >= 7 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1)) && Environment.UserInteractive)
            {
                if (Environment.UserInteractive && bFullyInitialized)
                    MessageBox.Show("Network trace collection requires\nWindows 7 or Server 2008 R2 or greater.\nPlease upgrade your OS to use that feature.", "SSAS Diagnotics Network Trace Incompatibility Warning", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                chkGetNetwork.Enabled = false;
            }

            imgPlay.Tag = "Play"; imgPlayLit.Tag = "Play Lit"; imgPlayHalfLit.Tag = "Play Half Lit"; imgStop.Tag = "Stop"; imgStopLit.Tag = "Stop Lit"; imgStopHalfLit.Tag = "Stop Half Lit";
            btnCapture.Image = imgPlay;
            Environment.CurrentDirectory = AppDomain.CurrentDomain.GetData("originalbinlocation") as string;
            if (!Args.ContainsKey("outputdir") && Properties.Settings.Default["SaveLocation"] as string != Environment.CurrentDirectory && Properties.Settings.Default["SaveLocation"] as string != "")
                Environment.CurrentDirectory = Properties.Settings.Default["SaveLocation"] as string;
            if (Args.ContainsKey("outputdir"))
                Environment.CurrentDirectory = Args["outputdir"];

            PopulateInstanceDropdown();

            if (Args.ContainsKey("stoptime"))
            {
                chkStopTime.Checked = true;
                try { dtStopTime.Value = Convert.ToDateTime(Args["stoptime"]); }
                catch { chkStopTime.Checked = false; }
            }
            else
                dtStopTime.Value = DateTime.Now.AddHours(1);
            dtStopTime.MinDate = DateTime.Now.AddMinutes(1);
            dtStopTime.CustomFormat += TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours > 0 ? "+" 
                + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString() : TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString();
            dtStartTime.CustomFormat = dtStopTime.CustomFormat;
            dtStartTime.MinDate = DateTime.Now;
            dtStartTime.MaxDate = DateTime.Now.AddDays(30);
            if (Args.ContainsKey("starttime"))
            {
                chkStartTime.Checked = true;
                try { dtStartTime.Value = Convert.ToDateTime(Args["starttime"]); }
                catch { chkStartTime.Checked = false; }
            }

            // UI timer to enable detection of fast/slow scroll to avoid messagebox if fast sliding past middle setting...
            tmScrollStart.Interval = 250;
            tmScrollStart.Tick += tmLevelOfDataScroll_Tick;
            frmSSASDiag_Resize(this, e);

            foreach (TabPage t in tcAnalysis.TabPages)
                HiddenTabPages.Add(t);
            for (int i = 0; i < tcAnalysis.TabPages.Count; i++)
                tcAnalysis.TabPages.RemoveAt(0);
            
            txtSaveLocation.Text = Environment.CurrentDirectory;

            AnalysisMessagePumpTimer.Tick += AnalysisMessagePumpTimer_Tick;
            AnalysisQueryExecutionPumpTimer.Tick += AnalysisQueryExecutionPumpTimer_Tick;

            if (!Args.ContainsKey("zip") && Args.Count > 0) chkZip.Checked = false;
            if (Args.ContainsKey("deleteraw")) chkDeleteRaw.Checked = true;

            if (!Args.ContainsKey("rollover") && Args.Count > 0)
                chkRollover.Checked = false;
            else
                try { udRollover.Value = Convert.ToInt32(Args["rollover"]); }
                catch {  }

            if (Args.ContainsKey("autorestartprofiler") && Args.ContainsKey("stoptime"))
                chkAutoRestart.Checked = true;

            if (Args.ContainsKey("perfmoninterval"))
                try { udInterval.Value = Convert.ToInt32(Args["perfmoninterval"]); }
                catch { }
        }

        private void chkAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoUpdate = Convert.ToString(chkAutoUpdate.Checked);
            Properties.Settings.Default.Save();
            if (chkAutoUpdate.Checked)
                Program.CheckForUpdates(AppDomain.CurrentDomain);
        }

        private void frmSSASDiag_Shown(object sender, EventArgs e)
        {
            bool bUsageStatsAlreadySet = true;
            string s = Properties.Settings.Default.AllowUsageStats;

            if (Args.ContainsKey("reportusage"))
                chkAllowUsageStatsCollection.Checked = true;
            else
            {
                if (Properties.Settings.Default.AllowUsageStats == "")
                {
                    bUsageStatsAlreadySet = false;
                    if (Environment.UserInteractive)
                    {
                        if (MessageBox.Show("Please help improve SSASDiag by allowing anonymous collection of usage statistics.\r\n\r\nWill you support improvements to the utility to enable now?", "Enable Collection of Anonymous Usage Statistics", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                            Properties.Settings.Default.AllowUsageStats = "True";
                        else
                            Properties.Settings.Default.AllowUsageStats = "False";
                        Properties.Settings.Default.Save();
                    }
                }
                chkAllowUsageStatsCollection.Checked = Convert.ToBoolean(Properties.Settings.Default.AllowUsageStats);
            }

            if (bUsageStatsAlreadySet)
            {
                if (Properties.Settings.Default.AutoUpdate == "" && Environment.UserInteractive)
                {
                    if (MessageBox.Show("Would you like to enable automatic update checks on startup?", "Enable Update Checking", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                        Properties.Settings.Default.AutoUpdate = "True";
                    else
                        Properties.Settings.Default.AutoUpdate = "False";
                    Properties.Settings.Default.Save();
                }
            }

            if (Environment.UserInteractive && Properties.Settings.Default.AutoUpdate!= "True")
                chkAutoUpdate.Checked = false;
            else
                chkAutoUpdate.Checked = true;

            if (Args.ContainsKey("problemtype"))
                try
                {
                    cmbProblemType.SelectedIndex = Convert.ToInt32(Args["problemtype"]);
                    SetRolloverAndStartStopEnabledStates();
                }
                catch { }
            else
            {
                chkGetConfigDetails.Checked = chkGetPerfMon.Checked = chkGetProfiler.Checked = chkProfilerPerfDetails.Checked = chkXMLA.Checked = chkABF.Checked = chkBAK.Checked = chkGetNetwork.Checked = false;
            }

            if (Args.ContainsKey("config")) chkGetConfigDetails.Checked = true;
            if (Args.ContainsKey("perfmon")) chkGetPerfMon.Checked = true;
            if (Args.ContainsKey("profiler")) chkGetProfiler.Checked = true;
            if (Args.ContainsKey("verbose")) chkProfilerPerfDetails.Checked = true; 
            if (Args.ContainsKey("xmla")) chkXMLA.Checked = true; 
            if (Args.ContainsKey("abf")) chkABF.Checked = true; 
            if (Args.ContainsKey("bak")) chkBAK.Checked = true; 
            if (Args.ContainsKey("network")) chkGetNetwork.Checked = true; 

            if (!Args.ContainsKey("config") && !Args.ContainsKey("perfmon") && !Args.ContainsKey("profiler") && !Args.ContainsKey("verbose") && !Args.ContainsKey("xmla") && !Args.ContainsKey("abf") && !Args.ContainsKey("bak") && !Args.ContainsKey("network") && !Args.ContainsKey("problemtype"))
                cmbProblemType.SelectedIndex = 0;

            if (Args.ContainsKey("levelofdata"))
                try { tbLevelOfData.Value = Convert.ToInt32(Args["levelofdata"]); }
                catch { }

            if (Args.ContainsKey("filename") && !Args.ContainsKey("start"))
            {
                txtFolderZipForAnalysis.Text = m_analysisPath = Args["filename"];
                LogFeatureUse("Analysis File Opened", Args["filename"].Substring(Args["filename"].LastIndexOf("\\") + 1));
                PopulateAnalysisTabs();
                tcCollectionAnalysisTabs.SelectedIndex = 1;
            }

            LogFeatureUse("Startup", "Initialization of tool complete");

            bFullyInitialized = true;
        }

        private void SetupDebugTrace()
        {
            if (Environment.GetCommandLineArgs().Select(s => s.ToLower()).Contains("/debug"))
                System.Diagnostics.Trace.Listeners.Add(new TextWriterTraceListener(AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\SSASDiagDebugTrace_" + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd_HH-mm-ss") + "_UTC" + ".log"));
            System.Diagnostics.Trace.AutoFlush = true;
            System.Diagnostics.Trace.WriteLine("Started diagnostic trace.");
            if (!Environment.UserInteractive)
                System.Diagnostics.Trace.WriteLine("Running as a service.");
        }

        public static void LogFeatureUse(string FeatureName, string FeatureDetail = "")
        {
            // For internal Microsoft users we can collect basic usage data without requiring consent.
            // For external users, we only collect with full consent.
            // For internal Microsoft users, we also collect alias to track global usage across teams primarily, and distinguish development use from genuine engineer use.
            try
            {
                if (Program.MainForm.chkAllowUsageStatsCollection.Checked || (Environment.UserInteractive && UserPrincipal.Current.UserPrincipalName.ToLower().Contains("microsoft.com")) || Environment.GetCommandLineArgs().Select(s=>s.ToLower()).Contains("/reportusage"))
                    new Thread(new ThreadStart(() =>
                    {
                        try
                        {
                            WebClient wc = new WebClient();
                            NameValueCollection nvc = new NameValueCollection();
                            nvc.Add("RunID", WebUtility.UrlEncode(Program.RunID.ToString()));
                            nvc.Add("UsageVersion", WebUtility.UrlEncode(FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).FileVersion));
                            nvc.Add("FeatureName", WebUtility.UrlEncode(FeatureName));
                            nvc.Add("FeatureDetail", WebUtility.UrlEncode(FeatureDetail.Replace("'", "''")));
                            nvc.Add("UpnSuffix", WebUtility.UrlEncode((Environment.UserInteractive ? UserPrincipal.Current.UserPrincipalName.Substring(UserPrincipal.Current.UserPrincipalName.IndexOf("@") + 1) : "")));
                            nvc.Add("MicrosoftInternal", WebUtility.UrlEncode((Environment.UserInteractive ? (UserPrincipal.Current.UserPrincipalName.ToLower().Contains("microsoft.com") ? Environment.UserName : "") : "LocalSystem")));
                            // Skip SSL certificate validation on this private server
                            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                            wc.UploadValues("https://jburchelsrv.southcentralus.cloudapp.azure.com/SSASDiagUsageStats.aspx", nvc);
                            // Reset SSL to normal behavior.
                            ServicePointManager.ServerCertificateValidationCallback = null;
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    })).Start();
            }
            catch(Exception e)
            {
                LogException(e);
            }
        }

        int ifrmSSASDiagSizeBeforeResize = 0;
        private void frmSSASDiag_ResizeBegin(object sender, EventArgs e)
        {
            ifrmSSASDiagSizeBeforeResize = Height;
        }

        private void frmSSASDiag_ResizeEnd(object sender, EventArgs e)
        {
            int Change = Height - ifrmSSASDiagSizeBeforeResize;
            splitCollectionUI.SplitterDistance += (Change / 6);
        }

        private void frmSSASDiag_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
                splitCollectionUI.SplitterDistance = (Height / 4) + 50;
        }

        private void chkAllowUsageStatsCollection_MouseHover(object sender, EventArgs e)
        {
            ttStatus.Show(
@"Data collected:
    * External facing IP/domain of client
    * UTC date and time of feature use
    * Release version of diagnostic
    * Name of feature engaged
    * Options selected for each feature", chkAllowUsageStatsCollection, 5000);
        }

        private void chkAllowUsageStatsCollection_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AllowUsageStats = Convert.ToString(chkAllowUsageStatsCollection.Checked);
            if (Environment.UserInteractive) Properties.Settings.Default.Save();
        }

        private void frmSSASDiag_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (btnCapture.Image.Tag as string == "Stop" || btnCapture.Image.Tag as string == "Stop Lit")
                {
                    if (!Environment.UserInteractive || MessageBox.Show("Capture in progress, exiting will stop.\r\nExit anyway?", "Capture in progress", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        bClosing = true;
                        btnCapture_Click(sender, e);
                    }
                    e.Cancel = true;
                }
                else if (((string)btnCapture.Image.Tag as string).Contains("Half Lit"))
                {
                    if (!Environment.UserInteractive || MessageBox.Show("Diagnostic Capture is in a blocking state.\nForcing exit now may leave locked files and traces in progress.\n\nExit anyway?", "Capture in progress", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        if (Application.OpenForms.Count > 1)
                            Application.OpenForms["PasswordPrompt"].Invoke(new System.Action(() => Application.OpenForms["PasswordPrompt"].Close()));
                    }
                }
                if (bProfilerTraceDbAttached && chkDettachProfilerAnalysisDBWhenDone.Checked)
                {
                    StatusFloater.lblStatus.Text = "Detaching attached profiler trace database...";
                    StatusFloater.Left = Left + Width / 2 - StatusFloater.Width / 2;
                    StatusFloater.Top = Top + Height / 2 - StatusFloater.Height / 2;
                    StatusFloater.Show(this);
                    Enabled = false;
                    BackgroundWorker bgDetachProfilerDB = new BackgroundWorker();
                    bgDetachProfilerDB.DoWork += BgDetachProfilerDB_DoWork; ;
                    bgDetachProfilerDB.RunWorkerCompleted += BgDetachProfilerDB_RunWorkerCompleted;
                    bgDetachProfilerDB.RunWorkerAsync();
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                // This should never happen but might if we are summarily killing midway through something.  Don't get hung up just close.
            }
            System.Diagnostics.Trace.WriteLine("SSASDiag form closed, Cancel set to: " + e.Cancel);
        }
        private void BgDetachProfilerDB_DoWork(object sender, DoWorkEventArgs e)
        {
            DettachProfilerTraceDB();
        }
        private void BgDetachProfilerDB_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusFloater.Close();
            Close();
        }
        private void frmSSASDiag_Resize(object sender, EventArgs e)
        {
            chkAllowUsageStatsCollection.Top = (lkAbout.Top = lkDiscussion.Top = lkFeedback.Top = lkBugs.Top = Height - 59) + 2;
            chkAllowUsageStatsCollection.Left = Width - chkAllowUsageStatsCollection.Width - 15;
            chkAutoUpdate.Left = Width - chkAutoUpdate.Width - 15;
            txtStatus.Width = Width - 30;
            txtStatus.Height = Height - 315;
            tcCollectionAnalysisTabs.Height = Height - 59;
            tcAnalysis.Height = Height - 119;
            btnImportProfilerTrace.Left = Width / 2 - btnImportProfilerTrace.Width / 2;
            splitProfilerAnalysis.Height = Height - 232;
            txtProfilerAnalysisQuery.Width = Width - 254;
            lblProfilerAnalysisStatusCenter.Left = Width / 2 - lblProfilerAnalysisStatusCenter.Width / 2;
            if (tcAnalysis.TabPages.ContainsKey("Network Trace") || HiddenTabPages.Where(t => t.Name == "Network Trace").Count() > 0)
            {
                Button btnAnalyzeNetworkTrace = tcAnalysis.TabPages.ContainsKey("Network Trace") ? 
                    tcAnalysis.TabPages["Network Trace"].Controls["btnAnalyzeNetworkTrace"] as Button : 
                    HiddenTabPages.First(t => t.Name == "Network Trace").Controls["btnAnalyzeNetworkTrace"] as Button;
                btnAnalyzeNetworkTrace.Left = Width / 2 - btnAnalyzeNetworkTrace.Width / 2;
            }
            // Expand last column of profiler analysis grid
            if (dgdProfilerAnalyses.Columns.Count > 0)
            {
                if (dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].AutoSizeMode == DataGridViewAutoSizeColumnMode.None)
                    dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                if (dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].Width < 80)
                    dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgdProfilerAnalyses.Refresh();
                int lastCellFullHeaderWidth = dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].Width;
                dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].Width = lastCellFullHeaderWidth;
            }
        }
        public static void LogException(Exception ex)
        {
            System.Diagnostics.Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
            LogFeatureUse("Exception", "Message:\n" + ex.Message + "\n at stack:\n" + ex.StackTrace);   
        }
        #endregion frmSSASDiagEvents

        #region FeedbackUI
        private void lkFeedback_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("mailto:ssasdiagchamps@service.microsoft.com?subject=Feedback on SSAS Diagnostics Collector Tool&cc=jburchel@microsoft.com");
        }
        private void lkBugs_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://asprofilertraceimporter.codeplex.com/workitem/list/basic");
        }
        private void lkDiscussion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://asprofilertraceimporter.codeplex.com/discussions");
        }
        private void lkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmAbout f = new frmAbout();
            f.ShowDialog(this);
        }
        #endregion FeedbackUI
    }
}
