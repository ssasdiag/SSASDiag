using System.Text;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using System.Security.AccessControl;
using Ionic.Zip;
using Microsoft.AnalysisServices;
using PdhNative;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Management;
using System.Threading.Tasks;

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
        bool bAutoRestart = false, bRollover = false, bUseStart, bUseEnd, bGetConfigDetails, bGetProfiler, bGetPerfMon, bGetNetwork, bCompress = true, bDeleteRaw = true, bPerfEvents = true;
        DateTime dtStart, dtEnd;
        #endregion toomanylocals

        public CDiagnosticsCollector(
                string TraceFilesPrefix, string InstanceName, string InstanceVersion, string InstanceMode, string InstanceEdition, string ConfigDir, string LogDir, string ServiceAccount,
                RichTextBox StatusTextBox,
                int Interval, 
                bool AutoRestart, bool Compress, bool DeleteRaw, bool IncludePerfEventsInProfiler,
                int RolloverMaxMB, bool Rollover, 
                DateTime Start, bool UseStart, 
                DateTime End, bool UseEnd, 
                bool GetConfigDetails, bool GetProfiler, bool GetPerfMon, bool GetNetwork)
        {
            PerfMonAndUIPumpTimer.Interval = 1000;
            PerfMonAndUIPumpTimer.Elapsed += CollectorPumpTick;
            sTracePrefix = TraceFilesPrefix; sInstanceName = InstanceName; sInstanceVersion = InstanceVersion; sInstanceMode = InstanceMode; sInstanceEdition = InstanceEdition; sConfigDir = ConfigDir; sLogDir = LogDir; sServiceAccount = ServiceAccount;
            txtStatus = StatusTextBox;
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
                AddItemToStatus("Collecting on server " + Environment.MachineName + ".", true);
                AddItemToStatus("Collecting for instance " + (sInstanceName == "" ? "Default instance (MSSQLServer)" : sInstanceName) + ".");
                AddItemToStatus("The version of the instance is " + sInstanceVersion + ".");
                AddItemToStatus("The edition of the instance is " + sInstanceEdition + ".");
                AddItemToStatus("The instance mode is " + sInstanceMode + ".");
                AddItemToStatus("The OLAP\\LOG folder for the instance is " + sLogDir + ".");
                AddItemToStatus("The msmdsrv.ini configuration for the instance at " + sConfigDir + ".");

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
                // Add explicit full control access for AS service account to our temp output location since server trace is written under that identity.  Genius!
                DirectoryInfo dirInfo = new DirectoryInfo(Environment.CurrentDirectory + "\\" + TraceID + "Output");
                DirectorySecurity dirSec = dirInfo.GetAccessControl();
                dirSec.AddAccessRule(new FileSystemAccessRule(sServiceAccount, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
                dirInfo.SetAccessControl(dirSec);

                AddItemToStatus("Created temporary folder " + TraceID + "Output to collect diagnostic files.");

                if (bGetConfigDetails)
                {
                    // Collect SSAS LOG dir, config and save log of this diagnostic capture
                    if (!Directory.Exists(TraceID + "Output\\Log")) Directory.CreateDirectory(TraceID + "Output\\Log");
                    foreach (string f in Directory.GetFiles(sLogDir))
                        File.Copy(f, TraceID + "Output\\Log\\" + f.Substring(f.LastIndexOf("\\") + 1));
                    File.Copy(sConfigDir + "\\msmdsrv.ini", TraceID + "Output\\msmdsrv.ini");
                    AddItemToStatus("Captured OLAP\\Log contents and msmdsrv.ini config for the instance.");
                }
                
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
            AddItemToStatus("Starting network trace. ");
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

                if (bGetProfiler)
                {
                    ServerExecute(Properties.Resources.ProfilerTraceStopXMLA.Replace("<TraceID/>", "<TraceID>" + TraceID + "</TraceID>"));
                    AddItemToStatus("Stopped profiler trace.");
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
        private void bgStopNewtworkWorker(object sender, DoWorkEventArgs e)
        {
            AddItemToStatus("Stopping network trace.  This may take a while. ");
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
                AddItemToStatus("Creating zip file of output: " + Environment.CurrentDirectory + "\\" + TraceID + ".zip. ");

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
            PerfMonAndUIPumpTimer.Stop();
            bScheduledStartPending = false;
            txtStatus.Invoke(new System.Action(() => CompletionCallback()));
        }
        #endregion EndCapture

        #region UtilityFunctions
        private string ServerExecute(string command)
        {
            string ret = "Success!";
            try
            {
                Server s = new Server();
                s.Connect("Data source=." + sInstanceName);
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
}
