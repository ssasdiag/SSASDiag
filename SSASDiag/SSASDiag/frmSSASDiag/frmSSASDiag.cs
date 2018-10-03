using Microsoft.Win32;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Text;
using System.Security.Principal;
using System.ServiceProcess;
using System;
using System.IO;
using System.Net.Security;
using System.Net.NetworkInformation;
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
        public Dictionary<string, string> Args = new Dictionary<string, string>();

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
                    if ((argArray[i].StartsWith("/") || argArray[i].StartsWith("-")) && !Args.ContainsKey(argArray[i].TrimStart(new char[] { '-', '/' }).ToLower()))
                        Args.Add(argArray[i].TrimStart(new char[] { '-', '/' }).ToLower(),
                            (argArray.Length > i + 1
                             && !(argArray[i + 1].StartsWith("/") || argArray[i + 1].StartsWith("-"))
                             && new string[] { "/instance", "/starttime", "/stoptime", "/rollover", "/workingdir", "/recurrence" }.Contains(argArray[i].ToLower())
                             ? argArray[i + 1]
                             : ""));
                    else
                    {
                        if (i > 0 && !Args.ContainsKey("filename") && File.Exists(argArray[i]))
                            Args.Add("filename", argArray[i]);
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }
            if (Args.ContainsKey("noui") && !(Args.ContainsKey("start") || Args.ContainsKey("stop")))
                Args.Remove("noui");
        }

        #region frmSSASDiagEvents

        protected override void OnLoad(EventArgs e)
        {
            if (Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").GetValue("LoggingEnabled", "False") as string == "True" || Args.ContainsKey("debug"))
            {
                enableDiagnosticLoggingToolStripMenuItem.Checked = true;
                Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").SetValue("LoggingEnabled", true);
                if (!Args.ContainsKey("debug")) Args.Add("debug", "");
            }
            else
            {
                enableDiagnosticLoggingToolStripMenuItem.Checked = false;
                Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").SetValue("LoggingEnabled", false);
            }
            enableDiagnosticLoggingToolStripMenuItem.CheckedChanged += enableDiagnosticLoggingToolStripMenuItem_CheckedChanged;

            InitializeArgs();
            if (Args.ContainsKey("noui"))
            {
                Visible = false;
                ShowInTaskbar = false;
                Opacity = 0;
            }
            base.OnLoad(e);
        }
        private void frmSSASDiag_Load(object sender, EventArgs e)
        {
            Program.SetupDebugTraceAndDumps();
            if (Args.ContainsKey("filename") && !Args.ContainsKey("start"))
            {
                tcCollectionAnalysisTabs.SelectedIndex = 1;
                txtFolderZipForAnalysis.Text = m_analysisPath = Args["filename"];
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
            string outputDir = Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").GetValue("SaveLocation", this.svcOutputPath) as string;
            if (Args.ContainsKey("outputdir"))
                outputDir = Args["outputdir"];
            if (outputDir != Environment.CurrentDirectory && outputDir != "")
            {
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);
                Environment.CurrentDirectory = outputDir;
            }

            connSqlDb = new SqlConnection("");

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
            ToolStripControlHost controlHost = new ToolStripControlHost(Recurrence);
            Recurrence.Enabled = true;
            controlHost.Padding = controlHost.Margin = RecurrenceDropDown.Margin = RecurrenceDropDown.Padding = new Padding(0);
            RecurrenceDropDown.AutoClose = false;
            RecurrenceDropDown.Items.Add(controlHost);
            HookupRecurrencePopupChildControlsClick(this);
            ttStatus.SetToolTip(lblRecurrenceDays, "Recurring schedule.");
            if (Args.ContainsKey("recurrence"))
            {
                string sched = Args["recurrence"].ToLower();
                if (sched.Replace("sa", "").Contains("s"))
                    Recurrence.chkSunday.Checked = true;
                if (sched.Contains("m"))
                    Recurrence.chkMonday.Checked = true;
                if (sched.Replace("th", "").Contains("t"))
                    Recurrence.chkTuesday.Checked = true;
                if (sched.Contains("w"))
                    Recurrence.chkWednesday.Checked = true;
                if (sched.Contains("th"))
                    Recurrence.chkThursday.Checked = true;
                if (sched.Contains("f"))
                    Recurrence.chkFriday.Checked = true;
                if (sched.Contains("sa"))
                    Recurrence.chkSaturday.Checked = true;
                if (sched.Contains("s") || sched.Contains("m") || sched.Contains("t") || sched.Contains("f"))
                    Recurrence.chkRecurringSchedule.Checked = true;
                else
                    Recurrence.chkRecurringSchedule.Checked = false;
                //pnlRecurrence.Enabled = Recurrence.chkRecurringSchedule.Checked && chkStartTime.Checked && chkStopTime.Checked;
                pnlRecurrence.BackgroundImage = (Recurrence.chkRecurringSchedule.Checked && chkStartTime.Checked && chkStopTime.Checked ? Properties.Resources.RecurrenceEnabled : Properties.Resources.RecurrenceButtonDisabled);
                lblRecurrenceDays.Visible = dtStopTime.Enabled && dtStartTime.Enabled && Recurrence.chkRecurringSchedule.Checked;
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

        private void HookupRecurrencePopupChildControlsClick(Control Parent)
        {
            foreach (Control c in Parent.Controls)
            {
                if (c != pnlRecurrence)
                {
                    HookupRecurrencePopupChildControlsClick(c);
                    c.MouseClick += RecurrencePopupClick;
                }
            }
        }

        private void RecurrencePopupClick(object sender, MouseEventArgs e)
        {
            pnlRecurrence.BackColor = SystemColors.Control;
            CloseRecurrencePopup();
        }

        private void chkAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").SetValue("AutoUpdate", automaticallyCheckForUpdatesToolStripMenuItem.Checked);
            if (automaticallyCheckForUpdatesToolStripMenuItem.Checked)
                Program.CheckForUpdates(AppDomain.CurrentDomain);
        }

        public static void SetWeakFileAssociation(string Extension, string KeyName, string OpenWith, string FileDescription, bool Unset = false)
        {
            RegistryKey BaseKey;
            RegistryKey OpenMethod;
            RegistryKey Shell;

            BaseKey = Registry.LocalMachine.CreateSubKey("Software\\Classes\\" + Extension, RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (!Unset)
            {
                BaseKey.CreateSubKey("OpenWithProgids").SetValue(KeyName, "");
                OpenMethod = Registry.LocalMachine.CreateSubKey("Software\\Classes\\" + KeyName);
                OpenMethod.SetValue("", FileDescription);
                Shell = OpenMethod.CreateSubKey("Shell");
                Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"%1\"");
                OpenMethod.Close();
                Shell.Close();
            }
            else
            {
                RegistryKey ProgIds = BaseKey.CreateSubKey("OpenWithProgids", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
                if (ProgIds != null)
                     ProgIds.DeleteValue(KeyName, false);
                Registry.LocalMachine.OpenSubKey("Software\\Classes\\", true).DeleteSubKeyTree(KeyName, false);
                ProgIds.Close();
            }
            BaseKey.Close();
            // Move the SSASDiag MRU selection to the lowest priority in the OpenWith list.  We don't want to be disruptive.  We just want to be there when the user wants us!
            BaseKey = Registry.LocalMachine.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + Extension + "\\OpenWithList", RegistryKeyPermissionCheck.Default);
            foreach (string val in BaseKey.GetValueNames())
                if (BaseKey.GetValue(val) as string == "SSASDiag.exe")
                    BaseKey.SetValue("MRUList", (BaseKey.GetValue("MRUList") as string).Replace(val, "") + val, RegistryValueKind.String);
            BaseKey.Close();

            // Tell explorer the file association has been changed
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        private void frmSSASDiag_Shown(object sender, EventArgs e)
        {
            EventWaitHandle splashScreenSignal = new EventWaitHandle(false, EventResetMode.ManualReset, "SSASDiagSplashscreenInitializedEvent");
            splashScreenSignal.Set();

            Text = "SSAS Diagnostics Tool v" + Application.ProductVersion;

            if (Args.ContainsKey("filename") && !Args.ContainsKey("start"))
            {
                tcCollectionAnalysisTabs.SelectedIndex = 1;
                txtFolderZipForAnalysis.Text = m_analysisPath = Args["filename"];
                LogFeatureUse("Analysis File Opened", Args["filename"].Substring(Args["filename"].LastIndexOf("\\") + 1));
                PopulateAnalysisTabs();
            }

            bool bUsageStatsAlreadySet = true;
            string s = Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").GetValue("AllowUsageStats", "True") as string;

            if (Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").GetValue("OpenWithEnabled", "True") as string == "True")
                enableOpenWithToolStripItem.Checked = true;
            else
                enableOpenWithToolStripItem.Checked = false;
            enableOpenWithToolStripItem_CheckedChanged(null, null);
            enableOpenWithToolStripItem.CheckedChanged += new EventHandler(enableOpenWithToolStripItem_CheckedChanged);

            if (Args.ContainsKey("reportusage"))
                enableAnonymousUsageStatisticCollectionToolStripMenuItem.Checked = true;
            else
            {
                if (Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").GetValue("AllowUsageStats", null) == null)
                {
                    bUsageStatsAlreadySet = false;
                    if (Environment.UserInteractive)
                    {
                        if (MessageBox.Show("Please help improve SSASDiag by allowing anonymous collection of usage statistics.\r\n\r\nWill you support improvements to the utility to enable now?", "Enable Collection of Anonymous Usage Statistics", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                            Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").SetValue("AllowUsageStats", true);
                        else
                            Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").SetValue("AllowUsageStats", false);
                    }
                }
                enableAnonymousUsageStatisticCollectionToolStripMenuItem.Checked = Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").GetValue("AllowUsageStats", "True") as string == "True";
            }

            if (bUsageStatsAlreadySet)
            {
                if (Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").GetValue("AutoUpdate", null) == null && Environment.UserInteractive)
                {
                    if (MessageBox.Show("Would you like to enable automatic update checks on startup?", "Enable Update Checking", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                        Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").SetValue("AutoUpdate", true);
                    else
                        Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").SetValue("AutoUpdate", false);
                }
            }

            if (Environment.UserInteractive && Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").GetValue("AutoUpdate", true) as string == "False")
                automaticallyCheckForUpdatesToolStripMenuItem.Checked = false;
            else
                automaticallyCheckForUpdatesToolStripMenuItem.Checked = true;

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
                cmbProblemType.SelectedIndex = 1;

            if (Args.ContainsKey("levelofdata"))
                try { tbLevelOfData.Value = Convert.ToInt32(Args["levelofdata"]); }
                catch { }

            LogFeatureUse("Startup", "Initialization complete.  AutoUpdate=" + automaticallyCheckForUpdatesToolStripMenuItem.Checked + ",AllowUsageStats=" + enableAnonymousUsageStatisticCollectionToolStripMenuItem.Checked);

            bFullyInitialized = true;

            pp.FormClosed += Pp_FormClosed;
        }

        private void enableDiagnosticLoggingToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").SetValue("LoggingEnabled", enableDiagnosticLoggingToolStripMenuItem.Checked);
            if (enableDiagnosticLoggingToolStripMenuItem.Checked)
                Program.SetupDebugTraceAndDumps();
            else
            {
                Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Stopping debug trace.");
                Program.ShutdownDebugTrace();
            }
            
        }
        
        public static void LogFeatureUse(string FeatureName, string FeatureDetail = "")
        {
            // For internal Microsoft users we can collect basic usage data without requiring consent.
            // For external users, we only collect with full consent.
            // For internal Microsoft users, we also collect alias to track global usage across teams primarily, and distinguish development use from genuine engineer use.
            try
            {
                if (Program.MainForm.enableAnonymousUsageStatisticCollectionToolStripMenuItem.Checked || (Environment.UserInteractive && IPGlobalProperties.GetIPGlobalProperties().DomainName.ToLower().Contains("microsoft.com")) || Environment.GetCommandLineArgs().Select(s=>s.ToLower()).Contains("/reportusage"))
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
                                string domain = IPGlobalProperties.GetIPGlobalProperties().DomainName.ToLower();
                                string domainend = domain.Substring(domain.LastIndexOf('.'));
                                domain = domain.Remove(domain.LastIndexOf('.'));
                                domain = domain.Substring(domain.LastIndexOf('.') + 1) + domainend;
                                nvc.Add("UpnSuffix", domain);
                                if (domain.EndsWith("microsoft.com"))
                                {
                                    if (Environment.UserName != "SYSTEM")
                                        nvc.Add("MicrosoftInternal", WebUtility.UrlEncode(Environment.UserName));
                                    else
                                        nvc.Add("MicrosoftInternal", WebUtility.UrlEncode(Program.LaunchingUser));
                                }
                            }
                            catch
                            {
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
            catch(Exception ex)
            {
                Debug.WriteLine("Exception during feature logging: " + ex.Message);
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

        private void txtStatus_SizeChanged(object sender, EventArgs e)
        {
            if (txtStatus.Text != "")
            {
                int lineHeight = TextRenderer.MeasureText(txtStatus.CreateGraphics(), txtStatus.Lines.Last(), txtStatus.Font).Height;
                if (lineHeight == 0) return;
                int heightDiscrepancy = ((txtStatus.Height - 5) % lineHeight);
                if (heightDiscrepancy > 0)
                {
                    if (splitCollectionUI.Panel1MinSize <= splitCollectionUI.SplitterDistance - heightDiscrepancy)
                        splitCollectionUI.SplitterDistance -= lineHeight - heightDiscrepancy;
                    else
                        splitCollectionUI.SplitterDistance += heightDiscrepancy;
                    
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
            if (npClient == null)
            {
                btnCapture.Enabled = dtStopTime.Enabled = dtStartTime.Enabled = udRollover.Enabled = btnHangDumps.Enabled = txtSaveLocation.Enabled = btnSaveLocation.Enabled = tbAnalysis.Enabled = chkZip.Enabled = chkDeleteRaw.Enabled = grpDiagsToCapture.Enabled = chkStopTime.Enabled = chkAutoRestart.Enabled = chkRollover.Enabled = chkStartTime.Enabled = udInterval.Enabled = cbInstances.Enabled = lblInterval.Enabled = lblInterval2.Enabled = false;
                txtStatus.Cursor = Cursors.WaitCursor;

                ComboBoxServiceDetailsItem cbsdi = cbInstances.SelectedItem as ComboBoxServiceDetailsItem;
                string TracePrefix = (cbsdi.Cluster ? cbsdi.Text.Replace(" (Clustered Instance)", "") : Environment.MachineName + (cbsdi == null ? "" : "_"
                        + (cbInstances.SelectedIndex == 0 ? "" : cbsdi.Text + "_")));
                string TraceID = TracePrefix
                    + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd_HH-mm-ss") + "_UTC"
                    + "_SSASDiag";
                txtStatus.Text = "Capturing three hang dumps on-demand, spaced 30s apart, at the following location:\r\n" + txtSaveLocation.Text + "\\" + TraceID;
                btnSettings.Focus();
                System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
                t.Interval = 1500;
                t.Tick += new EventHandler((p1, p2) => Invoke(new System.Action(() =>
                {
                    txtStatus.AppendText(".");
                    btnSettings.Focus();
                })));
                t.Start();
                new Thread(new ThreadStart(() =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Invoke(new System.Action(() => txtStatus.AppendText("\r\nInitiating manual memory dump collection for hang analysis at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".")));
                        try
                        {
                            if (!Directory.Exists(txtSaveLocation.Text + "\\" + TraceID + "\\HangDumps")) Directory.CreateDirectory(txtSaveLocation.Text + "\\" + TraceID + "\\HangDumps");
                            string args = CDiagnosticsCollector.GetProcessIDByServiceName(cbsdi.ServiceName) + " 0 0x34";
                            Process p = new Process();
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.FileName = Path.Combine(cbsdi.SQLSharedDir, "SQLDumper.exe");
                            p.StartInfo.WorkingDirectory = txtSaveLocation.Text + "\\" + TraceID + "\\HangDumps";
                            p.StartInfo.Arguments = args;
                            p.Start();
                            p.WaitForExit();
                        }
                        catch (Exception ex)
                        {
                            Invoke(new System.Action(() => txtStatus.AppendText("\r\nEXCEPTION DUMPING: " + ex.Message)));
                        }
                        Invoke(new System.Action(() => txtStatus.AppendText("\r\nHang dump collection finished at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".")));
                        if (i < 2)
                        {
                            Invoke(new System.Action(() =>
                            {
                                txtStatus.AppendText("\r\nWaiting 30s before capturing the next dump.");
                                btnSettings.Focus();
                            }));
                            if (Environment.GetCommandLineArgs().Where(a => a.Contains("nowaitonstop")).Count() == 0)
                                Thread.Sleep(30000);
                        }
                    }
                    Invoke(new System.Action(() =>
                    {
                        t.Stop();
                        txtStatus.AppendText("\r\nHang dump capture completed.");
                        btnHangDumps.Enabled = txtSaveLocation.Enabled = btnSaveLocation.Enabled = tbAnalysis.Enabled = chkZip.Enabled = chkDeleteRaw.Enabled = grpDiagsToCapture.Enabled = chkStopTime.Enabled = chkAutoRestart.Enabled = chkRollover.Enabled = chkStartTime.Enabled = udInterval.Enabled = cbInstances.Enabled = lblInterval.Enabled = lblInterval2.Enabled = true;
                        udRollover.Enabled = chkRollover.Checked;
                        dtStartTime.Enabled = chkStartTime.Checked;
                        dtStopTime.Enabled = chkStopTime.Checked;
                        btnCapture.Enabled = true;
                        txtStatus.Cursor = Cursors.Default;
                    }));
                })).Start();   
            }
            else
                npClient.PushMessage("Dumping");
        }

        private void tcSimpleAdvanced_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcSimpleAdvanced.SelectedIndex == 0)
                UpdateSimpleUIAfterAdvancedChanged();
        }

        private void ctxSettings_Click(object sender, EventArgs e)
        {
            ctxSettings.Show(btnSettings, new Point(btnSettings.Width - ctxSettings.Width - 3, btnSettings.Height - 3));
        }

        private void enableOpenWithToolStripItem_CheckedChanged(object sender, EventArgs e)
        {
            Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").SetValue("OpenWithEnabled", enableOpenWithToolStripItem.Checked);
            SetWeakFileAssociation(".trc", "SSASDiag Profiler Trace Analyzer", AppDomain.CurrentDomain.GetData("originalbinlocation") as string + "\\SSASDiag.exe", "SSAS Diagnostics Tool", !enableOpenWithToolStripItem.Checked);
            SetWeakFileAssociation(".etl", "SSASDiag Network Trace .etl Analyzer", AppDomain.CurrentDomain.GetData("originalbinlocation") as string + "\\SSASDiag.exe", "SSAS Diagnostics Tool", !enableOpenWithToolStripItem.Checked);
            SetWeakFileAssociation(".cap", "SSASDiag Network Trace .cap Analyzer", AppDomain.CurrentDomain.GetData("originalbinlocation") as string + "\\SSASDiag.exe", "SSAS Diagnostics Tool", !enableOpenWithToolStripItem.Checked);
            SetWeakFileAssociation(".zip", "SSASDiag Data Collection Analyzer", AppDomain.CurrentDomain.GetData("originalbinlocation") as string + "\\SSASDiag.exe", "SSAS Diagnostics Tool", !enableOpenWithToolStripItem.Checked);
            SetWeakFileAssociation(".mdmp", "SSASDiag Memory Dump Analyzer", AppDomain.CurrentDomain.GetData("originalbinlocation") as string + "\\SSASDiag.exe", "SSAS Diagnostics Tool", !enableOpenWithToolStripItem.Checked);
        }

        private void frmSSASDiag_Activated(object sender, EventArgs e)
        {
            if (StatusFloater.Visible)
                StatusFloater.BringToFront();
        }

        ucRecurrenceDialog Recurrence = new ucRecurrenceDialog();
        public ToolStripDropDown RecurrenceDropDown = new ToolStripDropDown();

        private void pnlRecurrence_MouseEnter(object sender, EventArgs e)
        {
            if (pnlRecurrence.BackgroundImage != Properties.Resources.RecurrenceButtonDisabled && dtStopTime.Enabled)
                pnlRecurrence.BackColor = SystemColors.ControlLight;
        }

        private void pnlRecurrence_MouseLeave(object sender, EventArgs e)
        {
            if (pnlRecurrence.GetChildAtPoint(pnlRecurrence.PointToClient(MousePosition)) == null && !RecurrenceDropDown.Visible)
                pnlRecurrence.BackColor = SystemColors.Control;
        }

        private void CloseRecurrencePopup()
        {
            RecurrenceDropDown.Close();
            if (Recurrence.chkRecurringSchedule.Checked)
                Recurrence.chkRecurringSchedule.Checked = Recurrence.chkSunday.Checked ||
                                                      Recurrence.chkMonday.Checked ||
                                                      Recurrence.chkTuesday.Checked ||
                                                      Recurrence.chkWednesday.Checked ||
                                                      Recurrence.chkThursday.Checked ||
                                                      Recurrence.chkFriday.Checked ||
                                                      Recurrence.chkSaturday.Checked;
        }
        private void pnlRecurrence_Click(object sender, EventArgs e)
        {
            if (RecurrenceDropDown.Visible)
            {
                CloseRecurrencePopup();
            }
            else
                RecurrenceDropDown.Show(pnlRecurrence, 5, pnlRecurrence.Height);
        }

        private void dtStartTime_ValueChanged(object sender, EventArgs e)
        {
            if (Environment.UserInteractive)
            {
                if (dtStartTime.Value < DateTime.Now.AddMinutes(2))
                    dtStartTime.Value = DateTime.ParseExact(DateTime.Now.AddMinutes(2).ToString("MM/dd/yyyy HH:mm:00"), "MM/dd/yyyy HH:mm:ss", null);
                if (dtStopTime.Value < dtStartTime.Value.AddMinutes(2))
                    dtStopTime.Value = dtStartTime.Value.AddMinutes(2);
            }
        }

        private void dtStopTime_ValueChanged(object sender, EventArgs e)
        {
            if (Environment.UserInteractive)
            {
                if (dtStopTime.Value < dtStartTime.Value)
                {
                    if (dtStopTime.Value > DateTime.Now.AddMinutes(4))
                        dtStartTime.Value = dtStopTime.Value.AddMinutes(-2);
                    else
                        dtStopTime.Value = dtStartTime.Value.AddMinutes(2);
                }
            }
        }

        private void pnlRecurrence_EnabledChanged(object sender, EventArgs e)
        {
            if (!pnlRecurrence.Enabled)
            {
                pnlRecurrence.BackgroundImage = Properties.Resources.RecurrenceButtonDisabled;
                if (btnCapture.Image.Tag!= imgPlayHalfLit.Tag)
                    lblRecurrenceDays.Text = "";
                pnlRecurrence.Width = 37;
            }
            else
            {
                if (Recurrence.chkRecurringSchedule.Checked)
                {
                    pnlRecurrence.BackgroundImage = Properties.Resources.RecurrenceEnabled;
                    Recurrence.UpdateDaysLabel();
                }
                else
                    pnlRecurrence.BackgroundImage = Properties.Resources.RecurrenceDisabled;
            }
        }

        private void chkAllowUsageStatsCollection_CheckedChanged(object sender, EventArgs e)
        {
            Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").SetValue("AllowUsageStats", enableAnonymousUsageStatisticCollectionToolStripMenuItem.Checked);
        }

        bool bExitAfterStop = false;
        private void frmSSASDiag_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (!Args.ContainsKey("noui"))
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
                    if (!e.Cancel)
                    {
                        if (tcAnalysis.TabPages.ContainsKey("Memory Dumps"))
                        {
                            ucASDumpAnalyzer da = (tcAnalysis.TabPages["Memory Dumps"].Controls[0] as ucASDumpAnalyzer);
                            if (da.btnAnalyzeDumps.Text.StartsWith("Cancel"))
                                da.btnAnalyzeDumps.PerformClick();
                            while (da.btnAnalyzeDumps.Text.StartsWith("Cancel"))
                                Thread.Sleep(50);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                // This should never happen but might if we are summarily killing midway through something.  Don't get hung up just close.
            }
            System.Diagnostics.Trace.WriteLine(Program.CurrentFormattedLocalDateTime() + ": SSASDiag form closed, Cancel set to: " + e.Cancel);
        }
        private void BgDetachProfilerDB_DoWork(object sender, DoWorkEventArgs e)
        {
            DettachProfilerTraceDB();
        }
        private void BgDetachProfilerDB_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusFloater.Hide();
            BeginInvoke(new System.Action(()=>Close()));
        }
        private void frmSSASDiag_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                btnSettings.Left = Width - btnSettings.Width - 15;
                pnlLinks.Top = Height - 59;
                pnlLinks.Left = Width / 2 - pnlLinks.Width / 2;
                txtFolderZipForAnalysis.Width = Width - 164;
                tcCollectionAnalysisTabs.Height = Height - 59;
                tcAnalysis.Height = Height - 119;
                btnImportProfilerTrace.Left = Width / 2 - btnImportProfilerTrace.Width / 2;
                txtProfilerAnalysisQuery.Width = Width - 254;
                if (splitProfilerStatus.SplitterDistance > splitProfilerStatus.Height - 200)
                    splitProfilerStatus.SplitterDistance = splitProfilerStatus.Height - 200;
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
        }
        public static void LogException(Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
            LogFeatureUse("Exception", "Message:\n" + ex.Message + "\n at stack:\n" + ex.StackTrace);   
        }
        #endregion frmSSASDiagEvents

        #region FeedbackUI
        private void lkFeedback_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("mailto:ssasdiagchamps@microsoft.com?subject=Feedback on SSAS Diagnostics Collector Tool&cc=jburchel@microsoft.com");
        }
        private void lkDiscussion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/ssasdiag/SSASDiag/issues");
        }
        private void lkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmAbout f = new frmAbout();
            f.ShowDialog(this);
        }
        private void lkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmCmdLineUsage f = new frmCmdLineUsage();
            f.ShowDialog(this);
        }

        #endregion FeedbackUI
    }
}
