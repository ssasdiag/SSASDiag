using System.Reflection;
using System.Net;
using System.Resources;
using System.Collections;
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
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;


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
        bool bClosing = false;
        
        #endregion locals

        public frmSSASDiag()
        {
            InitializeComponent();
        }

        #region frmSSASDiagEvents
        private void frmSSASDiag_Load(object sender, EventArgs e)
        {
            SetupDebugTrace();

            if (!(Environment.OSVersion.Version.Major >= 7 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1)))
            {
                MessageBox.Show("Network trace collection requires\nWindows 7 or Server 2008 R2 or greater.\nPlease upgrade your OS to use that feature.", "SSAS Diagnotics Network Trace Incompatibility Warning", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                chkGetNetwork.Enabled = false;
            }

            imgPlay.Tag = "Play"; imgPlayLit.Tag = "Play Lit"; imgPlayHalfLit.Tag = "Play Half Lit"; imgStop.Tag = "Stop"; imgStopLit.Tag = "Stop Lit"; imgStopHalfLit.Tag = "Stop Half Lit";
            btnCapture.Image = imgPlay;
            Environment.CurrentDirectory = AppDomain.CurrentDomain.GetData("originalbinlocation") as string;
            if (Properties.Settings.Default["SaveLocation"] as string != Environment.CurrentDirectory && Properties.Settings.Default["SaveLocation"] as string != "")
                Environment.CurrentDirectory = Properties.Settings.Default["SaveLocation"] as string;

            PopulateInstanceDropdown();
            dtStopTime.Value = DateTime.Now.AddHours(1);
            dtStopTime.MinDate = DateTime.Now.AddMinutes(1);
            dtStopTime.CustomFormat += TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours > 0 ? "+" 
                + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString() : TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString();
            dtStartTime.CustomFormat = dtStopTime.CustomFormat;
            dtStartTime.MinDate = DateTime.Now;
            dtStartTime.MaxDate = DateTime.Now.AddDays(30);
            cmbProblemType.SelectedIndex = 0;
            tmScrollStart.Interval = 250;
            tmScrollStart.Tick += tmLevelOfDataScroll_Tick;
            frmSSASDiag_Resize(this, e);

            foreach (TabPage t in tcAnalysis.TabPages)
                HiddenTabPages.Add(t);
            for (int i = 0; i < tcAnalysis.TabPages.Count; i++)
                tcAnalysis.TabPages.RemoveAt(0);

            ProfilerTraceAnalysisQueries = InitializeProfilerTraceAnalysisQueries();
            cmbProfilerAnalyses.DataSource = ProfilerTraceAnalysisQueries;
            cmbProfilerAnalyses.DisplayMember = "Name";

            txtSaveLocation.Text = Environment.CurrentDirectory;

            AnalysisMessagePumpTimer.Tick += AnalysisMessagePumpTimer_Tick;
            AnalysisQueryExecutionPumpTimer.Tick += AnalysisQueryExecutionPumpTimer_Tick;

            SetupSQLTextbox();
        }

        private void chkAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoUpdate.Checked)
                Program.CheckForUpdates(AppDomain.CurrentDomain);
            Properties.Settings.Default.AutoUpdate = Convert.ToString(chkAutoUpdate.Checked);
            Properties.Settings.Default.Save();
        }

        private void frmSSASDiag_Shown(object sender, EventArgs e)
        {
            bool bUsageStatsAlreadySet = true;

            if (Properties.Settings.Default.AllowUsageStats == "")
            {
                bUsageStatsAlreadySet = false;
                if (MessageBox.Show("Please help improve SSASDiag by allowing anonymous collection of usage statistics.\r\n\r\nWill you support improvements to the utility to enable now?", "Enable Collection of Anonymous Usage Statistics", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    Properties.Settings.Default.AllowUsageStats = "true";
                else
                    Properties.Settings.Default.AllowUsageStats = "false";
                Properties.Settings.Default.Save();
            }
            chkAllowUsageStatsCollection.Checked = Convert.ToBoolean(Properties.Settings.Default.AllowUsageStats);

            if (bUsageStatsAlreadySet)
            {
                if (Properties.Settings.Default.AutoUpdate == "")
                {
                    if (MessageBox.Show("Would you like to enable automatic update checks on startup?", "Enable Update Checking", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                        Properties.Settings.Default.AutoUpdate = "true";
                    else
                        Properties.Settings.Default.AutoUpdate = "false";
                    Properties.Settings.Default.Save();
                }
            }

            if (Properties.Settings.Default.AutoUpdate!= "true")
                chkAutoUpdate.Checked = false;
            else
                chkAutoUpdate.Checked = true;
        }

        private void SetupDebugTrace()
        {
            if (Environment.GetCommandLineArgs().Select(s => s.ToLower()).Contains("/debug"))
                System.Diagnostics.Trace.Listeners.Add(new TextWriterTraceListener(AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\SSASDiagDebugTrace_" + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd_HH-mm-ss") + "_UTC" + ".log"));
            System.Diagnostics.Trace.Listeners.Add(new UsageStatsCollectorTraceListener(this));
            System.Diagnostics.Trace.AutoFlush = true;
            System.Diagnostics.Trace.WriteLine("Started diagnostic trace.");
        }

        private void chkAllowUsageStatsCollection_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AllowUsageStats = Convert.ToString(chkAllowUsageStatsCollection.Checked);
            Properties.Settings.Default.Save();
        }

        [ConfigurationElementType(typeof(CustomTraceListenerData))]
        public class UsageStatsCollectorTraceListener : CustomTraceListener
        {
            frmSSASDiag MainForm;
            public UsageStatsCollectorTraceListener(frmSSASDiag mainForm) : base() { MainForm = mainForm; }
            public override void Write(string message)
            {
                WriteLine(message);
            }

            public override void WriteLine(string message)
            {
                if (MainForm.chkAllowUsageStatsCollection.Checked)
                    new Thread(new ThreadStart(() =>
                    {
                        WebClient wc = new WebClient();
                        wc.OpenRead(new Uri("http://jburchelsrv.southcentralus.cloudapp.azure.com/SSASDiagUsageStats.aspx" +
                                                              "?UsageVersion=" + WebUtility.UrlEncode(FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).FileVersion) +
                                                              "&UsageAction=" + WebUtility.UrlEncode(message)));
                    })).Start();
            }

            public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
            {
                if (data is LogEntry && this.Formatter != null)
                {
                    this.WriteLine(this.Formatter.Format(data as LogEntry));
                }
                else
                {
                    this.WriteLine(data.ToString());
                }
            }

        }

        private void frmSSASDiag_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (btnCapture.Image.Tag as string == "Stop" || btnCapture.Image.Tag as string == "Stop Lit")
                {
                    if (MessageBox.Show("Capture in progress, exiting will stop.\r\nExit anyway?", "Capture in progress", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        bClosing = true;
                        btnCapture_Click(sender, e);
                    }
                    e.Cancel = true;
                }
                else if (((string)btnCapture.Image.Tag as string).Contains("Half Lit"))
                    if (MessageBox.Show("Diagnostic Capture is in a blocking state.\nForcing exit now may leave locked files and traces in progress.\n\nExit anyway?", "Capture in progress", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        if (Application.OpenForms.Count > 1)
                            Application.OpenForms["PasswordPrompt"].Invoke(new System.Action(()=> Application.OpenForms["PasswordPrompt"].Close()));
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
