using System.ServiceProcess;
using System.Reflection;
using Microsoft.Win32;
using System.IO.MemoryMappedFiles;
using Ionic.Zip;
using Microsoft.AnalysisServices;
using Microsoft.Win32.SafeHandles;
using PdhNative;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using System.Xml;
using System.IO.Pipes;
using NamedPipeWrapper;
using System.Threading;
using System.Linq;

namespace SSASDiag
{
    public class CDiagnosticsCollector : INotifyPropertyChanged
    {
        #region publics
        public bool bScheduledStartPending = false, bRunning = false, bPerfMonRunning = false, bCluster = false;
        public System.Action CompletionCallback;
        public NamedPipeServer<string> npServer;
        public string TraceID;
        #endregion publics

        #region toomanylocals
        
        PdhHelper m_PdhHelperInstance = new PdhHelper(false);
        System.Timers.Timer PerfMonAndUIPumpTimer = new System.Timers.Timer();
        RichTextBox txtStatus;
        DateTime m_StartTime = DateTime.Now;
        string sTracePrefix = "", sInstanceName, sInstanceID, sASServiceName, sInstanceVersion, sInstanceMode, sInstanceEdition, sLogDir, sConfigDir, sServiceAccount, sRemoteAdminUser, sRemoteAdminDomain, sSQLProgramDir, sSQLSharedDir, sRecurrencePattern;
        SecureString sRemoteAdminPassword;
        int iInterval = 0, iRollover = 0, iCurrentTimerTicksSinceLastInterval = 0, iSecondsSinceLastHangCheck = 0;
        bool bAutoRestart = false, bRollover = false, bUseStart, bUseEnd, bGetConfigDetails, bGetProfiler, bGetXMLA, bGetABF, bGetBAK, bGetPerfMon, bGetNetwork, bCompress = true, bDeleteRaw = true, bPerfEvents = true, bAutomaticHangDumps = false;
        bool bCollectionFullyInitialized = false, bSuspendUITicking = false;
        DateTime dtStart, dtEnd;
        string svcOutputPath = "";
        EventWaitHandle clientWaiter = new EventWaitHandle(false, EventResetMode.AutoReset);
        #endregion toomanylocals

        #region Win32
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
        int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);
        [Flags]
        private enum EventExportLogFlags
        {
            ChannelPath = 1,
            LogFilePath = 2,
            TolerateQueryErrors = 0x1000
        };

        [DllImport(@"wevtapi.dll",
            CallingConvention = CallingConvention.Winapi,
            CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern bool EvtExportLog(
            IntPtr sessionHandle,
            string path,
            string query,
            string targetPath,
            [MarshalAs(UnmanagedType.I4)] EventExportLogFlags flags);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll")]
        public static extern int ImpersonateNamedPipeClient(int hNamedPipe);
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool RevertToSelf();
        [DllImport("kernel32.dll")]
        static extern uint GetLastError();
        #endregion Win32

        public CDiagnosticsCollector(
                string TraceFilesPrefix, string InstanceName, string ASServiceName, string InstanceID, string SQLProgramDir, string SQLSharedDir, string InstanceVersion, string InstanceMode, string InstanceEdition, string ConfigDir, string LogDir, string ServiceAccount,
                RichTextBox StatusTextBox,
                int Interval, 
                bool AutoRestart, bool Compress, bool DeleteRaw, bool IncludePerfEventsInProfiler, bool IncludeXMLA, bool IncludeABF, bool IncludeBAK,
                int RolloverMaxMB, bool Rollover, 
                DateTime Start, bool UseStart, 
                DateTime End, bool UseEnd, 
                string RecurrencePattern,
                bool GetConfigDetails, bool GetProfiler, bool GetPerfMon, bool GetNetwork, bool Cluster, bool AutomaticHangDumps, string ServiceOutputPath)
        {
            PerfMonAndUIPumpTimer.Interval = 1000;
            PerfMonAndUIPumpTimer.Elapsed += CollectorPumpTick;
            sTracePrefix = TraceFilesPrefix; sInstanceName = InstanceName; sInstanceVersion = InstanceVersion; sInstanceMode = InstanceMode; sInstanceEdition = InstanceEdition; sConfigDir = ConfigDir; sLogDir = LogDir; sServiceAccount = ServiceAccount; sSQLProgramDir = SQLProgramDir; sSQLSharedDir = SQLSharedDir; sInstanceID = InstanceID; sASServiceName = ASServiceName; sRecurrencePattern = RecurrencePattern;
            txtStatus = StatusTextBox;
            bGetXMLA = IncludeXMLA; bGetABF = IncludeABF; bGetBAK = IncludeBAK;
            iInterval = Interval;
            bAutoRestart = AutoRestart; bCompress = Compress; bDeleteRaw = DeleteRaw; bPerfEvents = IncludePerfEventsInProfiler; bCluster = Cluster; bAutomaticHangDumps = AutomaticHangDumps;
            iRollover = RolloverMaxMB; bRollover = Rollover;
            dtStart = Start; bUseStart = UseStart;
            dtEnd = End; bUseEnd = UseEnd;
            bGetConfigDetails = GetConfigDetails; bGetProfiler = GetProfiler; bGetNetwork = GetNetwork; bGetPerfMon = GetPerfMon;
            svcOutputPath = ServiceOutputPath;
            PipeSecurity ps = new PipeSecurity();
            ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null) as IdentityReference, PipeAccessRights.FullControl, AccessControlType.Allow));
            ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), PipeAccessRights.ReadWrite | PipeAccessRights.Synchronize, AccessControlType.Allow));
            npServer = new NamedPipeServer<string>("SSASDiag_" + (InstanceName == "" ? "MSSQLSERVER" : InstanceName), ps);
            npServer.ClientMessage += npServer_ClientMessage;
            npServer.ClientConnected += NpServer_ClientConnected;
            npServer.Start();
        }

        private void NpServer_ClientConnected(NamedPipeConnection<string, string> connection)
        {
            connection.PushMessage("Initialize pipe");
        }

        private SecureString GetSecureString(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;
            else
            {
                SecureString result = new SecureString();
                foreach (char c in source.ToCharArray())
                    result.AppendChar(c);
                return result;
            }
        }

        String SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        bool bForceStop = false;
        private void npServer_ClientMessage(NamedPipeConnection<string, string> connection, string message)
        {
            if (message == "Initialize pipe")
            {
                ImpersonateNamedPipeConnection(connection);
                clientWaiter.Set();
                ImpersonateNamedPipeConnection(connection);
                if (Environment.UserName != "SYSTEM")
                    Program.LaunchingUser = Environment.UserName;
            }
            if (message.StartsWith("Administrator="))
            {
                string[] KeyVals = message.Split(';');
                sRemoteAdminUser = KeyVals[0].Split('=')[1];
                sRemoteAdminDomain = KeyVals[1].Split('=')[1];
                sRemoteAdminPassword = GetSecureString(KeyVals[2].Split('=')[1]);
                clientWaiter.Set();
            }
            if (message == "Cancelled by client")
            {
                SendMessageToClients("Cancelled by client");
                sRemoteAdminUser = "Cancelled by client";
                sRemoteAdminDomain = "";
                sRemoteAdminPassword = GetSecureString("");
                clientWaiter.Set();
            }
            if (message == "Stop")
            {
                bForceStop = true;
                StopAndFinalizeAllDiagnostics();
            }
            else if (message == "Dumping")
                CaptureHangDumps();
        }

        private void CaptureHangDumps()
        {
            npServer.PushMessage("Dumping");
            bCollectionFullyInitialized = false;
            uint pid = GetProcessIDByServiceName(sASServiceName);
            if (pid == 0)
            {
                SendMessageToClients("Service process not found when attempting to capture hang dumps.");
                bCollectionFullyInitialized = true;
                npServer.PushMessage("DumpingOver");
                return;
            }

            new Thread(new ThreadStart(() =>
            {
                SendMessageToClients("Capturing three hang dumps on-demand, spaced 30s apart.");
                for (int i = 0; i < 3; i++)
                {
                    SendMessageToClients("Initiating manual memory dump collection for hang analysis at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                    try
                    {
                        if (!Directory.Exists(Environment.CurrentDirectory + "\\" + TraceID + "\\HangDumps")) Directory.CreateDirectory(Environment.CurrentDirectory + "\\" + TraceID + "\\HangDumps");
                        string args = pid + " 0 0x34";
                        Process p = new Process();
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = Path.Combine(sSQLSharedDir, "SQLDumper.exe");
                        p.StartInfo.WorkingDirectory = Environment.CurrentDirectory + "\\" + TraceID + "\\HangDumps";
                        p.StartInfo.Arguments = args;
                        Debug.WriteLine(args);
                        p.Start();
                        p.WaitForExit();
                    }
                    catch (Exception e)
                    {
                        SendMessageToClients("EXCEPTION DUMPING: " + e.Message);
                    }
                    SendMessageToClients("Hang dump collection finished at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                    if (i < 2)
                    {
                        SendMessageToClients("Waiting 30s before capturing dump " + (i + 2) + ".");
                        if (Environment.GetCommandLineArgs().Where(a => a.Contains("nowaitonstop")).Count() == 0)
                            Thread.Sleep(30000);
                    }
                }
                bCollectionFullyInitialized = true;
                npServer.PushMessage("DumpingOver");
            })).Start();
        }

        public static uint GetProcessIDByServiceName(string serviceName)
        {
            uint processId = 0;
            string qry = "SELECT PROCESSID FROM WIN32_SERVICE WHERE NAME = '" + serviceName + "'";
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(qry);
            foreach (System.Management.ManagementObject mngntObj in searcher.Get())
            {
                processId = (uint)mngntObj["PROCESSID"];
            }
            return processId;
        }

        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        #endregion Properties

        #region StartCapture
        public void StartDiagnostics()
        {
            bCollectionFullyInitialized = false;
            while (!npServer._connections.Exists(c => c.IsConnected))
                System.Threading.Thread.Sleep(100);
            m_StartTime = DateTime.Now;
            bScheduledStartPending = false;
            bRunning = true;

            // Start the timer ticking...
            PerfMonAndUIPumpTimer.Start();

            if (bUseStart && DateTime.Now < dtStart)
            {
                SendMessageToClients("Scheduled diagnostic collection starts automatically at " + dtStart.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                if (bUseEnd)
                {
                    SendMessageToClients("Collection scheduled to stop automatically at " + dtEnd.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                    if (sRecurrencePattern != "")
                    {
                        SendMessageToClients("Collection schedule recurs on the following days: " + 
                                             ((sRecurrencePattern.Replace("Sa", "").Contains("S") ? "Sun, " : "") +
                                             (sRecurrencePattern.Contains("M") ? "Mon, " : "") +
                                             (sRecurrencePattern.Replace("Th", "").Contains("T") ? "Tue, " : "") +
                                             (sRecurrencePattern.Contains("W") ? "Wed, " : "") +
                                             (sRecurrencePattern.Contains("Th") ? "Thu, " : "") +
                                             (sRecurrencePattern.Contains("F") ? "Fri, " : "") +
                                             (sRecurrencePattern.Contains("Sa") ? "Sat, " : "")).TrimEnd(' ').TrimEnd(','));
                    }

                }
                TimeSpan ts = dtStart - DateTime.Now;
                SendMessageToClients("Time remaining until collection starts: " + ts.ToString("dd\\:hh\\:mm\\:ss"));
                bScheduledStartPending = true;
                return;
            }
            else
            {
                TraceID = sTracePrefix
                    + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd_HH-mm-ss") + "_UTC"
                    + "_SSASDiag";
                if (bUseStart)
                {
                    SendMessageToClients("Scheduled diagnostic collection starting automatically at " + dtStart.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                    if (bUseEnd)
                    {
                        SendMessageToClients("Collection scheduled to stop automatically at " + dtEnd.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                        if (sRecurrencePattern != "")
                        {
                            SendMessageToClients("The collection schedule will recur on each of the following days:");
                            SendMessageToClients(((sRecurrencePattern.Replace("Sa", "").Contains("S") ? "Sunday, " : "") +
                                                 (sRecurrencePattern.Contains("M") ? "Monday, " : "") +
                                                 (sRecurrencePattern.Replace("Th", "").Contains("T") ? "Tuesday, " : "") +
                                                 (sRecurrencePattern.Contains("W") ? "Wednesday, " : "") +
                                                 (sRecurrencePattern.Contains("Th") ? "Thursday, " : "") +
                                                 (sRecurrencePattern.Contains("F") ? "Friday, " : "") +
                                                 (sRecurrencePattern.Contains("Sa") ? "Saturday, " : "")).TrimEnd(' ').TrimEnd(','));
                        }
                    }
                }
                SendMessageToClients("Initialized service for trace with ID: " + TraceID);

                SendMessageToClients("Collecting on computer " + Environment.MachineName + ".");
                if (sInstanceVersion != "")  // This occurs when we aren't really capturing instance details with Network only capture.
                {
                    SendMessageToClients("Collecting for instance " + (sInstanceName == "" ? "Default instance (MSSQLServer)" : (sInstanceName == "Power BI Report Server" ? ":" + sInstanceID : sInstanceName)) + (bCluster ? " (Clustered Instance)" : "") + ".");
                    SendMessageToClients("The version of the instance is " + sInstanceVersion + ".");
                    SendMessageToClients("The edition of the instance is " + sInstanceEdition + ".");
                    SendMessageToClients("The instance mode is " + sInstanceMode + ".");
                    SendMessageToClients("The log folder for the instance is " + sLogDir + ".");
                    SendMessageToClients("The msmdsrv.ini configuration for the instance at " + sConfigDir + ".");
                }

                if (!Directory.Exists(TraceID))
                    Directory.CreateDirectory(TraceID);
                else
                {
                    if (!Environment.UserInteractive || MessageBox.Show("There is an existing output directory found.\r\nDelete these files to start with a fresh output folder?", "Existing output folder found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try { Directory.Delete(TraceID, true); } catch { }
                        Directory.CreateDirectory(TraceID);
                    }
                }
                SendMessageToClients("Created temporary folder " + TraceID + " to collect diagnostic files.");

                // Add explicit full control access for AS service account to our temp output location since server trace is written under that identity.
                if (sInstanceVersion != "")
                {
                    try
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(Environment.CurrentDirectory + "\\" + TraceID );
                        DirectorySecurity dirSec = dirInfo.GetAccessControl();
                        dirSec.AddAccessRule(new FileSystemAccessRule(sServiceAccount, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
                        dirInfo.SetAccessControl(dirSec);
                        SendMessageToClients("Added full control for SSAS service account " + sServiceAccount + " to the output directory.");
                    }
                    catch (Exception ex)
                    {
                        SendMessageToClients("Adding access permissions for SSAS service account " + sServiceAccount + " to output folder failed:\n\t" + ex.Message + "");
                        frmSSASDiag.LogException(ex);
                    }
                }

                if (bAutomaticHangDumps)
                    SendMessageToClients("Checking every 30s to initiate hang dumps automatically if the server becomes unresponsive.");

                if (bGetConfigDetails)
                {
                    // Collect SSAS LOG dir, config and save log of this diagnostic capture
                    if (!Directory.Exists(TraceID + "\\Log")) Directory.CreateDirectory(TraceID + "\\Log");
                    foreach (string f in Directory.GetFiles(sLogDir))
                        File.Copy(f, TraceID + "\\Log" + f.Substring(f.LastIndexOf("\\")));
                    File.Copy(sConfigDir + "\\msmdsrv.ini", TraceID + "\\msmdsrv.ini");
                    SendMessageToClients("Captured OLAP\\Log contents and msmdsrv.ini config for the instance.");

                    if (sInstanceName != "Power BI Report Server")
                    {
                        BackgroundWorker bg = new BackgroundWorker();
                        bg.DoWork += bgGetSPNs;
                        bg.RunWorkerCompleted += bgGetSPNsCompleted;
                        bg.RunWorkerAsync();
                    }
                    else bgGetSPNsCompleted(null, null);
                }
                else
                    bgGetSPNsCompleted(null, null);
            }                   
        }

        bool ImpersonateNamedPipeConnection(NamedPipeConnection<string, string> conn, bool NotifyClients = true)
        {
            NamedPipeWrapper.IO.PipeStreamWrapper<string, string> pipe = GetPrivateField<NamedPipeWrapper.IO.PipeStreamWrapper<string, string>>(conn, "_streamWrapper");
            int h = pipe.BaseStream.SafePipeHandle.DangerousGetHandle().ToInt32();
            RevertToSelf();
            if (ImpersonateNamedPipeClient(h) != 0)
                return true;
            else
            {
                if (NotifyClients)
                    SendMessageToClients("Failure impersonating client connection.  Win32 error code was: " + GetLastError());
                return false;
            }
        }

        private void bgGetSPNs(object sender, DoWorkEventArgs e)
        {
            SendMessageToClients("Attempting to capture SPNs for the AS service account " + sServiceAccount + ".");
            string sErr = "", sOut = "";
            foreach (NamedPipeConnection<string, string> conn in npServer._connections)
            {
                if (conn.IsConnected)
                {
                    if (ImpersonateNamedPipeConnection(conn, false))
                    {
                        Process p = new Process();
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.RedirectStandardError = true;
                        p.StartInfo.FileName = "setspn";
                        p.StartInfo.Arguments = "-l \"" + sServiceAccount + "\"";
                        p.Start();
                        sOut = p.StandardOutput.ReadToEnd();
                        sErr = p.StandardError.ReadToEnd();
                        p.WaitForExit();
                        if (sErr == "")
                        {
                            File.WriteAllText(Environment.CurrentDirectory + "\\" + TraceID + "\\ServiceAccountSPNs.txt", sOut);
                            SendMessageToClients("Captured SPNs defined for service account " + sServiceAccount + ".");
                            break;
                        }
                    }
                }
            }
            if (sOut == "")
                SendMessageToClients("Failed to capture SPNs.  Rerun SSASDiag as a domain administrator if this configuration detail is required.");
        }

        private void bgGetSPNsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (bGetPerfMon)
            {
                uint r = InitializePerfLog(TraceID + "\\" + TraceID + ".blg");

                if (r != 0)
                {
                    SendMessageToClients("Error starting PerfMon logging: " + r.ToString("X") + "");
                    SendMessageToClients("Other diagnostic collection will still be attempted.");
                }
                else
                {
                    bPerfMonRunning = true;
                    SendMessageToClients("Performance logging every " + iInterval + " seconds.");
                    SendMessageToClients("Performance logging started to file: " + TraceID + ".blg.");
                }
            }

            if (bGetProfiler)
            {
                string XMLABatch = (bPerfEvents ? Properties.Resources.ProfilerTraceStartWithQuerySubcubeEventsXMLA : Properties.Resources.ProfilerTraceStartXMLA)
                    .Replace("<LogFileName/>", "<LogFileName>" + Environment.CurrentDirectory + "\\" + TraceID + "\\" + TraceID + ".trc</LogFileName>")
                    .Replace("<LogFileSize/>", bRollover ? "<LogFileSize>" + iRollover + "</LogFileSize>" : "")
                    .Replace("<LogFileRollover/>", bRollover ? "<LogFileRollover>" + bRollover.ToString().ToLower() + "</LogFileRollover>" : "")
                    .Replace("<AutoRestart/>", "<AutoRestart>" + bAutoRestart.ToString().ToLower() + "</AutoRestart>")
                    .Replace("<StartTime/>", "")
                    .Replace("<StopTime/>", bUseEnd ? "<StopTime>" + dtEnd.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "</StopTime>" : "")
                    .Replace("<ID/>", "<ID>" + TraceID + "</ID>")
                    .Replace("<Name/>", "<Name>" + TraceID + "</Name>");
                Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": XMLABatch to start trace: \r\n" + XMLABatch);

                string ret = ServerExecute(XMLABatch);

                if (ret.Substring(0, "Success".Length) != "Success")
                    SendMessageToClients("Error starting profiler trace: " + ret + "");
                else
                    SendMessageToClients("Profiler tracing " + (bPerfEvents ? "(including detailed performance relevant events) " : "") + "started to file: " + TraceID + ".trc.");

                if (bGetXMLA || bGetABF)
                {
                    XMLABatch = Properties.Resources.DbsCapturedTraceStartXMLA
                        .Replace("<LogFileName/>", "<LogFileName>" + Environment.CurrentDirectory + "\\" + TraceID + "\\DatabaseNamesOnly_" + TraceID + ".trc</LogFileName>")
                        .Replace("<LogFileSize/>", "")
                        .Replace("<LogFileRollover/>", "")
                        .Replace("<AutoRestart/>", "<AutoRestart>" + bAutoRestart.ToString().ToLower() + "</AutoRestart>")
                        .Replace("<StartTime/>", "")
                        .Replace("<StopTime/>", bUseEnd ? "<StopTime>" + dtEnd.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "</StopTime>" : "")
                        .Replace("<ID/>", "<ID>dbsOnly" + TraceID + "</ID>")
                        .Replace("<Name/>", "<Name>dbsOnly" + TraceID + "</Name>");
                    ret = ServerExecute(XMLABatch);
                }
            }

            if (bGetNetwork)
            {
                BackgroundWorker bg = new BackgroundWorker();
                bg.DoWork += bgGetNetworkWorker;
                bg.RunWorkerCompleted += bgGetNetworkCompletion;
                bg.RunWorkerAsync();
            }
            else
                FinalizeStart();
        }
        private uint InitializePerfLog(string strSaveAs)
        {
            m_PdhHelperInstance.OpenQuery();

            // Add our PerfMon counters
            System.Collections.Specialized.StringCollection s = new System.Collections.Specialized.StringCollection();
            s.Add("\\Process(*)\\*");
            s.Add("\\Processor(*)\\*");
            s.Add("\\Memory\\*");
            s.Add("\\PhysicalDisk(*)\\*");
            s.Add("\\LogicalDisk(*)\\*");
            s.Add("\\Network Interface(*)\\*");

            if (sInstanceName != "Power BI Report Server") // PBIRS doesn't include AS perf counters unfortunately.
            {
                // The SSAS counter path varies depending on instance and version so set it accordingly.
                string PerfMonInstanceID = "";
                if (sInstanceName == "")
                    PerfMonInstanceID = "\\MSAS" + sInstanceVersion.Substring(0, 2);
                else
                    PerfMonInstanceID = "\\MSOLAP$" + sInstanceName;
                // Now add the SSAS counters using correct instance prefix...
                s.Add(PerfMonInstanceID + ":Cache\\*");
                s.Add(PerfMonInstanceID + ":Connection\\*");
                s.Add(PerfMonInstanceID + ":Data Mining Model Processing\\*");
                s.Add(PerfMonInstanceID + ":Data Mining Prediction\\*");
                s.Add(PerfMonInstanceID + ":Locks\\*");
                s.Add(PerfMonInstanceID + ":MDX\\*");
                s.Add(PerfMonInstanceID + ":Memory\\*");
                s.Add(PerfMonInstanceID + ":Proactive Caching\\*");
                s.Add(PerfMonInstanceID + ":Proc Aggregations\\*");
                s.Add(PerfMonInstanceID + ":Proc Indexes\\*");
                s.Add(PerfMonInstanceID + ":Storage Engine Query\\*");
                s.Add(PerfMonInstanceID + ":Threads\\*");
            }

            // Add all the counters now to the query...
            m_PdhHelperInstance.AddCounters(ref s, false);
            uint ret = m_PdhHelperInstance.OpenLogForWriting(
                            strSaveAs,
                            PdhLogFileType.PDH_LOG_TYPE_BINARY,
                            true,
                            0,
                            false,
                            "SSAS Diagnostics Performance Monitor Log");

            return ret;
        }
        Process pNetworkCapture;
        private void bgGetNetworkWorker(object sender, DoWorkEventArgs e)
        {
            SendMessageToClients("Starting network trace.");
            pNetworkCapture = new Process();
            pNetworkCapture.StartInfo.UseShellExecute = false;
            pNetworkCapture.StartInfo.CreateNoWindow = true;
            pNetworkCapture.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            // elevate priv
            pNetworkCapture.StartInfo.Verb = "runas";
            pNetworkCapture.StartInfo.FileName = "nmcap";
            pNetworkCapture.StartInfo.Arguments = "/network * /capture /file \"" + Environment.CurrentDirectory + "\\" + TraceID + "\\" + TraceID + ".chn\":" + (bRollover && iRollover < 500 ? iRollover : 500) + "MB /RecordConfig /CaptureProcesses";
            pNetworkCapture.Start();
            SendMessageToClients("Network tracing started to file: " + TraceID + ".cap.");
        }
        private void bgGetNetworkCompletion(object sender, RunWorkerCompletedEventArgs e)
        {
            FinalizeStart();
        }
        private void FinalizeStart()
        {
            if (bRollover) SendMessageToClients("Log and trace files rollover after " + iRollover + "MB.");
            if (bUseEnd) SendMessageToClients("Diagnostic collection stops automatically at " + dtEnd.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
            m_StartTime = DateTime.Now;
            CompletionCallback();

            bCollectionFullyInitialized = true;
        }
        #endregion StartCapture

        private void SendMessageToClients(string s)
        {
            try
            {
                if (s.Length > 1) s = "\r\n" + s;
                npServer.PushMessage(s);
                if (s.StartsWith("\r\nTime remaining until collection starts: ") || s.StartsWith("\r\nDiagnostics captured for "))
                {
                    string[] lines = new string[0];
                    if (File.Exists(svcOutputPath))
                        File.ReadAllLines(svcOutputPath);
                    if (lines.Length > 0 && lines[lines.Length - 1].StartsWith(s.Substring(2, 14)))
                    {
                        lines[lines.Length - 1] = s.Trim(new char[] { '\r', '\n' });
                        File.WriteAllText(svcOutputPath, String.Join("\r\n", lines).TrimEnd(new char[] { '\r', '\n' }));
                    }
                    else
                        File.AppendAllText(svcOutputPath, s);
                }
                else
                    File.AppendAllText(svcOutputPath, s);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception sending message from service to client(s):\r\n" + ex.Message);
            }
        }

        private void CollectorPumpTick(object sender, EventArgs e)
        {
            if (bScheduledStartPending)
            {
                TimeSpan ts = dtStart - DateTime.Now;
                SendMessageToClients("Time remaining until collection starts: " + ts.ToString("dd\\:hh\\:mm\\:ss"));
                if (ts.TotalSeconds <= 0)
                {
                    SendMessageToClients("Scheduled start time reached at " + dtStart.ToString("MM/dd/yyyy HH:mm:ss UTCzzz")
                        + ".  Starting diagnostic collection now.");
                    StartDiagnostics();
                }
            }
            else
            {
                iCurrentTimerTicksSinceLastInterval++;
                iSecondsSinceLastHangCheck++;

                if (iSecondsSinceLastHangCheck == 30 && bAutomaticHangDumps)
                {
                    Server s = new Server();
                    try
                    {
                        s.Connect("Data source=" + (bCluster ? sInstanceName.Replace(" (Clustered Instance)", "") : Environment.MachineName + (sInstanceName == "" ? "" : (sInstanceName == "Power BI Report Server" ? ":" + sInstanceID : "\\" + sInstanceName))) + ";Timeout=1;Integrated Security=SSPI;SSPI=NTLM;", true);
                        if (!s.Connected)
                        {
                            SendMessageToClients("The server did not respond to a connection attempt.  Attempting automatic hang dump capture.");
                            new Thread(new ThreadStart(() => CaptureHangDumps())).Start();
                        }
                    }
                    catch (Exception)
                    {
                        SendMessageToClients("The server did not respond to a connection attempt.  Attempting automatic hang dump capture.");
                        new Thread(new ThreadStart(() => CaptureHangDumps())).Start();
                    }
                    iSecondsSinceLastHangCheck = 0;
                }

                if (!bSuspendUITicking)
                {
                    if (!bCollectionFullyInitialized)
                    {
                        // output a "... ... ... " pattern as sign of life while starting/stopping stuff
                        int s = DateTime.Now.Second;
                        if (s % 4 != 0)
                            SendMessageToClients(".");
                        else
                            SendMessageToClients(" ");
                    }
                    else
                        SendMessageToClients("Diagnostics captured for " + ((TimeSpan)(DateTime.Now - m_StartTime)).ToString("hh\\:mm\\:ss"));
                }

                if (iCurrentTimerTicksSinceLastInterval >= iInterval && bPerfMonRunning)
                {
                    // If perfmon logging failed we still want to tick our timer so just fail past this with try/catch anything...
                    try { m_PdhHelperInstance.UpdateLog("SSASDiag" + sInstanceName); }
                    catch (Exception ex)
                    {
                        frmSSASDiag.LogException(ex);
                    }
                    iCurrentTimerTicksSinceLastInterval = 0;
                    if (bRollover)
                    {
                        if (new FileInfo(m_PdhHelperInstance.LogName).Length > this.iRollover * 1024 * 1024)
                        {
                            string sCurFile = m_PdhHelperInstance.LogName.Substring(m_PdhHelperInstance.LogName.LastIndexOf(TraceID) + TraceID.Length).Replace(".blg", "");
                            int iCurFile = sCurFile == "" ? 0 : Convert.ToInt32(sCurFile);
                            m_PdhHelperInstance.Dispose();
                            InitializePerfLog(TraceID + "\\" + TraceID.Replace(".blg", "") + (iCurFile + 1) + ".blg");
                        }
                    }
                }

                if (DateTime.Now > dtEnd && bUseEnd)
                    StopAndFinalizeAllDiagnostics();
            }
        }

        #region EndCapture
        public void StopAndFinalizeAllDiagnostics()
        {
            bCollectionFullyInitialized = false;
            
            if (bRunning)
            {
                bRunning = false;
                SendMessageToClients("Stopping collection at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                //if (sRecurrencePattern != "" && !bForceStop)
                //    bSuspendUITicking = true;
                if (bGetPerfMon && !bScheduledStartPending)
                {
                    bPerfMonRunning = false;
                    m_PdhHelperInstance.Dispose();
                    SendMessageToClients("Stopped performance monitor logging.");
                }

                if (bGetConfigDetails && !bScheduledStartPending)
                {
                    // Grab event logs post repro
                    EvtExportLog(IntPtr.Zero, "Application", "*", Environment.CurrentDirectory + "\\" + TraceID + "\\" + TraceID + "_Application.evtx", EventExportLogFlags.ChannelPath);
                    EvtExportLog(IntPtr.Zero, "System", "*", Environment.CurrentDirectory + "\\" + TraceID + "\\" + TraceID + "_System.evtx", EventExportLogFlags.ChannelPath);
                    SendMessageToClients("Collected Application and System event logs.");
                }

                if (bGetNetwork && !bScheduledStartPending)
                {
                    BackgroundWorker bgStopNetwork = new BackgroundWorker();
                    bgStopNetwork.DoWork += bgStopNewtworkWorker;
                    bgStopNetwork.RunWorkerCompleted += bgStopNetworkComplete;
                    bgStopNetwork.RunWorkerAsync();
                }
                else
                {
                    // Complete profiler trace and zip the data as last background worker process, 
                    // from here if we skipped network traces,
                    // otherwise, from network trace completion below.
                    BackgroundWorker bgZipData = new BackgroundWorker();
                    bgZipData.DoWork += bgFinalProfilerAndZipSteps;
                    bgZipData.RunWorkerCompleted += bgFinalProfilerAndZipSteps_Completion;
                    bgZipData.RunWorkerAsync();
                }
            }
        }

        public T GetPrivateField<T>(object obj, string name)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = obj.GetType();
            FieldInfo field = type.GetField(name, flags);
            return (T)field.GetValue(obj);
        }
        private void GetBAK(string db, Server s)
        {
            foreach (DataSource ds in s.Databases.FindByName(db).DataSources)
            {
                string cs = ds.ConnectionString.TrimEnd(';') + ";";
                string srvName = "";
                string sqlDbName = "";
                if (cs.Contains("Data Source="))
                    srvName = cs.Substring(cs.IndexOf("Data Source=") + "Data Source=".Length) + ";";
                if (cs.Contains("Server="))
                    srvName = cs.Substring(cs.IndexOf("Server=") + "Server=".Length) + ";";
                if (cs.Contains("Initial Catalog="))
                    sqlDbName = cs.Substring(cs.IndexOf("Initial Catalog=") + "Initial Catalog=".Length) + ";";
                if (cs.Contains("Database="))
                    sqlDbName = cs.Substring(cs.IndexOf("Database=") + "Database=".Length) + ";";
                srvName = srvName.Substring(0, srvName.IndexOf(";"));
                sqlDbName = sqlDbName.Substring(0, sqlDbName.IndexOf(";"));
                if (cs.Contains("User ID"))
                {
                    cs = cs.Remove(cs.IndexOf("User ID="), cs.IndexOf(";", cs.IndexOf("User ID=")) - cs.IndexOf("User ID=") + 1);
                }
                if (cs.Contains("Password"))
                cs = cs.Remove(cs.IndexOf("Password="), cs.IndexOf(";", cs.IndexOf("Password=")));
                if (!cs.ToLower().Contains("trusted_connection=yes"))
                    cs += "Trusted_Connection=Yes;";
                if (!cs.ToLower().Contains("persist security info=false"))
                    cs += "Persist Security Info=false;";
                cs = cs.ToLower().Replace("integrated security=sspi;", "");

                OleDbConnection conn = new OleDbConnection(cs);
                bool bAuthenticated = false;
                SendMessageToClients("Starting SQL datasource backup for AS database " + db + ", data source name " + ds.Name + ", SQL database " + sqlDbName + " on server " + (srvName == "." ? Environment.MachineName : srvName) + "...");

                foreach (NamedPipeConnection<string, string> npConn in npServer._connections)
                {
                    if (npConn.IsConnected)
                    {
                        try
                        {
                            ImpersonateNamedPipeConnection(npConn);
                            SendMessageToClients("Attempting to connect to data source as client " + Environment.UserDomainName + "\\" + Environment.UserName);
                            conn.Open();
                            PerformBAKBackupAndMoveLocal(conn, srvName, ds.Name, db, sqlDbName);
                            bAuthenticated = true;
                        }
                        catch (Exception e)
                        {
                            SendMessageToClients(e.Message);
                        }
                    }
                }
                if (!bAuthenticated)
                {
                    bSuspendUITicking = true;
                    // If it fails the first try, prompt for remote admin
                    int iTries = 0;
                    while (!bAuthenticated && iTries < 3)
                    {
                        try
                        { 
                            SendMessageToClients("Waiting for client interaction:\r\n"
                                + "Windows Administrator required for remote server: " + srvName
                                + "\r\nFor data source name: " + ds.Name
                                + "\r\nIn AS database: " + db
                                + ((iTries > 0) ? "TryingAgain" : ""));
                            sRemoteAdminUser = "";
                            clientWaiter.WaitOne();
                            if (sRemoteAdminUser != "Cancelled by client")
                            {
                                SafeTokenHandle safeTokenHandle = null;
                                WindowsImpersonationContext impersonatedUser = null;
                                if (sRemoteAdminUser == "")
                                {
                                    // This happens if a new client connected rather than anybody entering credentials directly.
                                    SendMessageToClients("Attempting to connect to data source as client " + Environment.UserDomainName + "\\" + Environment.UserName + ".");
                                }
                                else
                                {
                                    SendMessageToClients("Attempting to authenticate user " + sRemoteAdminDomain + "\\" + sRemoteAdminUser + " on remote server " + srvName + ".");
                                    // Impersonate user remotely

                                    const int LOGON32_PROVIDER_DEFAULT = 0;
                                    const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
                                    bAuthenticated = LogonUser(sRemoteAdminUser, sRemoteAdminDomain, SecureStringToString(sRemoteAdminPassword), LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);
                                    if (!bAuthenticated)
                                    {
                                        iTries++;
                                        SendMessageToClients("Windows authentication for remote SQL data source " + ds.Name + " on server " + srvName + " failed.");
                                        break;
                                    }
                                    if (safeTokenHandle != null)
                                        impersonatedUser = WindowsIdentity.Impersonate(safeTokenHandle.DangerousGetHandle());
                                }
                                try
                                {
                                    conn.Open();
                                    bAuthenticated = true;
                                    bSuspendUITicking = false;
                                    if (PerformBAKBackupAndMoveLocal(conn, srvName, ds.Name, db, sqlDbName, impersonatedUser, sRemoteAdminDomain, sRemoteAdminUser))
                                        break;
                                    else
                                    {
                                        bAuthenticated = false;
                                        if (sRemoteAdminUser != "") iTries++;
                                    }
                                }
                                catch (Exception ex2)
                                {
                                    bAuthenticated = false;
                                    if (sRemoteAdminUser != "") iTries++;
                                    frmSSASDiag.LogException(ex2);
                                    SendMessageToClients(ex2.Message);
                                }
                            }
                            else
                                break;
                        }
                        catch (Exception)
                        { SendMessageToClients("Failure backing up dateabase as connected client user " + Environment.UserDomainName + "\\" + Environment.UserName + "."); }
                    }
                }
                if (!bAuthenticated)
                {
                    SendMessageToClients("Windows authentication for remote SQL data source " + ds.Name + " on server " + srvName + " failed.");
                }
            }
            bSuspendUITicking = false;
        }
        private bool PerformBAKBackupAndMoveLocal(OleDbConnection conn, string srvName, string dsName, string ASdbName, string SQLDBName, WindowsImpersonationContext impersonatedUser = null, string Domain = "", string User = "")
        {
            string BackupDir = "";
            try
            {
                OleDbCommand cmd = new OleDbCommand(@"EXEC  master.dbo.xp_instance_regread  N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'BackupDirectory'", conn);
                OleDbDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    BackupDir = rdr["Data"] as string;
                string domainName = "." + IPGlobalProperties.GetIPGlobalProperties().DomainName;
                string fqhostName = Dns.GetHostName();
                if (!fqhostName.EndsWith(domainName))  // if hostname does not already include domain name
                    fqhostName += domainName;   // add the domain name part
                if (srvName != "." && srvName.ToLower() != "localhost" && srvName.ToLower() != Environment.MachineName.ToLower() && srvName.ToLower() != fqhostName)
                {
                    SendMessageToClients("Confirming remote file share access to copy remote database backup locally.");
                    if (!Directory.Exists("\\\\" + srvName + "\\" + BackupDir.Replace(":", "$")))
                    {
                        SendMessageToClients("Remote file share access failed for user " + (sRemoteAdminDomain == "" ? Environment.UserDomainName : sRemoteAdminDomain) + "\\" + (sRemoteAdminUser == "" ? Environment.UserName : sRemoteAdminUser) + " to server " + srvName + ".");
                        return false;
                    }
                }
                    
                SendMessageToClients("Initiating backup of relational database " + SQLDBName + ".bak on SQL server " + srvName + ".");
                cmd = new OleDbCommand(@"BACKUP DATABASE [" + SQLDBName + "] TO  DISK = N'" + BackupDir + "\\SSASDiag_" + SQLDBName + ".bak' WITH FORMAT, INIT, NAME = N'SSASDag_" + SQLDBName + "-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, COMPRESSION, STATS = 10", conn);
                int ret = cmd.ExecuteNonQuery();
                SendMessageToClients("Database backup completed.");
                SendMessageToClients("Moving SQL backup to local capture directory...");
                try
                {
                    CopyBAKLocal(srvName, BackupDir, SQLDBName);
                }
                catch(Exception ex)
                {
                    frmSSASDiag.LogException(ex);
                    if (ex.Message.Contains("Could not find file") && (Domain == srvName || Domain == srvName.Substring(0, srvName.IndexOf(".")) || Domain == "."))
                    {
                        try
                        {
                            cmd.CommandText = @"EXEC xp_regwrite @rootkey='HKEY_LOCAL_MACHINE', @key='Software\Microsoft\Windows\CurrentVersion\Policies\System', @value_name='LocalAccountTokenFilterPolicy', @type='REG_DWORD', @value=1";
                            if (cmd.ExecuteNonQuery() != 0)
                            {
                                SendMessageToClients("Failure collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ".");
                                SendMessageToClients("For local administrator accounts except Administrator to access the remote SQL backups, create a DWORD32 value LocalAccountTokenFilterPolicy=1 in HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\Policies\\System on the server " + srvName + ".");
                            }
                            else
                            {
                                try
                                {
                                    CopyBAKLocal(srvName, BackupDir, SQLDBName);
                                }
                                catch (Exception ex2)
                                {
                                    SendMessageToClients("Failure type 1 collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ":\r\n" + ex2.Message);
                                }
                            }
                        }
                        catch(Exception)
                        {
                            frmSSASDiag.LogException(ex);
                            SendMessageToClients("Failure type 2 collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ":\r\n" + ex.Message);
                            SendMessageToClients("For local administrator accounts except Administrator to access the remote SQL backups, create a DWORD32 value LocalAccountTokenFilterPolicy=1 in HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\Policies\\System on the server " + srvName + ".");
                        }
                    }
                    else
                        SendMessageToClients("Failure type 3 collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ":\r\n" + ex.Message + "");
                    SendMessageToClients("Please collect .bak manually from " + BackupDir + " on server " + srvName + ".");
                    return false;
                }
                SendMessageToClients("Collected SQL data source .bak backup for data source " + dsName + " in database " + ASdbName + ".");
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Remote file share access failed for user "))
                    throw ex;
                frmSSASDiag.LogException(ex);
                SendMessageToClients("Failure type 4 collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ":\r\n" + ex.Message + "");
            }
            return true;
        }
        void CopyBAKLocal(string srvName, string BackupDir, string SQLDBName)
        {
            if (srvName != "." && srvName.ToUpper() != Environment.MachineName.ToUpper() && srvName.ToUpper() != GetFQDN())
                File.Move("\\\\" + srvName + "\\" + BackupDir.Replace(":", "$") + "\\SSASDiag_" + SQLDBName + ".bak", Environment.CurrentDirectory + "\\" + TraceID + "\\Databases\\" + SQLDBName + ".bak");
            else
                File.Move(BackupDir + "\\SSASDiag_" + SQLDBName + ".bak", Environment.CurrentDirectory + "\\" + TraceID + "\\Databases\\" + SQLDBName + ".bak");
        }
        private void bgStopNewtworkWorker(object sender, DoWorkEventArgs e)
        {
            SendMessageToClients("Stopping network trace.");
            StopProgram(pNetworkCapture);
            pNetworkCapture.WaitForExit();
            pNetworkCapture.Close();
            pNetworkCapture = null;
            SendMessageToClients("Network trace stopped and collected.");
        }
        #region Send Ctrl-C to console app
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        // Enumerated type for the control messages sent to the handler routine
        enum CtrlTypes : uint
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint dwProcessGroupId);

        public void StopProgram(Process proc)
        {
            //This does not require the console window to be visible.
            if (AttachConsole((uint)proc.Id))
            {
                // Disable Ctrl-C handling for our program
                SetConsoleCtrlHandler(null, true);
                GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);

                // Must wait here. If we don't and re-enable Ctrl-C
                // handling below too fast, we might terminate ourselves.
                proc.WaitForExit(2000);

                FreeConsole();

                //Re-enable Ctrl-C handling or any subsequently started
                //programs will inherit the disabled state.
                SetConsoleCtrlHandler(null, false);
            }
        }

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);

        // Delegate type to be used as the Handler Routine for SCCH
        delegate Boolean ConsoleCtrlDelegate(CtrlTypes CtrlType);
        #endregion 
        private void bgStopNetworkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker bgFinalSteps = new BackgroundWorker();
            bgFinalSteps.DoWork += bgFinalProfilerAndZipSteps;
            bgFinalSteps.RunWorkerCompleted += bgFinalProfilerAndZipSteps_Completion;
            bgFinalSteps.RunWorkerAsync();
        }

        private void bgFinalProfilerAndZipSteps(object sender, DoWorkEventArgs e)
        {
            if (bGetProfiler && !bScheduledStartPending)
            {
                if (Environment.GetCommandLineArgs().Where(a=>a.Contains("nowaitonstop")).Count() == 0)
                {
                    SendMessageToClients("Waiting 15s to allow profiler trace to catch up with any lagging events...");
                    System.Threading.Thread.Sleep(15000); // Wait 15s to allow profiler events to catch up a little bit.
                }
                SendMessageToClients("Executing AS server command to stop profiler trace...");
                ServerExecute(Properties.Resources.ProfilerTraceStopXMLA.Replace("<TraceID/>", "<TraceID>" + TraceID + "</TraceID>"));
                SendMessageToClients("Stopped profiler trace.");

                if (bGetXMLA || bGetABF || bGetBAK)
                {
                    List<string> dbs = new List<string>();
                    SendMessageToClients("Finding databases with queries/commands started/completed during tracing...");
                    ServerExecute(Properties.Resources.ProfilerTraceStopXMLA.Replace("<TraceID/>", "<TraceID>dbsOnly" + TraceID + "</TraceID>"));
                    string[] dbNamesTrace = Directory.GetFiles(Environment.CurrentDirectory as string, TraceID + "\\DatabaseNamesOnly_" + TraceID + "*.trc");
                    if (dbNamesTrace.Length > 0)
                        dbs = ExtractDBNamesFromDBNamesTrace(dbNamesTrace[0]);
                    if (Directory.GetFiles(Environment.CurrentDirectory as string, TraceID + "\\DatabaseNamesOnly_" + TraceID + "*.trc").Length > 0)
                        File.Delete(dbNamesTrace[0]);

                    if (dbs.Count == 0)
                        SendMessageToClients("There were no databases captured in the profiler trace.  No AS database definitions or backups will be captured.");
                    else
                    {
                        SendMessageToClients("Captured " + dbs.Count + " database" + (dbs.Count > 1 ? "s" : "") + " in the trace.");

                        if (!Directory.Exists(Environment.CurrentDirectory + "\\" + TraceID + "\\Databases"))
                            Directory.CreateDirectory(Environment.CurrentDirectory + "\\" + TraceID + "\\Databases");

                        foreach (string db in dbs)
                        {
                            Microsoft.AnalysisServices.Server s = new Microsoft.AnalysisServices.Server();
                            // Previously we connected before iterating through each db to be processed, but found if some misbehaving Cx databases were running, this leads to connection hangs!
                            // Now we connect directly to each database we need to capture, so other issues on the server may not impact then with locking conflicts to enumerate metadata on dbs not under consideration.
                            string sConn = "Data source=" + (bCluster ? sInstanceName.Replace(" (Clustered Instance)", "") : Environment.MachineName + (sInstanceName == "" ? "" : (sInstanceName == "Power BI Report Server" ? ":" + sInstanceID : "\\" + sInstanceName))) + ";Timeout=0;Integrated Security=SSPI;SSPI=NTLM;Initial Catalog=" + db + ";";
                            try { s.Connect(sConn); }
                            catch (Exception ex)
                            {
                                SendMessageToClients("Exception connecting to SSAS database: " + ex.Message + "\r\nConnection string used was: " + sConn);
                            }

                            if (bGetXMLA)
                            {
                                
                                Database DB = s.Databases.FindByName(db);
                                if (s.ServerMode == ServerMode.Multidimensional || DB.CompatibilityLevel < 1200)
                                {
                                    SendMessageToClients("Extracting database definition XMLA script for " + db + ".");
                                    MajorObject[] mo = { DB };
                                    XmlWriter output = XmlWriter.Create(Environment.CurrentDirectory + "\\" + TraceID + "\\Databases\\" + db + ".xmla", new XmlWriterSettings() { OmitXmlDeclaration = true });
                                    Microsoft.AnalysisServices.Scripter sc = new Microsoft.AnalysisServices.Scripter();
                                    sc.ScriptCreate(mo, output, true);
                                    output.Flush();
                                    output.Close();
                                }
                                else
                                {
                                    SendMessageToClients("Extracting database definition JSON script for " + db + ".");
                                    StreamWriter sw = File.CreateText(Environment.CurrentDirectory + "\\" + TraceID + "\\Databases\\" + db + ".json");
                                    sw.WriteLine(Microsoft.AnalysisServices.Tabular.JsonScripter.ScriptCreate(DB));
                                    sw.Close();
                                }
                                if (bGetBAK)
                                    GetBAK(db, s);
                            }
                            if (bGetABF)
                            {
                                SendMessageToClients("Backing up AS database .abf for " + db + ".");
                                string batch = Properties.Resources.BackupDbXMLA
                                    .Replace("<DatabaseID/>", "<DatabaseID>" + s.Databases.FindByName(db).ID + "</DatabaseID>")
                                    .Replace("<File/>", "<File>" + Environment.CurrentDirectory + "\\" + TraceID + "\\Databases\\" + db + ".abf</File>")
                                    .Replace("<AllowOverwrite/>", "<AllowOverwrite>true</AllowOverwrite>");
                                string ret = ServerExecute(batch);
                                if (ret != "Success!")
                                    SendMessageToClients("Error backing up AS database for " + db + ":\n\t" + ret + "");
                            }
                            if (s.Connected)
                                s.Disconnect();
                        }
                    }
                }
            }
            Debug.WriteLine("Stoppped SSAS diagnostics collection at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");

            // Just before zip, write out last line of this capture log and save that file...
            // The last line captured in text file here:
            SendMessageToClients("Stoppped SSAS diagnostics collection at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");

            if (!bScheduledStartPending)
            {
                List<string> log = File.ReadAllLines(svcOutputPath).ToList();
                string captureduration = log.Where(l => l.StartsWith("Diagnostics captured for ")).Last();
                log = log.Where(l => !l.StartsWith("Diagnostics captured for ") && !String.IsNullOrWhiteSpace(l)).ToList();
                log.Add(captureduration);
                File.WriteAllLines(TraceID + "\\SSASDiag.log", log);
            }

            if (bCompress && !bScheduledStartPending)
            {
                SendMessageToClients("Creating zip file of output: " + Environment.CurrentDirectory + "\\" + TraceID + ".zip.");

                try
                {
                    // Zip up all output into a single zip file.
                    ZipFile z = new ZipFile();
                    z.ParallelDeflateThreshold = -1;
                    z.UseZip64WhenSaving = Ionic.Zip.Zip64Option.Always;
                    z.AddDirectory(TraceID);
                    z.MaxOutputSegmentSize = 1024 * 1024 * (int)iRollover;
                    z.Encryption = EncryptionAlgorithm.None;
                    z.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                    z.BufferSize = 1000000;
                    z.CodecBufferSize = 1000000;
                    z.Save(TraceID + ".zip");

                    SendMessageToClients("Created zip file.");
                }
                catch (Exception ex)
                { frmSSASDiag.LogException(ex); }
            }

            if (bDeleteRaw && !bScheduledStartPending)
            {
                try
                {
                    Directory.Delete(TraceID, true);
                    SendMessageToClients("Deleted capture output folder.");
                }
                catch
                {
                    SendMessageToClients("Failed to delete output folder:\n"
                        + "\tThis could be due to locked files in the folder and suggests possible failure stopping a trace.\n"
                        + "\tPlease review the contents of the folder " + TraceID + "\n."
                        + "\tIt was created in the same location where you ran this utility.");
                } 
            }
            Debug.WriteLine("Finalized collection.");
        }

        private List<string> ExtractDBNamesFromDBNamesTrace(string DBNamesTrace)
        {
            byte[] b = File.ReadAllBytes(DBNamesTrace);
            List<string> s = new List<string>();

            // HACK ALERT...

            // The following byte pattern is observed in SQL Profiler traces, when cell text data is about to be presented:
            // [0][x+10][0][0][0][28][0][x]
            // x varies across each release but the pattern itself persists, preceding text content.
            // In the very limited context of a single column trace, this can be used to extract the text.
            // The trailing bit pattern [41][0][4] was found consistently on all versions tested.
            // Using this extreme hack can avoid an otherwise unnecessary dependency on the trace reader libraries for data collection in SSASDiag.
            // Otherwise, unfortunately, the read of profiler traces requires full install of management studio, developer confirms, for the corresponding edition.
            // One workaround might be to embed numerous copies of the trace reader components, adding several hundred KB minimum to the size of the total .exe payload,
            // and also involving other possible complications...  For now this hack is a very good way to achieve the simple result required.
            // We scan for the byte pattern above in the data and when found, read out the string trailed by the known trailing text.

            for (int i = 7; i < b.Length; i++)
            {
                // Search for byte pattern [0][x+10][0][0][0][28][0][x]
                if (b[i - 7] == 0 && (b[i - 6] == b[i] + 10) && b[i - 5] == 0 && b[i - 4] == 0 && b[i - 3] == 0 && b[i - 2] == 28 && b[i - 1] == 0)
                {
                    i++;
                    String sdb = "";
                    // Terminate on bit pattern [41][0][4]
                    while (i < b.Length - 3 && !(b[i] == 41 && b[i + 1] == 0 && b[i + 2] == 4))
                    {
                        sdb += BitConverter.ToChar(b, i);
                        i += 2;
                    }

                    if (sdb.TrimEnd().TrimStart() != "" && !s.Contains(sdb))
                        s.Add(sdb);
                }
            }
            return s;
        }

        private void bgFinalProfilerAndZipSteps_Completion(object sender, RunWorkerCompletedEventArgs e)
        {
            FinalizeStop();
        }
        private void FinalizeStop()
        {
            SendMessageToClients("SSASDiag completed at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
            Debug.WriteLine("SSASDiag collection completed at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
            if (sRecurrencePattern != "" && !bForceStop)
                bSuspendUITicking = false;
            if (sRecurrencePattern == "" || bForceStop)
            {
                PerfMonAndUIPumpTimer.Stop();
                bScheduledStartPending = false;
                SendMessageToClients("Stop");
                CompletionCallback();
            }
            else
            {
                DayOfWeek d = DateTime.Today.DayOfWeek == DayOfWeek.Saturday ? DayOfWeek.Sunday : DateTime.Today.DayOfWeek + 1;
                while (!sRecurrencePattern.ToLower().Contains(Program.MainForm.DayLettersFromDay(d)))
                {
                    if (d == DayOfWeek.Saturday)
                        d = DayOfWeek.Sunday;
                    else
                        d++;
                }

                int iDaysUntilNextStart = d - DateTime.Today.DayOfWeek;
                if (iDaysUntilNextStart < 1) iDaysUntilNextStart += 7;

                dtEnd = dtEnd.AddDays(iDaysUntilNextStart > 0 ? iDaysUntilNextStart : 7);
                dtStart = dtStart.AddDays(iDaysUntilNextStart > 0 ? iDaysUntilNextStart : 7);
                SendMessageToClients("Increment schedule by days: " + iDaysUntilNextStart);
                bScheduledStartPending = true;
                StartDiagnostics();
            }
        }
        #endregion EndCapture

        #region UtilityFunctions
        public static string GetFQDN()
        {
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();

            domainName = "." + domainName;
            if (!hostName.EndsWith(domainName))  // if hostname does not already include domain name
            {
                hostName += domainName;   // add the domain name part
            }

            return hostName;                    // return the fully qualified name
        }
        private string ServerExecute(string command)
        {
            string ret = "Success!";
            try
            {
                Microsoft.AnalysisServices.Server s = new Microsoft.AnalysisServices.Server();
                s.Connect("Data source=" + (bCluster ? sInstanceName.Replace(" (Clustered Instance)", "") : Environment.MachineName + (sInstanceName == "" ? "" : (sInstanceName == "Power BI Report Server" ? ":" + sInstanceID : "\\" + sInstanceName))) + ";Timeout=0;Integrated Security=SSPI;SSPI=NTLM;", true);
                try
                {
                    XmlaResultCollection results = s.Execute(command);

                    foreach (XmlaResult result in results)
                        foreach (XmlaMessage message in result.Messages)
                            if (message is XmlaError)
                                return "Error: " + message.Description;
                            else if (message is XmlaWarning)
                                return "Warning: " + message.Description;
                            else ret = "Success: " + message.Description;
                }
                catch (Exception ex)
                {
                    frmSSASDiag.LogException(ex);
                }
                finally { s.Disconnect(); }
            }
            catch (Exception ex)
            {
                frmSSASDiag.LogException(ex);
                return "Error: " + ex.Message;
            }
            return ret;
        }

        #endregion UtilityFunctions
    }

    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }
}
