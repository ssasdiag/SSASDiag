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


namespace SSASDiag
{
    public class CDiagnosticsCollector : INotifyPropertyChanged
    {
        #region publics
        public bool bScheduledStartPending = false, bRunning = false, bPerfMonRunning = false;
        public System.Action CompletionCallback;
        #endregion publics

        #region toomanylocals
        PdhHelper m_PdhHelperInstance = new PdhHelper(false);
        System.Timers.Timer PerfMonAndUIPumpTimer = new System.Timers.Timer();
        RichTextBox txtStatus;
        DateTime m_StartTime = DateTime.Now;
        string sTracePrefix = "", sInstanceName, sInstanceVersion, sInstanceMode, sInstanceEdition, TraceID, sLogDir, sConfigDir, sServiceAccount;
        int iInterval = 0, iRollover = 0, iCurrentTimerTicksSinceLastInterval = 0;
        bool bAutoRestart = false, bRollover = false, bUseStart, bUseEnd, bGetConfigDetails, bGetProfiler, bGetXMLA, bGetABF, bGetBAK, bGetPerfMon, bGetNetwork, bCompress = true, bDeleteRaw = true, bPerfEvents = true;
        DateTime dtStart, dtEnd;
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
        }

        #region Properties
        private List<string> slStatus = new List<string>();
        public string[] Status
        {
            get
            {
                return slStatus.ToArray();
            }
        }
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
            m_StartTime = DateTime.Now;
            bScheduledStartPending = false;
            bRunning = true;

            // Start the timer ticking...
            PerfMonAndUIPumpTimer.Start();

            if (bUseStart && DateTime.Now < dtStart)
            {
                AddItemToStatus("Scheduled Diagnostic collection starts automatically at " + dtStart.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                TimeSpan ts = dtStart - DateTime.Now;
                AddItemToStatus("Time remaining until collection starts: " + ts.ToString("hh\\:mm\\:ss"), false);
                bScheduledStartPending = true;
                return;
            }
            else
            {
                TraceID = sTracePrefix
                    + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd_HH-mm-ss") + "_UTC"
                    + "_SSASDiag";
                AddItemToStatus("Initializing SSAS diagnostics collection at " + m_StartTime.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                if (AppDomain.CurrentDomain.GetData("Case") != null && (AppDomain.CurrentDomain.GetData("Case") as string).Trim() != "")
                    AddItemToStatus("Diagnostic collection associated with case number: " + AppDomain.CurrentDomain.GetData("Case"));
                AddItemToStatus("Collecting on computer " + Environment.MachineName + ".", true);
                if (sInstanceVersion == "")  // This occurs when we aren't really capturing instance details with Network only capture.
                {
                    AddItemToStatus("Collecting for instance " + (sInstanceName == "" ? "Default instance (MSSQLServer)" : sInstanceName) + ".");
                    AddItemToStatus("The version of the instance is " + sInstanceVersion + ".");
                    AddItemToStatus("The edition of the instance is " + sInstanceEdition + ".");
                    AddItemToStatus("The instance mode is " + sInstanceMode + ".");
                    AddItemToStatus("The OLAP\\LOG folder for the instance is " + sLogDir + ".");
                    AddItemToStatus("The msmdsrv.ini configuration for the instance at " + sConfigDir + ".");
                }

                if (!Directory.Exists(TraceID + "Output"))
                    Directory.CreateDirectory(TraceID + "Output");
                else
                {
                    if (MessageBox.Show("There is an existing output directory found.\r\nDelete these files to start with a fresh output folder?", "Existing output folder found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try { Directory.Delete(TraceID + "Output", true); } catch { }
                        Directory.CreateDirectory(TraceID + "Output");
                    }
                }
                AddItemToStatus("Created temporary folder " + TraceID + "Output to collect diagnostic files.");

                // Add explicit full control access for AS service account to our temp output location since server trace is written under that identity.
                if (sInstanceVersion != "")
                {
                    try
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(Environment.CurrentDirectory + "\\" + TraceID + "Output");
                        DirectorySecurity dirSec = dirInfo.GetAccessControl();
                        dirSec.AddAccessRule(new FileSystemAccessRule(sServiceAccount, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
                        dirInfo.SetAccessControl(dirSec);
                        AddItemToStatus("Added full control for SSAS service account " + sServiceAccount + " to the output directory.");
                    }
                    catch (Exception ex)
                    {
                        AddItemToStatus("Adding access permissions for SSAS service account " + sServiceAccount + " to output folder failed:\n\t" + ex.Message);
                    }
                }

                if (bGetConfigDetails)
                {
                    // Collect SSAS LOG dir, config and save log of this diagnostic capture
                    if (!Directory.Exists(TraceID + "Output\\Log")) Directory.CreateDirectory(TraceID + "Output\\Log");
                    foreach (string f in Directory.GetFiles(sLogDir))
                        File.Copy(f, TraceID + "Output\\Log\\" + f.Substring(f.LastIndexOf("\\") + 1));
                    File.Copy(sConfigDir + "\\msmdsrv.ini", TraceID + "Output\\msmdsrv.ini");
                    AddItemToStatus("Captured OLAP\\Log contents and msmdsrv.ini config for the instance.");

                    BackgroundWorker bg = new BackgroundWorker();
                    bg.DoWork += bgGetSPNs;
                    bg.RunWorkerCompleted += bgGetSPNsCompleted;
                    bg.RunWorkerAsync();
                }
                else
                    bgGetSPNsCompleted(null, null);
            }                   
        }
        private void bgGetSPNs(object sender, DoWorkEventArgs e)
        {
            AddItemToStatus("Attempting to capture SPNs for the AS service account " + sServiceAccount + ".");
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = "setspn";
            p.StartInfo.Arguments = "-l \"" + sServiceAccount + "\"";
            p.Start();
            string sOut = p.StandardOutput.ReadToEnd();
            string sErr = p.StandardError.ReadToEnd();
            p.WaitForExit();
            if (sErr == "")
            {
                File.WriteAllText(AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\ServiceAccountSPNs.txt", sOut);
                AddItemToStatus("Captured SPNs defined for service account " + sServiceAccount + ".");
            }
            else
            {
                AddItemToStatus("Failed to capture SPNs.  Rerun as a domain administrator if this configuration detail is required.");
                AddItemToStatus("Error capturing SPNs: " + sErr.TrimEnd(new char[] {'\r', '\n'}));
            }

        }
        private void bgGetSPNsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (bGetPerfMon)
            {
                uint r = InitializePerfLog(TraceID + "Output\\" + TraceID + ".blg");

                if (r != 0)
                {
                    AddItemToStatus("Error starting PerfMon logging: " + r.ToString("X"));
                    AddItemToStatus("Other diagnostic collection will still be attempted.");
                }
                else
                {
                    bPerfMonRunning = true;
                    AddItemToStatus("Performance logging every " + iInterval + " seconds.");
                    AddItemToStatus("Performance logging started to file: " + TraceID + ".blg.");
                }
            }

            if (bGetProfiler)
            {
                string XMLABatch = (bPerfEvents ? Properties.Resources.ProfilerTraceStartWithQuerySubcubeEventsXMLA : Properties.Resources.ProfilerTraceStartXMLA)
                    .Replace("<LogFileName/>", "<LogFileName>" + AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\" + TraceID + ".trc</LogFileName>")
                    .Replace("<LogFileSize/>", bRollover ? "<LogFileSize>" + iRollover + "</LogFileSize>" : "")
                    .Replace("<LogFileRollover/>", bRollover ? "<LogFileRollover>" + bRollover.ToString().ToLower() + "</LogFileRollover>" : "")
                    .Replace("<AutoRestart/>", "<AutoRestart>" + bAutoRestart.ToString().ToLower() + "</AutoRestart>")
                    .Replace("<StartTime/>", "")
                    .Replace("<StopTime/>", bUseEnd ? "<StopTime>" + dtEnd.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "</StopTime>" : "")
                    .Replace("<ID/>", "<ID>" + TraceID + "</ID>")
                    .Replace("<Name/>", "<Name>" + TraceID + "</Name>");

                string ret = ServerExecute(XMLABatch);

                if (ret.Substring(0, "Success".Length) != "Success")
                    AddItemToStatus("Error starting profiler trace: " + ret);
                else
                    AddItemToStatus("Profiler tracing " + (bPerfEvents ? "(including detailed performance relevant events) " : "") + "started to file: " + TraceID + ".trc.");

                if (bGetXMLA || bGetABF)
                {
                    XMLABatch = Properties.Resources.DbsCapturedTraceStartXMLA
                        .Replace("<LogFileName/>", "<LogFileName>" + AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\DatabaseNamesOnly_" + TraceID + ".trc</LogFileName>")
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
            s.Add("\\Processor(*)\\*");
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
            AddItemToStatus("Starting network trace...");
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
            p.StartInfo.Arguments = "trace start fileMode=" + (bRollover ? "circular" : "single") + " capture=yes tracefile=" + Environment.CurrentDirectory + "\\" + TraceID + "Output\\" + TraceID + ".etl maxSize=" + (bRollover ? iRollover.ToString() : "0");
            p.Start();
            sOut = p.StandardOutput.ReadToEnd();
            Debug.WriteLine("netsh trace start's output: " + sOut);
            p.WaitForExit();
            AddItemToStatus("Network tracing started to file: " + TraceID + ".etl.");
        }
        private void bgGetNetworkCompletion(object sender, RunWorkerCompletedEventArgs e)
        {
            FinalizeStart();
        }
        private void FinalizeStart()
        {
            if (bRollover) AddItemToStatus("Log and trace files rollover after " + iRollover + "MB.");
            if (bUseEnd) AddItemToStatus("Diagnostic collection stops automatically at " + dtEnd.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
            m_StartTime = DateTime.Now.AddSeconds(-1);
            AddItemToStatus("Diagnostics captured for 00:00:00");
            CompletionCallback.Invoke();
        }
        #endregion StartCapture

        private void CollectorPumpTick(object sender, EventArgs e)
        {
            if (bScheduledStartPending)
            {
                TimeSpan ts = dtStart - DateTime.Now;
                AddItemToStatus("Time remaining until collection starts: " + ts.ToString("hh\\:mm\\:ss"), false, "Time remaining until collection starts: ");
                if (ts.TotalSeconds < 0)
                {
                    AddItemToStatus("Scheduled start time reached at " + dtStart.ToString("MM/dd/yyyy HH:mm:ss UTCzzz")
                        + ".  Starting diagnostic collection now.");
                    StartDiagnostics();
                }
            }
            else
            {
                if (slStatus.Count > 0)
                    AddItemToStatus(slStatus[slStatus.Count - 1] + ".", false, slStatus[slStatus.Count - 1]);

                if (bRunning)
                {
                    // Update elapsed time.
                    AddItemToStatus("Diagnostics captured for " + ((TimeSpan)(DateTime.Now - m_StartTime)).ToString("hh\\:mm\\:ss"), false, "Diagnostics captured for ");

                    if (iCurrentTimerTicksSinceLastInterval >= iInterval && bPerfMonRunning)
                    {
                        // If perfmon logging failed we still want to tick our timer so just fail past this with try/catch anything...
                        try { m_PdhHelperInstance.UpdateLog("SSASDiag"); } catch (Exception ex) { MessageBox.Show(ex.Message); }
                        iCurrentTimerTicksSinceLastInterval = 0;
                    }
                    else
                        iCurrentTimerTicksSinceLastInterval++;

                    if (DateTime.Now > dtEnd && bUseEnd)
                        StopAndFinalizeAllDiagnostics();

                }
            }
        }

        #region EndCapture
        public void StopAndFinalizeAllDiagnostics()
        {
            if (bRunning)
            {
                bRunning = false;
                if (bGetPerfMon)
                {
                    m_PdhHelperInstance.Dispose();
                    bPerfMonRunning = false;
                    AddItemToStatus("Stopped performance monitor logging.");
                }

                if (bGetConfigDetails)
                {
                    // Grab those event logs post repro!
                    EvtExportLog(IntPtr.Zero, "Application", "*", AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\Application.evtx", EventExportLogFlags.ChannelPath);
                    EvtExportLog(IntPtr.Zero, "System", "*", AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\System.evtx", EventExportLogFlags.ChannelPath);
                    AddItemToStatus("Collected Application and System event logs.");
                }

                if (bGetProfiler)
                {
                    ServerExecute(Properties.Resources.ProfilerTraceStopXMLA.Replace("<TraceID/>", "<TraceID>" + TraceID + "</TraceID>"));
                    AddItemToStatus("Stopped profiler trace.");

                    if (bGetXMLA || bGetABF)
                    {
                        string[] dbs = { };

                        #region X86 TraceFile reader workaround
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // TraceFile binaries for Microsoft.SqlServer.Management.Trace are only available in x86.
                        // The SSASDiag process needs to be X64 to optimally work with large files.
                        // Workaround here, costs a few extra seconds to invoke at stop time but worth it
                        // Call the simple X86 ExtractDbNamesFromTrace process from SSASDiag.  

                        AddItemToStatus("Finding databases with queries/commands started/completed during tracing...");
                        ServerExecute(Properties.Resources.ProfilerTraceStopXMLA.Replace("<TraceID/>", "<TraceID>dbsOnly" + TraceID + "</TraceID>"));
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.FileName = Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\ExtractDbNamesFromTrace.exe";
                        p.StartInfo.Arguments = 
                            AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\DatabaseNamesOnly_" + TraceID + "1.trc " 
                            + AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\DatabaseNamesOnly_" + TraceID + "1.txt";
                        p.Start();
                        p.WaitForExit();
                        if (File.Exists(AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\DatabaseNamesOnly_" + TraceID + "1.txt"))
                        {
                            dbs = File.ReadAllLines(AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\DatabaseNamesOnly_" + TraceID + "1.txt");
                            File.Delete(AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\DatabaseNamesOnly_" + TraceID + "1.txt");
                        }
                        if (File.Exists(AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\DatabaseNamesOnly_" + TraceID + "1.trc"))
                            File.Delete(AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\DatabaseNamesOnly_" + TraceID + "1.trc");
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        #endregion X86 TraceFile reader workaround

                        if (dbs.Length == 0)
                            AddItemToStatus("There were no databases captured in the trace.  No AS database definitions or backups will be captured.");
                        else
                        {
                            Microsoft.AnalysisServices.Server s = new Microsoft.AnalysisServices.Server();
                            s.Connect("." + (sInstanceName == "" ? "" : "\\" + sInstanceName));

                            if (!Directory.Exists(AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\Databases"))
                                Directory.CreateDirectory(AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\Databases");

                            foreach (string db in dbs)
                            {
                                if (bGetXMLA)
                                {
                                    AddItemToStatus("Extracting database definition XMLA script for " + db + "...");
                                    MajorObject[] mo = { s.Databases.FindByName(db) };

                                    XmlWriter output = XmlWriter.Create(AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\Databases\\" + db + ".xmla", new XmlWriterSettings() { OmitXmlDeclaration = true });
                                    Microsoft.AnalysisServices.Scripter sc = new Microsoft.AnalysisServices.Scripter();
                                    sc.ScriptCreate(mo, output, true);
                                    output.Flush();
                                    output.Close();

                                    if (bGetBAK)
                                        GetBAK(db, s);
                                }
                                if (bGetABF)
                                {
                                    AddItemToStatus("Backing up AS database .abf for " + db + "...");
                                    string batch = Properties.Resources.BackupDbXMLA
                                        .Replace("<DatabaseID/>", "<DatabaseID>" + s.Databases.FindByName(db).ID  + "</DatabaseID>")
                                        .Replace("<File/>", "<File>" + AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\Databases\\" + db + ".abf</File>")
                                        .Replace("<AllowOverwrite/>", "<AllowOverwrite>true</AllowOverwrite>");
                                    string ret = ServerExecute(batch);
                                    if (ret != "Success!")
                                        AddItemToStatus("Error backing up AS database for " + db + ":\n\t" + ret);
                                }
                            }
                        }
                    }
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
                    // Zip the data as last background worker process, 
                    // from here if we skipped network traces,
                    // otherwise, from network trace completion below.
                    BackgroundWorker bgZipData = new BackgroundWorker();
                    bgZipData.DoWork += bgZipDataWorker;
                    bgZipData.RunWorkerCompleted += bgZipData_Completion;
                    bgZipData.RunWorkerAsync();
                }
            }
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
                    cs += "Integrated Security=SSPI;";
                }
                if (cs.Contains("Password"))
                cs = cs.Remove(cs.IndexOf("Password="), cs.IndexOf(";", cs.IndexOf("Password=")));

                AddItemToStatus("Starting SQL datasource backup for AS database " + db + ", data source name " + ds.Name + ", SQL database " + sqlDbName + " on server " + (srvName == "." ? Environment.MachineName : srvName) + ".");

                OleDbConnection conn = new OleDbConnection(cs);
                bool bAuthenticated = false;

                try
                {
                    // Try first just with our current credentials as local administrator.
                    // This will work of course with local dbs, and with remote if we are admins there too.
                    conn.Open();
                    PerformBAKBackupAndMoveLocal(conn, srvName, ds.Name, db, sqlDbName);
                }
                catch (OleDbException ex)
                {
                    if (ex.Message.StartsWith("Login failed"))
                    {
                        // If it fails the first try, prompt for remote admin
                        PasswordPrompt pp = new PasswordPrompt();
                        pp.UserMessage = "Windows Administrator required for remote server:\r\n" + srvName
                            + "\r\n\r\nFor data source name:\r\n" + ds.Name
                            + "\r\n\r\nIn AS database:\r\n" + db;

                        int iTries = 0;
                        while (!bAuthenticated && iTries < 3)
                        {
                            Form f = Application.OpenForms["frmSSASDiag"];  // need a better way to center in this parent cross-thread but for now this will achieve it...
                            txtStatus.Invoke(new System.Action(() =>
                            {
                                pp.Top = f.Top + f.Height / 2 - pp.Height / 2;
                                pp.Left = f.Left + f.Width / 2 - pp.Width / 2;
                            }));
                            pp.ShowDialog();
                            if (pp.DialogResult == DialogResult.OK)
                            {
                                // Impersonate user remotely
                                SafeTokenHandle safeTokenHandle;
                                const int LOGON32_PROVIDER_DEFAULT = 0;
                                const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
                                bAuthenticated = LogonUser(pp.User, pp.Domain, pp.Password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);
                                if (!bAuthenticated)
                                {
                                    pp.lblUserPasswordError.Visible = true;
                                    iTries++;
                                }
                                else
                                {
                                    using (WindowsImpersonationContext impersonatedUser = WindowsIdentity.Impersonate(safeTokenHandle.DangerousGetHandle()))
                                    {
                                        try
                                        {
                                            conn.Open();
                                            bAuthenticated = true;
                                            PerformBAKBackupAndMoveLocal(conn, srvName, ds.Name, db, sqlDbName, impersonatedUser, pp.Domain, pp.User);
                                            break;
                                        }
                                        catch (OleDbException)
                                        {
                                            pp.lblUserPasswordError.Visible = true;
                                            bAuthenticated = false;
                                            iTries++;
                                        }
                                    }
                                }
                            }
                            else
                                break;
                        }
                    }
                    else
                        AddItemToStatus("Error during backup: " + ex.Message);
                }
                if (!bAuthenticated)
                {
                    AddItemToStatus("Unable to login to backup SQL data source " + ds.Name + " in database " + db + ".");
                }
            }
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
                AddItemToStatus("Initiating backup of relational database " + SQLDBName + ".bak on SQL server " + srvName + "...");
                cmd = new OleDbCommand(@"BACKUP DATABASE [" + SQLDBName + "] TO  DISK = N'" + BackupDir + "\\SSASDiag_" + SQLDBName + ".bak' WITH NOFORMAT, INIT, NAME = N'SSASDag_" + SQLDBName + "-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, COMPRESSION, STATS = 10", conn);
                int ret = cmd.ExecuteNonQuery();
                AddItemToStatus("Database backup completed.");
                AddItemToStatus("Moving SQL backup to local capture directory...");
                try
                {
                    CopyBAKLocal(srvName, BackupDir, SQLDBName);
                }
                catch(Exception ex)
                {
                    if (ex.Message.Contains("Could not find file") && (Domain == srvName || Domain == srvName.Substring(0, srvName.IndexOf(".")) || Domain == "."))
                    {
                        try
                        {
                            cmd.CommandText = @"EXEC xp_regwrite @rootkey='HKEY_LOCAL_MACHINE', @key='Software\Microsoft\Windows\CurrentVersion\Policies\System', @value_name='LocalAccountTokenFilterPolicy', @type='REG_DWORD', @value=1";
                            if (cmd.ExecuteNonQuery() != 0)
                            {
                                AddItemToStatus("Failure collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ".");
                                AddItemToStatus("\r\nFor local administrator accounts except Administrator to access the remote SQL backup, create a DWORD32 value LocalAccountTokenFilterPolicy=1 in HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\Policies\\System on the local server " + srvName + ".\r\n");
                            }
                            else
                            {
                                try
                                {
                                    CopyBAKLocal(srvName, BackupDir, SQLDBName);
                                }
                                catch (Exception ex2)
                                {
                                    AddItemToStatus("Failure collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ":\r\n" + ex2.Message);
                                }
                            }
                        }
                        catch(Exception)
                        {
                            AddItemToStatus("Failure collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ".");
                            AddItemToStatus("\r\nFor local administrator accounts except Administrator to access the remote SQL backup, create a DWORD32 value LocalAccountTokenFilterPolicy=1 in HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\Policies\\System on the local server " + srvName + ".\r\n");
                        }
                    }
                    else
                        AddItemToStatus("Failure collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ":\r\n" + ex.Message);
                    AddItemToStatus("Please collect .bak manually from " + BackupDir + " on server " + srvName + ".");
                    return false;
                }
                AddItemToStatus("Collected SQL data source .bak backup for data source " + dsName + " in database " + ASdbName + ".");
            }
            catch (Exception ex)
            {
                AddItemToStatus("Failure collecting SQL data source .bak for data source " + dsName + " in database " + ASdbName + ":\r\n" + ex.Message);
            }
            return true;
        }
        void CopyBAKLocal(string srvName, string BackupDir, string SQLDBName)
        {
            if (srvName != "." && srvName.ToUpper() != Environment.MachineName.ToUpper() && srvName.ToUpper() != GetFQDN())
                File.Move("\\\\" + srvName + "\\" + BackupDir.Replace(":", "$") + "\\SSASDiag_" + SQLDBName + ".bak", AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\Databases\\" + SQLDBName + ".bak");
            else
                File.Move(BackupDir + "\\SSASDiag_" + SQLDBName + ".bak", AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + TraceID + "Output\\Databases\\" + SQLDBName + ".bak");
        }
        private void bgStopNewtworkWorker(object sender, DoWorkEventArgs e)
        {
            AddItemToStatus("Stopping network trace.  This may take a while...");
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "netsh";
            p.StartInfo.Arguments = "trace stop";
            p.Start();
            string sOut = p.StandardOutput.ReadToEnd();
            Debug.WriteLine("netsh trace stop output: " + sOut);
            if (sOut == "There is no trace session currently in progress.")
                AddItemToStatus("Network trace failed to capture for unknown reason.  Manual collection may be necessary.");
            p.WaitForExit();
        }
        private void bgStopNetworkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker bgZipData = new BackgroundWorker();
            bgZipData.DoWork += bgZipDataWorker;
            bgZipData.RunWorkerCompleted += bgZipData_Completion;
            bgZipData.RunWorkerAsync();
        }
        private void bgZipDataWorker(object sender, DoWorkEventArgs e)
        {
            // Just before zip, write out last line of this capture log and save that file...
            // The last line captured in text file here:
            AddItemToStatus("Stoppped SSAS diagnostics collection at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
            // And save the log:
            string logtext = "";
            int i = slStatus.Count - 1;
            for (; i > 0; i--)
                if (slStatus[i].StartsWith("Initializing")) break;
            for (; i < slStatus.Count - 1; i++)
                logtext += slStatus[i] + "\r\n";
            File.WriteAllText(TraceID + "Output\\SSASDiag.log", logtext);

            if (bCompress)
            {
                AddItemToStatus("Creating zip file of output: " + Environment.CurrentDirectory + "\\" + TraceID + ".zip...");

                // Zip up all output into a single zip file.
                ZipFile z = new ZipFile();
                z.AddDirectory(TraceID + "Output");
                z.MaxOutputSegmentSize = 1024 * 1024 * (int)iRollover;
                z.Encryption = EncryptionAlgorithm.None;
                z.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                z.BufferSize = 1000000;
                z.CodecBufferSize = 1000000;
                z.Save(TraceID + ".zip");

                AddItemToStatus("Created zip file.");
            }

            if (bDeleteRaw)
            {
                try
                {
                    Directory.Delete(TraceID + "Output", true);
                    AddItemToStatus("Deleted capture output folder.");
                }
                catch
                {
                    AddItemToStatus("Failed to delete output folder:\n"
                        + "\tThis could be due to locked files in the folder and suggests possible failure stopping a trace.\n"
                        + "\tPlease review the contents of the folder " + TraceID + "Output\n."
                        + "\tIt was created in the same location where you ran this utility.");
                } 
            }
        }
        private void bgZipData_Completion(object sender, RunWorkerCompletedEventArgs e)
        {
            FinalizeStop();
        }
        private void FinalizeStop()
        {
            AddItemToStatus("SSASDiag completed at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
            PerfMonAndUIPumpTimer.Stop();
            bScheduledStartPending = false;
            txtStatus.Invoke(new System.Action(() => CompletionCallback()));
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
                s.Connect("Data source=." + (sInstanceName == "" ? "" : "\\" + sInstanceName));
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
                finally { s.Disconnect(); }
            }
            catch (Exception e) { return "Error: " + e.Message; }
            return ret;
        }
        void AddItemToStatus(string Item, bool bScroll = true, string AtLastLineStartingWith = "")
        {
            int iCurStatusPos = 0;
            txtStatus.Invoke(new System.Action(() => iCurStatusPos = txtStatus.SelectionStart));

            if (AtLastLineStartingWith == "")
                slStatus.Insert(slStatus.Count, Item);                
            else
                for (int i = slStatus.Count - 1; i >= 0; i--)
                    if (slStatus[i].StartsWith(AtLastLineStartingWith))
                        slStatus[i] = Item;
            
            if (bScroll) txtStatus.Invoke(new System.Action(() =>
                                {
                                    txtStatus.Select(txtStatus.Text.Length, 0);
                                    txtStatus.ScrollToCaret();
                                }));
            txtStatus.Invoke(new System.Action(() => RaisePropertyChanged("Status")));
            return;
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
