using Microsoft.Win32;
using System.DirectoryServices;
using System.Text;
using System.Security.Principal;
using System.ServiceProcess;
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
using System.Runtime.InteropServices;


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

        #region Win32
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength);

        enum TOKEN_INFORMATION_CLASS
        {
            /// <summary>
            /// The buffer receives a TOKEN_USER structure that contains the user account of the token.
            /// </summary>
            TokenUser = 1
        }

        public struct TOKEN_USER
        {
            public SID_AND_ATTRIBUTES User;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SID_AND_ATTRIBUTES
        {

            public IntPtr Sid;
            public int Attributes;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool LookupAccountSid(string lpSystemName, IntPtr Sid, System.Text.StringBuilder lpName, ref uint cchName, System.Text.StringBuilder ReferencedDomainName, ref uint cchReferencedDomainName, out SID_NAME_USE peUse);

        enum SID_NAME_USE
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
        #endregion Win32

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
            if (AppDomain.CurrentDomain.GetData("originalbinlocation") as string != null)
                Environment.CurrentDirectory = AppDomain.CurrentDomain.GetData("originalbinlocation") as string;
            else
            {
                Hide();
                MessageBox.Show("The tool cannot run from its own temp directory, used internally.  Please run from another location.", "App cannot run from its own temp location - by design", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
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

            SetWeakFileAssociation(".trc", "SSASDiag Profiler Trace Analyzer", AppDomain.CurrentDomain.GetData("originalbinlocation") as string + "\\SSASDiag.exe", "SSAS Diagnostics Tool");
            SetWeakFileAssociation(".etl", "SSASDiag Network Trace .etl Analyzer", AppDomain.CurrentDomain.GetData("originalbinlocation") as string + "\\SSASDiag.exe", "SSAS Diagnostics Tool");
            SetWeakFileAssociation(".cap", "SSASDiag Network Trace .cap Analyzer", AppDomain.CurrentDomain.GetData("originalbinlocation") as string + "\\SSASDiag.exe", "SSAS Diagnostics Tool");
            SetWeakFileAssociation(".zip", "SSASDiag Data Collection Analyzer", AppDomain.CurrentDomain.GetData("originalbinlocation") as string + "\\SSASDiag.exe", "SSAS Diagnostics Tool");
        }

        private void chkAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoUpdate = Convert.ToString(chkAutoUpdate.Checked);
            Properties.Settings.Default.Save();
            if (chkAutoUpdate.Checked)
                Program.CheckForUpdates(AppDomain.CurrentDomain);
        }

        public static void SetWeakFileAssociation(string Extension, string KeyName, string OpenWith, string FileDescription)
        {
            RegistryKey BaseKey;
            RegistryKey OpenMethod;
            RegistryKey Shell;

            BaseKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + Extension);
            BaseKey.CreateSubKey("OpenWithProgids").SetValue(KeyName, "");

            OpenMethod = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + KeyName);
            OpenMethod.SetValue("", FileDescription);

            Shell = OpenMethod.CreateSubKey("Shell");
            Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"%1\"");
            BaseKey.Close();
            OpenMethod.Close();
            Shell.Close();

            // Tell explorer the file association has been changed
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
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

            LogFeatureUse("Startup", "Initialization complete.  AutoUpdate=" + chkAutoUpdate.Checked + ",AllowUsageStats=" + chkAllowUsageStatsCollection.Checked);

            bFullyInitialized = true;

            SetWeakFileAssociation(".trc", "SSASDiag Profiler Trace Analyzer", AppDomain.CurrentDomain.GetData("originalbinlocation") as string + "\\SSASDiag.exe", "SSAS Diagnostics Tool");
            SetWeakFileAssociation(".etl", "SSASDiag Network Trace .etl Analyzer", AppDomain.CurrentDomain.GetData("originalbinlocation") as string + "\\SSASDiag.exe", "SSAS Diagnostics Tool");
            SetWeakFileAssociation(".cap", "SSASDiag Network Trace .cap Analyzer", AppDomain.CurrentDomain.GetData("originalbinlocation") as string + "\\SSASDiag.exe", "SSAS Diagnostics Tool");
            SetWeakFileAssociation(".zip", "SSASDiag Data Collection Analyzer", AppDomain.CurrentDomain.GetData("originalbinlocation") as string + "\\SSASDiag.exe", "SSAS Diagnostics Tool");

            pp.FormClosed += Pp_FormClosed;
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
                            try
                            {
                                string UserAndDomain = UserPrincipal.Current.UserPrincipalName;
                                nvc.Add("UpnSuffix", WebUtility.UrlEncode((Environment.UserInteractive ? UserAndDomain.Substring(UserAndDomain.IndexOf("@") + 1) : "")));
                                if (UserAndDomain.EndsWith("microsoft.com"))
                                    nvc.Add("MicrosoftInternal", WebUtility.UrlEncode(UserAndDomain.Substring(0, UserAndDomain.IndexOf("@"))));
                            }
                            catch
                            {
                                /* This may occur if user isn't domain user. No easy way to check that I could find so just eat exception and move on...  */
                                nvc.Add("UpnSuffix", "unknown");
                            }
                            wc.UploadValues("http://jburchelsrv.southcentralus.cloudapp.azure.com/SSASDiagUsageStats.aspx", nvc);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Exception during feature logging: " + e.Message);
                        }
                    })).Start();
            }
            catch(Exception)
            {
                // Need to handle, but don't log, since LogException tries to log through feature use also and will create loop...  For now ignoring.
                //LogException(e);
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

        private void txtStatus_SizeChanged(object sender, EventArgs e)
        {
            if (txtStatus.Text != "")
            {
                int lineHeight;
                using (Graphics g = txtStatus.CreateGraphics())
                {
                    lineHeight = TextRenderer.MeasureText(g, txtStatus.Lines.Last(), txtStatus.Font).Height;
                }
                if (lineHeight > 0)
                {
                    if (splitCollectionUI.SplitterDistance >= splitCollectionUI.Panel1MinSize + lineHeight)
                        splitCollectionUI.SplitterDistance -= (txtStatus.Height % lineHeight) - 5;
                    else
                        splitCollectionUI.SplitterDistance += lineHeight - (txtStatus.Height % lineHeight) + 5;
                }

                txtStatus.SelectionStart = txtStatus.TextLength;
                txtStatus.ScrollToCaret();
                
            }
        }

        private void frmSSASDiag_DragDrop(object sender, DragEventArgs e)
        {
            if (btnCapture.Image.Tag as string == "Play")
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (file.EndsWith(".trc") || file.EndsWith(".zip") || file.EndsWith(".etl") || file.EndsWith(".cap") || Directory.Exists(file))
                    {
                        txtFolderZipForAnalysis.Text = m_analysisPath = file;
                        LogFeatureUse("Analysis File Opened", file.Substring(file.LastIndexOf("\\") + 1));
                        PopulateAnalysisTabs();
                        tcCollectionAnalysisTabs.SelectedIndex = 1;
                        break;
                    }
                }
            }
        }

        private void frmSSASDiag_DragEnter(object sender, DragEventArgs e)
        {
            if (btnCapture.Image.Tag as string == "Play" && e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void btnHangDumps_Click(object sender, EventArgs e)
        {
            npClient.PushMessage("Dumping");
        }

        private void chkAllowUsageStatsCollection_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AllowUsageStats = Convert.ToString(chkAllowUsageStatsCollection.Checked);
            if (Environment.UserInteractive) Properties.Settings.Default.Save();
        }

        bool bExitAfterStop = false;
        private void frmSSASDiag_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (btnCapture.Image.Tag as string == "Stop" || btnCapture.Image.Tag as string == "Stop Lit" || ((string)btnCapture.Image.Tag as string) == ("Play Half Lit"))
                {
                    if (!Environment.UserInteractive || MessageBox.Show("Continue collecting data as a service until SSASDiag runs again to stop manually " + (chkStopTime.Checked ? "or the automatic stop time is reached" : "") + "?\r\n\r\nIf you select No, SSASDiag will close after collection stops immediately.", "Data collection in progress", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    {
                        btnCapture_Click(null, null);
                        bExitAfterStop = true;
                        e.Cancel = true;
                    }
                }
                else if (((string)btnCapture.Image.Tag as string) == ("Stop Half Lit"))
                {
                    if (!Environment.UserInteractive || MessageBox.Show("Disconnect this SSASDiag client from the in-progress shutdown?\r\n\r\nShutdown will continue but may take time to complete.\r\nRerun SSASDiag to monitor shutdown.", "Diagnostic shutdown in progress", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                        e.Cancel = true;
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
