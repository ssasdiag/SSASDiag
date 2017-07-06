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

namespace SSASDiag
{
    public class CDiagnosticsCollector : INotifyPropertyChanged
    {
        #region publics
        public bool bScheduledStartPending = false, bRunning = false, bPerfMonRunning = false;
        public System.Action CompletionCallback;
        public NamedPipeServer<string> npServer;
        public string TraceID;
        #endregion publics

        #region toomanylocals
        
        PdhHelper m_PdhHelperInstance = new PdhHelper(false);
        System.Timers.Timer PerfMonAndUIPumpTimer = new System.Timers.Timer();
        RichTextBox txtStatus;
        DateTime m_StartTime = DateTime.Now;
        string sTracePrefix = "", sInstanceName, sInstanceVersion, sInstanceMode, sInstanceEdition, sLogDir, sConfigDir, sServiceAccount, sRemoteAdminUser, sRemoteAdminDomain;
        SecureString sRemoteAdminPassword;
        int iInterval = 0, iRollover = 0, iCurrentTimerTicksSinceLastInterval = 0;
        bool bAutoRestart = false, bRollover = false, bUseStart, bUseEnd, bGetConfigDetails, bGetProfiler, bGetXMLA, bGetABF, bGetBAK, bGetPerfMon, bGetNetwork, bCompress = true, bDeleteRaw = true, bPerfEvents = true;
        bool bCollectionFullyInitialized = false, bSuspendUITicking = false;
        DateTime dtStart, dtEnd;
        string svcOutputPath = "";
        Dictionary<int, string> clients = new Dictionary<int, string>();
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
                string TraceFilesPrefix, string InstanceName, string InstanceVersion, string InstanceMode, string InstanceEdition, string ConfigDir, string LogDir, string ServiceAccount,
                RichTextBox StatusTextBox,
                int Interval, 
                bool AutoRestart, bool Compress, bool DeleteRaw, bool IncludePerfEventsInProfiler, bool IncludeXMLA, bool IncludeABF, bool IncludeBAK,
                int RolloverMaxMB, bool Rollover, 
                DateTime Start, bool UseStart, 
                DateTime End, bool UseEnd, 
                bool GetConfigDetails, bool GetProfiler, bool GetPerfMon, bool GetNetwork)
        {
            PerfMonAndUIPumpTimer.Interval = 1000;
            PerfMonAndUIPumpTimer.Elapsed += CollectorPumpTick;
            sTracePrefix = TraceFilesPrefix; sInstanceName = InstanceName; sInstanceVersion = InstanceVersion; sInstanceMode = InstanceMode; sInstanceEdition = InstanceEdition; sConfigDir = ConfigDir; sLogDir = LogDir; sServiceAccount = ServiceAccount;
            txtStatus = StatusTextBox;
            bGetXMLA = IncludeXMLA; bGetABF = IncludeABF; bGetBAK = IncludeBAK;
            iInterval = Interval;
            bAutoRestart = AutoRestart; bCompress = Compress; bDeleteRaw = DeleteRaw; bPerfEvents = IncludePerfEventsInProfiler;
            iRollover = RolloverMaxMB; bRollover = Rollover;
            dtStart = Start; bUseStart = UseStart;
            dtEnd = End; bUseEnd = UseEnd;
            bGetConfigDetails = GetConfigDetails; bGetProfiler = GetProfiler; bGetNetwork = GetNetwork; bGetPerfMon = GetPerfMon;
            svcOutputPath = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\services\\SSASDiag_" + (InstanceName == "" ? "MSSQLSERVER" : InstanceName)).GetValue("ImagePath") as string;
            svcOutputPath = svcOutputPath.Substring(0, svcOutputPath.IndexOf(".exe")) + ".output.log";
            PipeSecurity ps = new PipeSecurity();
            ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null) as IdentityReference, PipeAccessRights.FullControl, AccessControlType.Allow));
            npServer = new NamedPipeServer<string>("SSASDiag_" + (InstanceName == "" ? "MSSQLSERVER" : InstanceName), ps);
            npServer.ClientMessage += npServer_ClientMessage;
            npServer.ClientConnected += NpServer_ClientConnected;
            npServer.Start();
        }

        private void NpServer_ClientConnected(NamedPipeConnection<string, string> connection)
        {
            connection.PushMessage("Request User ID");
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

        private void npServer_ClientMessage(NamedPipeConnection<string, string> connection, string message)
        {
            if (message.StartsWith("User="))
            {
                if (!clients.ContainsKey(connection.Id))
                {
                    clients.Add(connection.Id, message.Substring("User=".Length));
                    Debug.WriteLine("Client user " + clients[connection.Id] + " connected.");
                    clientWaiter.Set();
                }
                else
                    clients[connection.Id] = message.Substring("User=".Length);
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
                sRemoteAdminUser = "Cancelled by client";
                sRemoteAdminDomain = "";
                sRemoteAdminPassword = GetSecureString("");
                clientWaiter.Set();
            }
            if (message == "Stop")
            {
                StopAndFinalizeAllDiagnostics();
            }
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
                SendMessageToClients("Scheduled Diagnostic collection starts automatically at " + dtStart.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                TimeSpan ts = dtStart - DateTime.Now;
                SendMessageToClients("Time remaining until collection starts: " + ts.ToString("hh\\:mm\\:ss"));
                bScheduledStartPending = true;
                return;
            }
            else
            {
                TraceID = sTracePrefix
                    + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd_HH-mm-ss") + "_UTC"
                    + "_SSASDiag";
                Debug.WriteLine("Starting TraceID: " + TraceID);

                SendMessageToClients("Collecting on computer " + Environment.MachineName + ".");
                if (sInstanceVersion != "")  // This occurs when we aren't really capturing instance details with Network only capture.
                {
                    SendMessageToClients("Collecting for instance " + (sInstanceName == "" ? "Default instance (MSSQLServer)" : sInstanceName) + ".");
                    SendMessageToClients("The version of the instance is " + sInstanceVersion + ".");
                    SendMessageToClients("The edition of the instance is " + sInstanceEdition + ".");
                    SendMessageToClients("The instance mode is " + sInstanceMode + ".");
                    SendMessageToClients("The OLAP\\LOG folder for the instance is " + sLogDir + ".");
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

                if (bGetConfigDetails)
                {
                    // Collect SSAS LOG dir, config and save log of this diagnostic capture
                    if (!Directory.Exists(TraceID + "\\Log")) Directory.CreateDirectory(TraceID + "\\Log");
                    foreach (string f in Directory.GetFiles(sLogDir))
                        File.Copy(f, TraceID + "\\Log\\" + f.Substring(f.LastIndexOf("\\") + 1));
                    File.Copy(sConfigDir + "\\msmdsrv.ini", TraceID + "\\msmdsrv.ini");
                    SendMessageToClients("Captured OLAP\\Log contents and msmdsrv.ini config for the instance.");

                    BackgroundWorker bg = new BackgroundWorker();
                    bg.DoWork += bgGetSPNs;
                    bg.RunWorkerCompleted += bgGetSPNsCompleted;
                    bg.RunWorkerAsync();
                }
                else
                    bgGetSPNsCompleted(null, null);
            }                   
        }

        bool ImpersonateNamedPipeConnection(NamedPipeConnection<string, string> conn)
        {
            NamedPipeWrapper.IO.PipeStreamWrapper<string, string> pipe = GetPrivateField<NamedPipeWrapper.IO.PipeStreamWrapper<string, string>>(conn, "_streamWrapper");
            int h = pipe.BaseStream.SafePipeHandle.DangerousGetHandle().ToInt32();
            RevertToSelf();
            if (ImpersonateNamedPipeClient(h) != 0)
                return true;
            else
                return false;
        }

        private void bgGetSPNs(object sender, DoWorkEventArgs e)
        {
            SendMessageToClients("Attempting to capture SPNs for the AS service account " + sServiceAccount + ".");
            string sErr = "";
            foreach (NamedPipeConnection<string, string> conn in npServer._connections)
            {
                ImpersonateNamedPipeConnection(npServer._connections[0]);
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.FileName = "setspn";
                p.StartInfo.Arguments = "-l \"" + sServiceAccount + "\"";
                p.Start();
                string sOut = p.StandardOutput.ReadToEnd();
                sErr = p.StandardError.ReadToEnd();
                p.WaitForExit();
                if (sErr == "")
                {
                    File.WriteAllText(Environment.CurrentDirectory + "\\" + TraceID + "\\ServiceAccountSPNs.txt", sOut);
                    SendMessageToClients("Captured SPNs defined for service account " + sServiceAccount + ".");
                    break;
                }
                else
                {
                    SendMessageToClients("Failed to capture SPNs impersonating connected client " + clients[conn.Id] + ".");
                }
            }
            if (sErr != "")
                SendMessageToClients("Rerun SSASDiag as a domain administrator if this configuration detail is required.");
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
            s.Add("\\Memory\\*");
            s.Add("\\PhysicalDisk(*)\\*");
            s.Add("\\LogicalDisk(*)\\*");

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

            // Add all the counters now to the query...
            m_PdhHelperInstance.AddCounters(ref s, false);
            uint ret = m_PdhHelperInstance.OpenLogForWriting(
                            strSaveAs,
                            PdhLogFileType.PDH_LOG_TYPE_BINARY,
                            true,
                            bRollover ? (uint)iRollover : 0,
                            bRollover ? true : false,
                            "SSAS Diagnostics Performance Monitor Log");

            return ret;
        }
        private void bgGetNetworkWorker(object sender, DoWorkEventArgs e)
        {
            SendMessageToClients("Starting network trace.");
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "netsh";
            p.StartInfo.Arguments = "trace stop";
            p.Start();
            p.WaitForExit(500);
            string sOut = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.StartInfo.Arguments = "trace start fileMode=" + (bRollover ? "circular" : "single") + " capture=yes tracefile=\"" + Environment.CurrentDirectory + "\\" + TraceID + "\\" + TraceID + ".etl\" maxSize=" + (bRollover ? iRollover.ToString() : "0");
            p.Start();
            sOut = p.StandardOutput.ReadToEnd();
            System.Diagnostics.Trace.WriteLine("netsh trace start's output: " + sOut);
            p.WaitForExit();
            SendMessageToClients("Network tracing started to file: " + TraceID + ".etl.");
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
            if (s.Length > 1) s = "\r\n" + s;
            npServer.PushMessage(s);
            if (s.StartsWith("\r\nTime remaining until collection starts: ") || s.StartsWith("\r\nDiagnostics captured for "))
            {
                string[] lines = File.ReadAllLines(svcOutputPath);
                if (lines[lines.Length - 1].StartsWith(s.Substring(2, 14)))
                {
                    lines[lines.Length - 1] = s.Trim(new char[] { '\r', '\n' }) ;
                    File.WriteAllText(svcOutputPath, String.Join("\r\n", lines).TrimEnd(new char[] { '\r', '\n' }));
                }
                else
                    File.AppendAllText(svcOutputPath, s);
            }
            else
                File.AppendAllText(svcOutputPath, s);
        }

        private void CollectorPumpTick(object sender, EventArgs e)
        {
            if (bScheduledStartPending)
            {
                TimeSpan ts = dtStart - DateTime.Now;
                SendMessageToClients("Time remaining until collection starts: " + ts.ToString("hh\\:mm\\:ss"));
                if (ts.TotalSeconds <= 0)
                {
                    SendMessageToClients("Scheduled start time reached at " + dtStart.ToString("MM/dd/yyyy HH:mm:ss UTCzzz")
                        + ".  Starting diagnostic collection now.");
                    StartDiagnostics();
                }
            }
            else
            {
                //    SendMessageToClients(lines[lines.Count - 2] + (lines[lines.Count - 2].Length - lines[lines.Count - 2].LastIndexOf(" ") < 4 ? "." : " "));
                //    if (lines[lines.Count - 1].StartsWith("Executing AS server command to stop profiler trace... ..."))
                //    {
                //        SendMessageToClients("\r\nStarting of Profiler tracing usually completes instantly.  Since it has not completed yet, the server may be hung.  "
                //                        + "You may need to manually stop the SSAS service or kill the msmdsrv.exe process to complete capture of the diagnostic then.  "
                //                        + "All other diagnostics have been stopped already, so this may only impact the data not yet flushed to file for the Profiler trace, even in a worst case scenario.\r\n");
                //    }
                //}
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
                    {
                        SendMessageToClients("Diagnostics captured for " + ((TimeSpan)(DateTime.Now - m_StartTime)).ToString("hh\\:mm\\:ss"));
                    }
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
                }
                else
                    iCurrentTimerTicksSinceLastInterval++;

                if (DateTime.Now > dtEnd && bUseEnd)
                    StopAndFinalizeAllDiagnostics();
            }
        }

        #region EndCapture
        public void StopAndFinalizeAllDiagnostics()
        {
            bCollectionFullyInitialized = false;
            SendMessageToClients("");
            if (bRunning)
            {
                bRunning = false;
                if (bGetPerfMon)
                {
                    m_PdhHelperInstance.Dispose();
                    bPerfMonRunning = false;
                    SendMessageToClients("Stopped performance monitor logging.");
                }

                if (bGetConfigDetails)
                {
                    // Grab event logs post repro
                    EvtExportLog(IntPtr.Zero, "Application", "*", Environment.CurrentDirectory + "\\" + TraceID + "\\" + TraceID + "_Application.evtx", EventExportLogFlags.ChannelPath);
                    EvtExportLog(IntPtr.Zero, "System", "*", Environment.CurrentDirectory + "\\" + TraceID + "\\" + TraceID + "_System.evtx", EventExportLogFlags.ChannelPath);
                    SendMessageToClients("Collected Application and System event logs.");
                }

                if (bGetNetwork)
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
                    if (clients.ContainsKey(npConn.Id))
                    {
                        try
                        {
                            SendMessageToClients("Attempting impersonation as " + clients[npConn.Id] + ": " + (ImpersonateNamedPipeConnection(npConn) ? "Suceeded" : "Failed"));
                            conn.Open();
                            PerformBAKBackupAndMoveLocal(conn, srvName, ds.Name, db, sqlDbName);
                            bAuthenticated = true;
                        }
                        catch (Exception e)
                        {
                            SendMessageToClients(e.Message);
                            //if (clients[npConn.Id] != null)
                            //    SendMessageToClients("Failure backing up dateabase as connected client user " + clients[npConn.Id] + ".");
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
                                if (sRemoteAdminUser == "")
                                {
                                    // This happens if a new client connected rather than anybody entering credentials directly.
                                    npServer._connections.Sort();
                                    NamedPipeConnection<string, string> npConn = npServer._connections[npServer._connections.ToArray().Length - 2];
                                    ImpersonateNamedPipeConnection(npConn);
                                    SendMessageToClients("Attempting to authenticate newly connected client user " + clients[npConn.Id] + " on remote server " + srvName + ".");
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
                                        return;
                                    }
                                }
                                WindowsImpersonationContext impersonatedUser = null;
                                if (safeTokenHandle != null)
                                    impersonatedUser = WindowsIdentity.Impersonate(safeTokenHandle.DangerousGetHandle());
                                try
                                {
                                    conn.Open();
                                    bAuthenticated = true;
                                    bSuspendUITicking = false;
                                    PerformBAKBackupAndMoveLocal(conn, srvName, ds.Name, db, sqlDbName, impersonatedUser, sRemoteAdminDomain, sRemoteAdminUser);
                                    break;
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
                        { SendMessageToClients("Failure backing up dateabase as connected client user " + sRemoteAdminDomain + "\\" + sRemoteAdminUser + "."); }
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
                if (srvName != "." && srvName.ToLower() != "localhost" && srvName.ToLower() != Environment.MachineName.ToLower())
                    SendMessageToClients("Confirming remote file share access to copy remote database backup locally.");
                if (!Directory.Exists("\\\\" + srvName + "\\" + BackupDir.Replace(":", "$")))
                {
                    string msg = "Remote file share access failed for user " + sRemoteAdminDomain + "\\" + sRemoteAdminUser + " to server " + srvName + ".";
                    throw new Exception(msg);
                }
                    
                SendMessageToClients("Initiating backup of relational database " + SQLDBName + ".bak on SQL server " + srvName + ".");
                cmd = new OleDbCommand(@"BACKUP DATABASE [" + SQLDBName + "] TO  DISK = N'" + BackupDir + "\\SSASDiag_" + SQLDBName + ".bak' WITH NOFORMAT, INIT, NAME = N'SSASDag_" + SQLDBName + "-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, COMPRESSION, STATS = 10", conn);
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
                                    SendMessageToClients("Failure collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ":\r\n" + ex2.Message);
                                }
                            }
                        }
                        catch(Exception)
                        {
                            frmSSASDiag.LogException(ex);
                            SendMessageToClients("Failure collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ".");
                            SendMessageToClients("For local administrator accounts except Administrator to access the remote SQL backups, create a DWORD32 value LocalAccountTokenFilterPolicy=1 in HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\Policies\\System on the server " + srvName + ".");
                        }
                    }
                    else
                        SendMessageToClients("Failure collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ":\r\n" + ex.Message + "");
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
                SendMessageToClients("Failure collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ":\r\n" + ex.Message + "");
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
            SendMessageToClients("Stopping network trace.  This may take a while...");
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "netsh";
            p.StartInfo.Arguments = "trace stop";
            p.Start();
            string sOut = p.StandardOutput.ReadToEnd();
            System.Diagnostics.Trace.WriteLine("netsh trace stop output: " + sOut);
            if (sOut == "There is no trace session currently in progress.")
                SendMessageToClients("Network trace failed to capture for unknown reason.  Manual collection may be necessary.");
            p.WaitForExit();
            SendMessageToClients("Network trace stopped and collected.");
        }
        private void bgStopNetworkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker bgFinalSteps = new BackgroundWorker();
            bgFinalSteps.DoWork += bgFinalProfilerAndZipSteps;
            bgFinalSteps.RunWorkerCompleted += bgFinalProfilerAndZipSteps_Completion;
            bgFinalSteps.RunWorkerAsync();
        }
        private void bgFinalProfilerAndZipSteps(object sender, DoWorkEventArgs e)
        {
            if (bGetProfiler)
            {
                SendMessageToClients("Waiting 20s to allow profiler trace to catch up with any lagging events...");
                System.Threading.Thread.Sleep(20000); // Wait 15s to allow profiler events to catch up a little bit.
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
                            try { s.Connect("Data source=" + Environment.MachineName + (sInstanceName == "" ? "" : "\\" + sInstanceName) + ";Timeout=0;Integrated Security=SSPI;SSPI=NTLM;Initial Catalog=" + db + ";"); }
                            catch (Exception ex)
                            {
                                SendMessageToClients("Exception " + ex.Message);
                            }

                            if (bGetXMLA)
                            {
                                SendMessageToClients("Extracting database definition XMLA script for " + db + ".");
                                MajorObject[] mo = { s.Databases.FindByName(db) };

                                XmlWriter output = XmlWriter.Create(Environment.CurrentDirectory + "\\" + TraceID + "\\Databases\\" + db + ".xmla", new XmlWriterSettings() { OmitXmlDeclaration = true });
                                Microsoft.AnalysisServices.Scripter sc = new Microsoft.AnalysisServices.Scripter();
                                sc.ScriptCreate(mo, output, true);
                                output.Flush();
                                output.Close();
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

            // Just before zip, write out last line of this capture log and save that file...
            // The last line captured in text file here:
            SendMessageToClients("Stoppped SSAS diagnostics collection at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
            File.Copy(svcOutputPath, TraceID + "\\SSASDiag.log");

            if (bCompress)
            {
                SendMessageToClients("Creating zip file of output: " + Environment.CurrentDirectory + "\\" + TraceID + ".zip.");

                // Zip up all output into a single zip file.
                ZipFile z = new ZipFile();
                z.AddDirectory(TraceID );
                z.MaxOutputSegmentSize = 1024 * 1024 * (int)iRollover;
                z.Encryption = EncryptionAlgorithm.None;
                z.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                z.BufferSize = 1000000;
                z.CodecBufferSize = 1000000;
                z.Save(TraceID + ".zip");

                SendMessageToClients("Created zip file.");
            }

            if (bDeleteRaw)
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
            SendMessageToClients("Stop");
            PerfMonAndUIPumpTimer.Stop();
            bScheduledStartPending = false;
            for (int i = 0; i < npServer._connections.ToArray().Length; i++)
            {
                clients.Remove(npServer._connections[i].Id);
                npServer._connections[i].Close();
            }
            CompletionCallback();
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
                s.Connect("Data source=" + Environment.MachineName + (sInstanceName == "" ? "" : "\\" + sInstanceName) + ";Timeout=0;Integrated Security=SSPI;SSPI=NTLM;", true);
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
