using System.IO.MemoryMappedFiles;
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
using System.IO.Pipes;
using NamedPipeWrapper;

namespace SSASDiag
{
    public partial class frmSSASDiag : Form
    {
        string svcOutputPath = "";
        NamedPipeClient<string> npClient;
        frmPasswordPrompt pp = new frmPasswordPrompt();

        [DllImport("advapi32.dll")]
        public static extern int ImpersonateNamedPipeClient(int hNamedPipe);

        private void btnCapture_Click(object sender, EventArgs e)
        {
            // worker we use to launch either start or stop blocking operations to the CDiastnosticsCollector asynchronously
            BackgroundWorker bg = new BackgroundWorker();

            if (dc == null || dc.bRunning || !(btnCapture.Image.Tag as string).Contains("Half Lit"))
            {
                if (btnCapture.Image.Tag as string == "Play" || btnCapture.Image.Tag as string == "Play Lit")
                {
                    new Thread(new ThreadStart(() => DettachProfilerTraceDB())).Start();  // Dettach any existing data from analysis because we're capturing new data now.

                    // Adjust UI to startup.
                    InitializeCaptureUI();
                    ComboBoxServiceDetailsItem cbsdi = cbInstances.SelectedItem as ComboBoxServiceDetailsItem;
                    string TracePrefix = (cbsdi.Cluster ? cbsdi.Text.Replace(" (Clustered Instance)", "") : Environment.MachineName + (cbsdi == null ? "" : "_"
                        + (cbInstances.SelectedIndex == 0 ? "" : cbsdi.Text + "_")));

                    if (cbsdi.ServiceName == "PowerBIReportServer")
                        LogFeatureUse("PBIRS Collection");

                    // Unhook the status text area from selection while we are actively using it.
                    // I do allow selection after but it was problematic to scroll correctly while allowing user selection during active collection.
                    // This is functionally good, allows them to copy paths or file names after completion but also gives nice behavior during collection.
                    txtStatus.Cursor = Cursors.Arrow;
                    txtStatus.GotFocus += txtStatus_GotFocusWhileRunning;
                    txtStatus.Enter += txtStatus_EnterWhileRunning;
                    
                    if (!Environment.UserInteractive)
                    {
                        if (Args.ContainsKey("workingdir")) Environment.CurrentDirectory = Args["workingdir"];
                        string InstanceName = cbsdi.Text.Replace("Default instance (", "").Replace(" (Clustered Instance", "").Replace(")", "");
                        svcOutputPath = Program.TempPath + "SSASDiagService_" + InstanceName + ".output.log";
                        if (File.Exists(svcOutputPath))
                            File.Delete(svcOutputPath);
                        dc = new CDiagnosticsCollector(TracePrefix, (cbsdi == null ? "" : (InstanceName.ToUpper() == "MSSQLSERVER" ? "" : InstanceName)), cbsdi.ServiceName, (InstanceName == "Power BI Report Server" ? PBIRSPort : cbsdi.InstanceID), cbsdi.SQLProgramDir, cbsdi.SQLSharedDir, m_instanceVersion, m_instanceType, m_instanceEdition, m_ConfigDir, m_LogDir, (cbsdi == null ? null : cbsdi.ServiceAccount),
                            txtStatus,
                            (int)udInterval.Value, chkAutoRestart.Checked, chkZip.Checked, chkDeleteRaw.Checked, chkProfilerPerfDetails.Checked, chkXMLA.Checked, chkABF.Checked, chkBAK.Checked, (int)udRollover.Value, chkRollover.Checked, dtStartTime.Value, chkStartTime.Checked, dtStopTime.Value, chkStopTime.Checked,
                            chkGetConfigDetails.Checked, chkGetProfiler.Checked, chkGetPerfMon.Checked, chkGetNetwork.Checked, cbsdi.Cluster, svcOutputPath);
                        while (!dc.npServer._connections.Exists(c => c.IsConnected))
                            Thread.Sleep(100);
                        
                        LogFeatureUse("Collection", "InstanceVersion=" + m_instanceVersion + ",InstanceType=" + m_instanceType + ",InstanceEdition=" + m_instanceEdition + ",PerfMonInterval=" + udInterval.Value + ",AutoRestartProfiler=" + chkAutoRestart.Checked +
                                                    ",UseZip=" + chkZip.Checked + ",DeleteRawDataAfterZip=" + chkDeleteRaw.Checked + ",IncludeProfilerVerbosePerfDetails=" + chkProfilerPerfDetails.Checked +
                                                    ",IncludeXMLA=" + chkXMLA.Checked + ",IncludeABF=" + chkABF.Checked + ",IncludeBAK=" + chkBAK.Checked + (chkRollover.Checked ? ",RolloverMB=" + udRollover.Value : "") +
                                                    (chkStartTime.Checked ? ",StartTime=" + dtStartTime.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") : "") +
                                                    (chkStopTime.Checked ? ",StopTime=" + dtStopTime.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") : "")
                                                    + ",ConfigDetails=" + chkGetConfigDetails.Checked + ",Profiler=" + chkGetProfiler.Checked + ",PerfMon=" + chkGetPerfMon.Checked + ",NetworkTrace=" + chkGetNetwork.Checked + ",RunningAsService=" + !Environment.UserInteractive + ",Clustered=" + cbsdi.Cluster);
                        dc.CompletionCallback = callback_StartDiagnosticsComplete;
                        new Thread(new ThreadStart(() => dc.StartDiagnostics())).Start();
                    }
                    else
                    {
                        // Notify UI of start and start UI timer...

                        string InstanceName = cbInstances.Text.Replace("Default instance (", "").Replace(" (Clustered Instance", "").Replace(")", "");
                        string sInstanceServiceConfig = Program.TempPath + "SSASDiagService_" + InstanceName + ".exe";
                        svcOutputPath = sInstanceServiceConfig.Substring(0, sInstanceServiceConfig.IndexOf(".exe")) + ".output.log";
                        
                        string sMsg = "Initializing SSAS diagnostics collection at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\nInstalling collection service SSASDiag_" + InstanceName + ".";
                        txtStatus.Text = sMsg;
                        tPumpUIUpdatesPreServiceStart.Interval = 1000;
                        tPumpUIUpdatesPreServiceStart.Tick += TPumpUIUpdatesPreServiceStart_Tick;
                        tPumpUIUpdatesPreServiceStart.Start();

                        new Thread(new ThreadStart(() =>
                        {
                            // Install the service for this instance

                            File.Copy(Program.TempPath + "SSASDiagService.exe", sInstanceServiceConfig, true);
                            File.Copy(Program.TempPath + "SSASDiagService.ini", sInstanceServiceConfig.Replace(".exe", ".ini"), true);
                            List<string> svcconfig = new List<string>(File.ReadAllLines(sInstanceServiceConfig.Replace(".exe", ".ini")));
                            svcconfig[svcconfig.FindIndex(s => s.StartsWith("ServiceName="))] = "ServiceName=SSASDiag_" + InstanceName;
                            svcconfig[svcconfig.FindIndex(s => s.StartsWith("ServiceLongName="))] = "ServiceLongName=SQL Server Analysis Services Diagnostic Collection Service (" + InstanceName + ")";
                            svcconfig[svcconfig.FindIndex(s => s.StartsWith("ServiceDesc="))] = "ServiceDesc=Launch SSASDiag.exe to administer data collection.  SSASDiag provides automated diagnostic collection for SQL Server Analysis Services.";
                            svcconfig[svcconfig.FindIndex(s => s.StartsWith("WorkingDir="))] = "WorkingDir=" + (AppDomain.CurrentDomain.GetData("originalbinlocation") as string);
                            File.WriteAllLines(sInstanceServiceConfig.Replace(".exe", ".ini"), svcconfig.ToArray());
                            ProcessStartInfo p = new ProcessStartInfo(sInstanceServiceConfig);
                            p.CreateNoWindow = true;
                            p.Verb = "runas"; // ensures elevation of priv
                            p.WindowStyle = ProcessWindowStyle.Hidden;
                            p.Arguments = "-i";
                            Process proc = Process.Start(p);
                            proc.WaitForExit();

                            // Setup the service startup parameters according to user selections
                            svcconfig[svcconfig.FindIndex(s => s.StartsWith("CommandLine="))]
                                =
                                "CommandLine=" + (AppDomain.CurrentDomain.GetData("originalbinlocation") as string) + "\\SSASDiag.exe" +
                                " /workingdir \"" + txtSaveLocation.Text + "\"" +
                                (chkZip.Checked ? " /zip" : "") +
                                " /instance \"" + (InstanceName == "" ? "MSSQLServer" : InstanceName) + "\"" +
                                (chkDeleteRaw.Checked ? " /deleteraw" : "") +
                                (chkRollover.Checked ? " /rollover " + udRollover.Value : "") +
                                (chkStartTime.Checked ? " /starttime \"" + dtStartTime.Value.ToString("MM/dd/yyyy HH:mm:ss") + "\"" : "") +
                                (chkStopTime.Checked ? " /stoptime \"" + dtStopTime.Value.ToString("MM/dd/yyyy HH:mm:ss") + "\"" : "") +
                                (chkAutoRestart.Checked ? " /autorestartprofiler" : "") +
                                " /perfmoninterval " + udInterval.Value +
                                (chkGetConfigDetails.Checked ? " /config" : "") +
                                (chkGetPerfMon.Checked ? " /perfmon" : "") +
                                (chkGetProfiler.Checked ? " /profiler" : "") +
                                (chkProfilerPerfDetails.Checked ? " /verbose" : "") +
                                (chkABF.Checked ? " /abf" : "") +
                                (chkBAK.Checked ? " /bak" : "") +
                                (chkXMLA.Checked ? " /xmla" : "") +
                                (chkGetNetwork.Checked ? " /network" : "") +
                                (Args.ContainsKey("nowaitonstop") ? " /nowaitonstop" : "") +
                                (Args.ContainsKey("debug") ? " /debug" : "") +
                                (enableAnonymousUsageStatisticCollectionToolStripMenuItem.Checked ? " /reportusage" : "") +
                                " /outputdir \"" + Environment.CurrentDirectory + "\"" +
                                " /start";
                            File.WriteAllLines(sInstanceServiceConfig.Replace(".exe", ".ini"), svcconfig.ToArray());

                            if (npClient != null)
                            {
                                npClient.ServerMessage -= NpClient_ServerMessage;
                                npClient = null;
                            }
                            txtStatus.Invoke(new System.Action(()=> txtStatus.Text += "\r\nStarting service..."));
                            npClient = new NamedPipeClient<string>("SSASDiag_" + InstanceName);
                            npClient.ServerMessage += NpClient_ServerMessage;
                            npClient.Start();
                            npClient.PushMessage("Initialize Pipe");
                            string svcName = "SSASDiag_" + InstanceName;
                            new Thread(new ThreadStart(() =>
                            {
                                p = new ProcessStartInfo("cmd.exe", "/c net start \"" + svcName + "\"");
                                p.WindowStyle = ProcessWindowStyle.Hidden;
                                p.Verb = "runas"; // ensures elevation of priv
                                p.CreateNoWindow = true;
                                Program.ShutdownDebugTrace();
                                Process.Start(p).WaitForExit();
                                txtStatus.Invoke(new System.Action(()=> txtStatus.AppendText("\r\nCollection service SSASDiag_" + cbInstances.Text.Replace("Default instance (", "").Replace(" (Clustered Instance", "").Replace(")", "") + " is running.\r\nCollection initializing...")));
                            })).Start();
                        })).Start();
                    }
                }
                else if (btnCapture.Image.Tag as string == "Stop" || btnCapture.Image.Tag as string == "Stop Lit" || (btnCapture.Image.Tag as string == "Play Half Lit" && Args.ContainsKey("stop") && Args.ContainsKey("noui")))
                {
                    btnCapture.Click -= btnCapture_Click;
                    btnCapture.Image = imgStopHalfLit;
                    btnHangDumps.Enabled = false;
                    if (!Environment.UserInteractive)
                        new Thread(new ThreadStart(() => dc.StopAndFinalizeAllDiagnostics())).Start();
                    else
                        npClient.PushMessage("Stop");
                }
            }
        }

        System.Windows.Forms.Timer tPumpUIUpdatesPreServiceStart = new System.Windows.Forms.Timer();

        private void TPumpUIUpdatesPreServiceStart_Tick(object sender, EventArgs e)
        {
            Invoke(new System.Action(() =>
            {
                // output a "... ... ... " pattern as sign of life while starting/stopping stuff
                int s = DateTime.Now.Second;
                if (s % 4 != 0)
                    txtStatus.AppendText(".");
                else
                    txtStatus.AppendText(" ");
            }));
        }

        private void InitializeCaptureUI()
        {
            btnHangDumps.Visible = true;
            btnCapture.Image = imgPlayHalfLit;
            btnCapture.Click -= btnCapture_Click;
            tbAnalysis.ForeColor = SystemColors.ControlDark;
            tcCollectionAnalysisTabs.Refresh();
            btnHangDumps.Enabled = txtSaveLocation.Enabled = btnSaveLocation.Enabled = tbAnalysis.Enabled = chkZip.Enabled = chkDeleteRaw.Enabled = grpDiagsToCapture.Enabled = dtStopTime.Enabled = chkStopTime.Enabled = chkAutoRestart.Enabled = dtStartTime.Enabled = chkRollover.Enabled = chkStartTime.Enabled = udRollover.Enabled = udInterval.Enabled = cbInstances.Enabled = lblInterval.Enabled = lblInterval2.Enabled = false;
        }

        string svcName = "";
        private void NpClient_ServerMessage(NamedPipeConnection<string, string> connection, string message)
        {
            try
            {
                if (tPumpUIUpdatesPreServiceStart != null && tPumpUIUpdatesPreServiceStart.Enabled == true)
                {
                    tPumpUIUpdatesPreServiceStart.Stop();
                    tPumpUIUpdatesPreServiceStart.Tick -= TPumpUIUpdatesPreServiceStart_Tick;
                }

                if (message == "Initialize pipe")
                {
                    connection.PushMessage("Initialize pipe");
                    return;
                }

                if (message.StartsWith("\r\nInitialized service for trace with ID: "))
                {
                    Invoke(new System.Action(() =>
                    {
                        string InstanceName = cbInstances.Text.Replace("Default instance (", "").Replace(" (Clustered Instance", "").Replace(")", "");
                        txtStatus.Text = "Initializing SSAS diagnostics collection at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n"
                                        + "Collection service SSASDiag_" + InstanceName + " started.";
                        InitializeCaptureUI();
                        if (Args.ContainsKey("noui"))
                        {
                            Invoke(new System.Action(() => Close()));
                        }
                    }));
                }

                if (message.StartsWith("\r\nCreated temporary folder "))
                {
                    txtFolderZipForAnalysis.Invoke(new System.Action(() =>
                        txtFolderZipForAnalysis.Text = m_analysisPath = Environment.CurrentDirectory + "\\" + message.Replace("\r\nCreated temporary folder ", "").Replace(" to collect diagnostic files.", "") + (chkZip.Checked && chkDeleteRaw.Checked ? ".zip" : "")
                        ));
                }

                if (message.StartsWith("\r\nDiagnostics captured for ") || // && LastStatusLine.StartsWith("Diagnostics captured for ")) ||
                        message.StartsWith("\r\nTime remaining until collection starts: ")) //&& LastStatusLine.StartsWith("Time remaining until collection starts: ")))
                {
                    string LastStatusLine = "";
                    txtStatus.Invoke(new System.Action(() =>
                                                            {
                                                                if (txtStatus.Text.Length > 0)
                                                                    LastStatusLine = txtStatus.Lines.Last();
                                                            }));
                    if (LastStatusLine != "" && LastStatusLine.StartsWith(message.Substring(2, 20)))
                        txtStatus.Invoke(new System.Action(() =>
                        {
                            txtStatus.Text = txtStatus.Text.Replace(LastStatusLine, message.Replace("\r\n", ""));
                            txtStatus.SelectionStart = txtStatus.TextLength;
                            txtStatus.ScrollToCaret();
                        }
                        ));
                    else
                    {
                        Invoke(new System.Action(() =>
                        {
                            if (btnCapture.Image.Tag as string == "Play Half Lit")
                            {
                                btnHangDumps.Enabled = true;
                                btnCapture.Image = imgStop;
                                btnCapture.Click += btnCapture_Click;
                            }
                        }));
                        txtStatus.Invoke(new System.Action(() => txtStatus.AppendText(message)));
                    }
                }
                else if (message.StartsWith("\r\nWaiting for client interaction:\r\n"))
                {
                    string uiMsg = message.Replace("\r\nWaiting for client interaction:\r\n", "");
                    if (uiMsg.StartsWith("Windows Administrator required for remote server:") && pp != null)
                    {
                        if (uiMsg.EndsWith("TryingAgain"))
                        {
                            uiMsg = uiMsg.Replace("TryingAgain", "");
                            pp.lblUserPasswordError.Visible = true;
                        }
                        pp.UserMessage = uiMsg;
                        Invoke(new System.Action(() =>
                        {
                            pp.Top = Top + Height / 2 - pp.Height / 2;
                            pp.Left = Left + Width / 2 - pp.Width / 2;
                        }));
                        Invoke(new System.Action(() =>
                        {
                            if (!pp.IsDisposed)
                                pp.Show();
                            Enabled = false;
                        }
                        ));
                    }
                }
                else if (message == "Dumping")
                {
                    btnCapture.Image = imgStopHalfLit;
                    btnCapture.Click -= btnCapture_Click;
                    btnHangDumps.Invoke(new System.Action(() => { btnHangDumps.Enabled = false; }));
                }
                else if (message == "DumpingOver")
                {
                    btnCapture.Image = imgStop;
                    btnCapture.Click += btnCapture_Click;
                    btnHangDumps.Invoke(new System.Action(() => { btnHangDumps.Enabled = true; }));
                }
                else if (message == "\r\nStop")
                {
                    Invoke(new System.Action(() =>
                    {

                        callback_StopAndFinalizeAllDiagnosticsComplete();

                        ProcessStartInfo p = null;
                        try
                        {
                            string InstanceName = cbInstances.Text.Replace("Default instance (", "").Replace(" (Clustered Instance", "").Replace(")", "");
                            svcName = "SSASDiag_" + InstanceName;
                            // Stop the service via command line.
                            p = new ProcessStartInfo("cmd.exe", "/c net stop \"" + svcName + "\"");
                            p.WindowStyle = ProcessWindowStyle.Hidden;
                            p.UseShellExecute = true;
                            p.Verb = "runas";
                            p.CreateNoWindow = true;
                            
                            Process.Start(p);
                            if (Environment.UserInteractive)
                            {
                                // Uninstall service.  We already got the Stop message indicating we're done closing, so the net stop command will finish very quickly.  But give it a second.  Better than blocking and not worth implementing a callback on this...
                                p = new ProcessStartInfo("reg.exe", @"query ""HKLM\System\CurrentControlSet\Services\" + svcName + @""" /v ImagePath");
                                p.UseShellExecute = false;
                                p.CreateNoWindow = true;
                                p.WindowStyle = ProcessWindowStyle.Hidden;
                                p.RedirectStandardOutput = true;
                                p.RedirectStandardError = true;
                                Process proc = Process.Start(p);
                                string SvcPath = proc.StandardOutput.ReadToEnd();

                                if (SvcPath == "")
                                {
                                    string err = proc.StandardError.ReadToEnd();
                                    throw new Exception("Exception getting service path: " + err);
                                }
                                else
                                {
                                    SvcPath = SvcPath.Replace("\r\nHKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Services\\" + svcName + "\r\n    ImagePath    REG_EXPAND_SZ    ", "").Replace("\r\n", "");
                                }
                                
                                p = new ProcessStartInfo("cmd.exe", "/c ping 1.1.1.1 -n 1 -w 2000 > nul & \"" + SvcPath + "\" -u");
                                p.WindowStyle = ProcessWindowStyle.Hidden;
                                p.Verb = "runas";
                                p.UseShellExecute = true;
                                p.CreateNoWindow = true;
                                Process.Start(p);
                                if (Args.ContainsKey("noui"))
                                    Invoke(new System.Action(() => Close()));

                                if (npClient != null)
                                    npClient.ServerMessage -= NpClient_ServerMessage;
                                npClient = null;
                            }
                        }
                        catch (Exception e)
                        {
                            LogException(e);
                        }
                    }));
                }
                else if (message.Contains("Cancelled by client"))
                {
                    if (pp != null && pp.Visible)
                    {
                        pp.DialogResult = DialogResult.Abort;
                        Invoke(new System.Action(() => pp.Close()));
                    }
                }
                else
                    txtStatus.Invoke(new System.Action(() =>
                    {
                        txtStatus.AppendText(message);
                        txtStatus.SelectionStart = txtStatus.TextLength;
                        txtStatus.ScrollToCaret();
                    }));

                if (pp != null && pp.InvokeRequired)
                {
                    if (message.StartsWith("\r\nWindows authentication for remote SQL data source "))
                        Invoke(new System.Action(() => pp.lblUserPasswordError.Text = "Incorrect user name or password"));
                    if (message.StartsWith("\r\nAuthenticated user "))
                        Invoke(new System.Action(() => pp.lblUserPasswordError.Text = "User unauthorized to database "));
                    if (message.StartsWith("\r\nRemote file share access failed for user "))
                        Invoke(new System.Action(() => pp.lblUserPasswordError.Text = "User unauthorized to remote share"));
                    Invoke(new System.Action(() => pp.lblUserPasswordError.Left = pp.Width / 2 - pp.lblUserPasswordError.Width / 2));
                }

                if (message.StartsWith("\r\nStopping collection at "))
                    btnCapture.Image = imgStopHalfLit;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in NPClient_ServerMessage");
                LogException(ex);
            }
        }

        private void Pp_FormClosed(object sender, FormClosedEventArgs e)
        {
            Invoke(new System.Action(() => Enabled = true));
            if (pp.DialogResult != DialogResult.Abort)
            {
                try
                {
                    // We will get our dialog closed by notifcation from the service if another client gives credentials before we complete ourselves...
                    string SvcPath = Registry.LocalMachine.OpenSubKey("SYSTEM\\ControlSet001\\Services\\" + svcName, false).GetValue("ImagePath") as string;
                    List<string> CurrentStatus = File.ReadLines(SvcPath.Substring(0, SvcPath.Length - 4) + ".output.log").ToList();
                    CurrentStatus = CurrentStatus.GetRange(0, CurrentStatus.Count - 4);
                    File.WriteAllText(svcOutputPath, String.Join("\r\n", CurrentStatus).TrimEnd(new char[] { '\r', '\n' }));
                }
                catch (Exception ex) { LogException(ex); }
            }
            if (pp.DialogResult == DialogResult.OK)
                npClient.PushMessage("Administrator=" + pp.User.Trim() + ";Domain=" + pp.Domain.Trim() + ";Password=" + pp.Password.Trim());
            if (pp.DialogResult == DialogResult.Cancel)
                npClient.PushMessage("Cancelled by client");
        }

        private string[] WriteSafeReadAllLines(String path)
        {
            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                List<string> file = new List<string>();
                while (!sr.EndOfStream)
                    file.Add(sr.ReadLine());
                return file.ToArray();
            }
            else
                return new string[] { "" };
        }

        #region BlockingUIComponentsBesidesCapture

        class ComboBoxServiceDetailsItem
        {
            public string Text { get; set; }
            public string ConfigPath { get; set; }
            public string InstanceID { get; set; }
            public string ServiceName { get; set; }
            public string SQLProgramDir { get; set; }
            public string SQLSharedDir { get; set; }
            public string ServiceAccount { get; set; }
            public bool Cluster { get; set; }
        }
        BackgroundWorker bgPopulateInstanceDetails;
        private void cbInstances_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reset text if we actually did change selected instances.  Check that by comparing the current global ConfigDir location kept for the active instance
            // with that config location stored in the ComboBoxServiceDetailsItem associated with the instance in the combobox SelectedItem member.
            ComboBoxServiceDetailsItem cdi = cbInstances.SelectedItem as ComboBoxServiceDetailsItem;
            if (cdi != null && cdi.ConfigPath != m_ConfigDir)
                txtStatus.ResetText();
            
            // Only act if we are already fully initialized
            if (cbInstances.DisplayMember != "" && tcCollectionAnalysisTabs.SelectedIndex == 0)
            {
                btnCapture.Enabled = btnHangDumps.Enabled = false;
                bgPopulateInstanceDetails = new BackgroundWorker();
                bgPopulateInstanceDetails.DoWork += BgPopulateInstanceDetails_DoWork;
                bgPopulateInstanceDetails.RunWorkerCompleted += BgPopulateInstanceDetails_RunWorkerCompleted;
                bgPopulateInstanceDetails.RunWorkerAsync();
            }
        }

        private void BgPopulateInstanceDetails_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Server srv = new Server();
                ComboBoxServiceDetailsItem SelItem = cbInstances.Invoke(new Func<ComboBoxServiceDetailsItem>(() => { return (cbInstances.SelectedItem as ComboBoxServiceDetailsItem); })) as ComboBoxServiceDetailsItem;
                if (SelItem != null)
                {
                    srv.Connect("Data source=" + (SelItem.Cluster ? SelItem.Text.Replace(" (Clustered Instance)", "") : Environment.MachineName + (SelItem.Text == "Default instance (MSSQLServer)" ? "" : (SelItem.Text == "Power BI Report Server" ? ":" + PBIRSPort : "\\" + SelItem.Text))) + ";Timeout=0;Integrated Security=SSPI;SSPI=NTLM;", true);
                    System.Diagnostics.Trace.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Connected to server with connection string: " + srv.ConnectionString);
                    lblInstanceDetails.Invoke(new System.Action(() => lblInstanceDetails.Text = "Instance Details:\r\n" + srv.Version + " (" + srv.ProductLevel + "), " + srv.ServerMode + ", " + srv.Edition));
                    m_instanceType = srv.ServerMode.ToString();
                    m_instanceVersion = srv.Version + " - " + srv.ProductLevel;
                    m_instanceEdition = srv.Edition.ToString();
                    m_LogDir = srv.ServerProperties["LogDir"].Value.TrimEnd('\\');
                    m_ConfigDir = SelItem.ConfigPath;
                    srv.Disconnect();

                    if (Environment.UserInteractive)
                    {
                        // Does the instance already have an existing data collection in progress??
                        try
                        {
                            ServiceController InstanceCollectionService = null;
                            string svcName = "";
                            string InstanceName = SelItem.Text.Replace("Default instance (", "").Replace(" (Clustered Instance", "").Replace(")", "");
                            cbInstances.Invoke(new System.Action(() => svcName = "SSASDiag_" + InstanceName));
                            InstanceCollectionService = new ServiceController(svcName);
                            RegistryKey svcKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\ControlSet001\\Services\\" + svcName);
                            if (InstanceCollectionService != null && svcKey != null)
                            {
                                string svcPath = svcKey.GetValue("ImagePath") as string;
                                svcOutputPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\TEMP\\SSASDiag\\SSASDiagService_" + InstanceName + ".output.log";
                                List<string> CurrentStatus = new List<string>();
                                if (File.Exists(svcOutputPath))
                                    CurrentStatus = File.ReadAllLines(svcOutputPath).ToList();
                                string capturedelapsedtime = "";
                                if (CurrentStatus.Where(l => l.StartsWith("Diagnostics captured for ")).Count() > 0)
                                {
                                    capturedelapsedtime = CurrentStatus.Where(l => l.StartsWith("Diagnostics captured for ")).Last();
                                    CurrentStatus = CurrentStatus.Where(l => !l.StartsWith("Diagnostics captured for ")).ToList();
                                }
                                string RawStatusText = String.Join("\r\n", CurrentStatus.ToArray()).TrimStart(new char[] { '\r', '\n' });
                                // If we encounter a stopped service, this indicates unexpected halt in prior state.  Report and deliver service log to output directory, then clean up old service.
                                if (InstanceCollectionService.Status == ServiceControllerStatus.Stopped)
                                {
                                    if (capturedelapsedtime != "") CurrentStatus.Add(capturedelapsedtime);
                                    if (CurrentStatus.Count > 0 && CurrentStatus.Last() == "Stop")
                                    {
                                        // Service stopped normally and is just awaiting cleanup by any client.  
                                        // Display its status log and go through normal stop/cleanup procedure for this, the first client to connect since it stopped.
                                        // In the future it would be better to not use strings but constants for different message types (control/status/etc.) and send a message class through the pipe.
                                        // But this is simpler and easier and low priority.  Later if we try to localize or whatnot we could have a little work to fix it up though...
                                        CurrentStatus.RemoveAt(CurrentStatus.Count - 1);
                                        RawStatusText = String.Join("\r\n", CurrentStatus.ToArray()).TrimStart(new char[] { '\r', '\n' });
                                        txtStatus.Invoke(new System.Action(() => txtStatus.Text = RawStatusText));
                                        NpClient_ServerMessage(null, "\r\nStop");
                                        if (!Args.ContainsKey("noui"))
                                            MessageBox.Show("Status log displayed for the last collection since no clients were connected when it completed.",
                                                        "Prior collection completed" + CurrentStatus.Last().Replace("SSASDiag completed", "").TrimEnd('.') + ".",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Information);
                                    }
                                    else
                                    {
                                        // This state should only be reached if there was an unexpected interruption like service or server crash...
                                        txtStatus.Invoke(new System.Action(() => txtStatus.Text = RawStatusText));
                                        NpClient_ServerMessage(null, "\r\nStop");
                                        if (!Args.ContainsKey("noui"))
                                            MessageBox.Show("Partial log displayed for the last collection,\r\nwhich terminated unexpectedly.",
                                                        "Prior collection terminated without complete shutdown",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Warning);
                                    }
                                }
                                // If we are running, starting or stopping, get current status and connect pipe to continue monitoring.
                                else
                                {
                                    if (npClient != null)
                                    {
                                        npClient.ServerMessage -= NpClient_ServerMessage;
                                        npClient = null;
                                    }
                                    cbInstances.Invoke(new System.Action(() => npClient = new NamedPipeClient<string>(svcName)));
                                    npClient.ServerMessage += NpClient_ServerMessage;
                                    npClient.Start();
                                    npClient.WaitForConnection();

                                    callback_StartDiagnosticsComplete();
                                    Invoke(new System.Action(() => txtSaveLocation.Enabled = btnSaveLocation.Enabled = tbAnalysis.Enabled = chkZip.Enabled = chkDeleteRaw.Enabled = grpDiagsToCapture.Enabled = dtStopTime.Enabled = chkStopTime.Enabled = chkAutoRestart.Enabled = dtStartTime.Enabled = chkRollover.Enabled = chkStartTime.Enabled = udRollover.Enabled = udInterval.Enabled = cbInstances.Enabled = lblInterval.Enabled = lblInterval2.Enabled = false));
                                    if (RawStatusText.Contains("Diagnostics captured for") && !CurrentStatus.Last().Contains("Diagnostics captured for"))
                                        // We're stopping...
                                        btnCapture.Image = imgStopHalfLit;
                                    if (!RawStatusText.Contains("Diagnostics captured for"))
                                        // We're starting up still...
                                        btnCapture.Image = imgPlayHalfLit;
                                    if (CurrentStatus.Count > 0 && CurrentStatus[CurrentStatus.Count - 4] == "Waiting for client interaction:")
                                    {
                                        string uiMsg = String.Join("\r\n", CurrentStatus.GetRange(CurrentStatus.Count - 3, 3)).Replace("TryingAgain", "");
                                        CurrentStatus = CurrentStatus.GetRange(0, CurrentStatus.Count - 4);
                                        txtStatus.Invoke(new System.Action(() => txtStatus.Text = String.Join("\r\n", CurrentStatus).TrimEnd(new char[] { '\r', '\n' })));
                                        if (!Args.ContainsKey("noui"))
                                            MessageBox.Show("Connecting to the in-progress data capture running for this instance.\r\nThe data capture is currently awaiting user interaction.",
                                                        "Existing data capture in-progress",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Information);
                                    }
                                    else
                                    {
                                        txtStatus.Invoke(new System.Action(() => txtStatus.Text = RawStatusText));
                                        // If we aren't waiting for client interaction but service is not in a stoped state, then just update the status and continue monitoring as a newly connected client.
                                        if (!Args.ContainsKey("noui"))
                                            MessageBox.Show("Connecting to the in-progress data capture running for this instance.",
                                                        "Existing data capture in-progress",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Information);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }
                    btnCapture.Invoke(new System.Action(() => btnCapture.Enabled = btnHangDumps.Enabled = true));
                }
                else
                    Debug.WriteLine("Selected item had no service details in BgPopulateInstanceDetails_DoWork!!!");
            }
            catch (Exception ex)
            {
                if (ex.Message != "A connection cannot be made. Ensure that the server is running.")
                    LogException(ex);
                if (!lblInstanceDetails.IsDisposed) lblInstanceDetails.Invoke(new System.Action(() => lblInstanceDetails.Text = "Instance details could not be obtained due to failure connecting:\r\n" + ex.Message));
            }
        }

        private void BgPopulateInstanceDetails_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((Args.ContainsKey("stop") && !(btnCapture.Image.Tag as string == "Stop" || btnCapture.Image.Tag as string == "Stop Lit" || btnCapture.Image.Tag as string == "Play Half Lit")) ||
                (Args.ContainsKey("start") && !(btnCapture.Image.Tag as string == "Play" || btnCapture.Image.Tag as string == "Play Lit")))
                Invoke(new System.Action(() => Close()));
            else if (btnCapture.Enabled && cbInstances.Items.Count > 0 && 
                (Args.ContainsKey("start") || Args.ContainsKey("stop")))
                btnCapture_Click(sender, e);
        }

        private void PopulateInstanceDropdown()
        {
            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += bgPopulateInstanceDropdown;
            bg.RunWorkerCompleted += bgPopulateInstanceDropdownComplete;
            bg.RunWorkerAsync();
        }

        string PBIRSPort = "";
        private void bgPopulateInstanceDropdown(object sender, DoWorkEventArgs e)
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                foreach (ServiceController s in services.OrderBy(ob => ob.DisplayName))
                {
                    if (s.DisplayName.StartsWith("SQL Server Analysis Services") && !s.DisplayName.Contains("SQL Server Analysis Services CEIP (") && !s.DisplayName.Contains("Diagnostic Collection Service"))
                    {
                        SelectQuery sQuery = new SelectQuery("select name, startname, pathname from Win32_Service where name = \"" + s.ServiceName + "\"");
                        ManagementObjectSearcher mgmtSearcher = new ManagementObjectSearcher(sQuery);
                        string sSvcUser = "";
                        foreach (ManagementObject svc in mgmtSearcher.Get())
                            sSvcUser = svc["startname"] as string;
                        if (sSvcUser.Contains(".")) sSvcUser = sSvcUser.Replace(".", Environment.UserDomainName);
                        if (sSvcUser == "LocalSystem") sSvcUser = "NT AUTHORITY\\SYSTEM";

                        string ConfigPath = Registry.LocalMachine.OpenSubKey("SYSTEM\\ControlSet001\\Services\\" + s.ServiceName, false).GetValue("ImagePath") as string;
                        ConfigPath = ConfigPath.Substring(ConfigPath.IndexOf("-s \"") + "-s \"".Length).TrimEnd('\"');
                        string InstanceID = s.DisplayName.Replace("SQL Server Analysis Services (", "").Replace(")", "");
                        InstanceID = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\OLAP", false).GetValue(InstanceID, "") as string;
                        if (InstanceID == "") InstanceID = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\OLAP", false).GetValue("MSSQLSERVER") as string;
                        string SQLProgramDir = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\" + InstanceID + @"\Setup", false).GetValue("SQLProgramDir") as string;
                        string Ver = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\" + InstanceID + @"\Setup", false).GetValue("Version") as string;
                        string SQLSharedDir = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\" + Ver.Substring(0, 2) + "0", false).GetValue("SharedCode") as string;
                        string ClusterName = "";
                        RegistryKey clusterKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\" + InstanceID + @"\Cluster", false);
                        if (clusterKey != null)
                            ClusterName = clusterKey.GetValue("ClusterName") as string;
                        if (s.DisplayName.Replace("SQL Server Analysis Services (", "").Replace(")", "").ToUpper() == "MSSQLSERVER")
                            LocalInstances.Insert(0, new ComboBoxServiceDetailsItem() { Text = "Default instance (MSSQLServer)", ConfigPath = ConfigPath, ServiceAccount = sSvcUser, InstanceID = InstanceID, SQLProgramDir = SQLProgramDir, ServiceName = s.ServiceName, Cluster = false, SQLSharedDir = SQLSharedDir });
                        else
                            LocalInstances.Add(new ComboBoxServiceDetailsItem() { Text = (ClusterName == "" ? s.DisplayName.Replace("SQL Server Analysis Services (", "").Replace(")", "") : ClusterName + " (Clustered Instance)"), ConfigPath = ConfigPath, ServiceAccount = sSvcUser, InstanceID = InstanceID, SQLProgramDir = SQLProgramDir, ServiceName = s.ServiceName, Cluster = (ClusterName == "" ? false : true), SQLSharedDir = SQLSharedDir });
                    }
                }
                SelectQuery sQuery2 = new SelectQuery("select name, startname, pathname from Win32_Service where name = \"PowerBIReportServer\"");
                ManagementObjectSearcher mgmtSearcher2 = new ManagementObjectSearcher(sQuery2);
                string PBIRSconfigPath = "";
                foreach (ManagementObject svc in mgmtSearcher2.Get())
                    PBIRSconfigPath = svc["pathname"] as string;
                if (PBIRSconfigPath != "")
                {
                    PBIRSconfigPath = PBIRSconfigPath.Replace("RSHostingService.exe\"", "config.json").TrimStart('\"');
                    PBIRSPort = File.ReadAllText(PBIRSconfigPath);
                    PBIRSPort = PBIRSPort.Substring(PBIRSPort.IndexOf("\"ASPort\": \"") + "\"ASPort\": \"".Length);
                    PBIRSPort = PBIRSPort.Substring(0, PBIRSPort.IndexOf("\""));
                    string ASDir = PBIRSconfigPath.Replace("RSHostingService\\config.json", "ASEngine");
                    LocalInstances.Add(new ComboBoxServiceDetailsItem() { Text = "Power BI Report Server", Cluster = false, ServiceName = "PowerBIReportServer", ServiceAccount = "PowerBIReportServer", SQLProgramDir = ASDir, SQLSharedDir = ASDir, ConfigPath = ASDir, InstanceID = PBIRSPort});
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Failure during instance enumeration - could be because no instances were there.  Move on quietly then.");
                System.Diagnostics.Trace.WriteLine(ex);
            }
            if (LocalInstances.Count == 0)
                cbInstances.Invoke(new System.Action(() => cbInstances.Enabled = false));
        }
        private void bgPopulateInstanceDropdownComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (cbInstances.SelectedIndex > -1)
                svcName = "SSASDiag_" + (cbInstances.SelectedIndex == 0 ? "MSSQLSERVER" : cbInstances.Text); 
            cbInstances.DataSource = LocalInstances;
            cbInstances.DisplayMember = "Text";
            cbInstances.Refresh();
            if (LocalInstances.Count == 0)
                lblInstanceDetails.Text = "There were no Analysis Services instances found on this server.\r\nPlease run on a server with a SQL 2008 or later SSAS instance.";
            else
            {
                if (Args.ContainsKey("instance"))
                {
                    string instance = Args["instance"].ToLower().TrimEnd().TrimStart();
                    int i = LocalInstances.FindIndex(c => c.Text.ToLower() == instance || c.Text.ToLower() == instance + " (clustered instance)");
                    if (i > 0)
                        cbInstances.SelectedIndex = i;
                }
            }

            cbInstances.SelectedIndexChanged += cbInstances_SelectedIndexChanged;
            cbInstances_SelectedIndexChanged(null, null);
        }
        
        #endregion BlockingUIComponentsBesidesCapture

        #region VariousNonBlockingUIElements

        private void btnSaveLocation_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            fbd.Description = "Select save location for capture.";
            fbd.SelectedPath = txtSaveLocation.Text;
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                Registry.CurrentUser.CreateSubKey(@"Software\SSASDiag").SetValue("SaveLocation", fbd.SelectedPath);
                Environment.CurrentDirectory = txtSaveLocation.Text = fbd.SelectedPath;
            }
        }
        private void txtStatus_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                string sOut = "";
                if (txtStatus.SelectedText.Length == 0)
                {
                    sOut += txtStatus.Text;
                    ttStatus.Show("Output window text copied to clipboard.", txtStatus, 2500);
                }
                else
                {
                    ttStatus.Show("Selection text copied to clipboard.", txtStatus, 2500);
                    sOut = txtStatus.SelectedText;
                }
                Clipboard.SetData(DataFormats.StringFormat, sOut);
                new Thread(new ThreadStart(new System.Action(() =>
                {
                    Thread.Sleep(2500);
                    txtStatus.Invoke(new System.Action(() => ttStatus.SetToolTip(txtStatus, "")));
                }))).Start();
            }
        }
        private void splitCollectionUI_Panel1_Resize(object sender, EventArgs e)
        {
            grpDiagsToCapture.Height = splitCollectionUI.Panel1.Height - grpDiagsToCapture.Top;
            grpDiagsToCapture.Width = splitCollectionUI.Panel1.Width - grpDiagsToCapture.Left;
            rtbProblemDescription.Width = tabGuided.Width - rtbProblemDescription.Left;
            rtbProblemDescription.Height = tabGuided.Height - rtbProblemDescription.Top;
        }
        private void btnCapture_MouseEnter(object sender, EventArgs e)
        {
            if (btnCapture.Image.Tag as string == "Play")
                btnCapture.Image = imgPlayLit;
            else if (btnCapture.Image.Tag as string == "Stop")
                btnCapture.Image = imgStopLit;
        }
        private void btnCapture_MouseLeave(object sender, EventArgs e)
        {
            if (btnCapture.Image.Tag as string == "Play Lit")
                btnCapture.Image = imgPlay;
            else if (btnCapture.Image.Tag as string == "Stop Lit")
                btnCapture.Image = imgStop;
        }
        private void tcCollectionAnalysisTabs_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = tcCollectionAnalysisTabs.TabPages[e.Index];
            e.Graphics.FillRectangle(new SolidBrush(page.BackColor), e.Bounds);
            Rectangle paddedBounds = e.Bounds;
            int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
            paddedBounds.Offset(1, yOffset);
            TextRenderer.DrawText(e.Graphics, page.Text, Font, paddedBounds, page.ForeColor);
        }

        #endregion VariousNonBlockingUIElements

        #region CaptureDetailsUI

        private void chkRollover_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRollover.Checked) udRollover.Enabled = true; else udRollover.Enabled = false;
        }
        private void chkStopTime_CheckedChanged(object sender, EventArgs e)
        {
            dtStopTime.Enabled = chkStopTime.Checked;
            if (!chkStopTime.Checked)
            {
                if (chkAutoRestart.Checked) ttStatus.Show("AutoRestart disabled for your protection without stop time.", chkAutoRestart, 1750);
                chkAutoRestart.Checked = false;
            }
            else
                dtStopTime.Value = DateTime.Now.AddHours(1);
        }
        private void chkStartTime_CheckedChanged(object sender, EventArgs e)
        {
            dtStartTime.Enabled = chkStartTime.Checked;
            if (chkStartTime.Checked) dtStartTime.Value = DateTime.Now.AddHours(0);
        }
        private void chkAutoRestart_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoRestart.Checked && !chkStopTime.Checked)
            {
                ttStatus.Show("Stop time required for your protection with AutoRestart=true.", dtStopTime, 1750);
                chkStopTime.Checked = true;
            }
        }
        private void chkZip_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkZip.Checked)
                chkDeleteRaw.Checked = false;
        }
        private void chkDeleteRaw_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDeleteRaw.Checked)
            {
                ttStatus.Show("After zip, keep raw data for analysis.\r\nThis saves the zip decompression step later.", chkDeleteRaw, 4000);
                chkZip.Checked = true;
            }
        }
        private void SetRolloverAndStartStopEnabledStates()
        {
            chkRollover.Enabled = chkStartTime.Enabled = chkStopTime.Enabled = dtStartTime.Enabled = dtStopTime.Enabled
                = chkGetPerfMon.Checked | chkGetProfiler.Checked | chkGetNetwork.Checked;
            udRollover.Enabled = chkRollover.Enabled & chkRollover.Checked;
            dtStartTime.Enabled = chkStartTime.Enabled & chkStartTime.Checked;
            dtStopTime.Enabled = chkStopTime.Enabled & chkStopTime.Checked;
        }

        #endregion CaptureDetailsUI
    }
}
