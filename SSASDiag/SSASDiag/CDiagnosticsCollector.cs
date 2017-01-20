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

namespace SSASDiag
{
    public class CDiagnosticsCollector
    {
        PdhHelper m_PdhHelperInstance = new PdhHelper(false);
        System.Timers.Timer PerfMonAndUIPumpTimer = new System.Timers.Timer();
        ListBox lbStatus;
        Button btnStart;
        bool bScheduledStartPending = false;
        DateTime m_StartTime = DateTime.Now;
        string sTracePrefix = "", sInstanceName, sInstanceVersion, sInstanceMode, sInstanceEdition, TraceID, sLogDir, sConfigDir, sServiceAccount;
        int iInterval = 0, iRollover = 0, iCurrentTimerTicksSinceLastInterval = 0;
        bool bAutoRestart = false, bRollover = false, bUseStart, bUseEnd, bGetConfigDetails, bGetProfiler, bGetPerfMon, bGetNetwork;
        DateTime dtStart, dtEnd;

        public CDiagnosticsCollector(
                string TraceFilesPrefix, string InstanceName, string InstanceVersion, string InstanceMode, string InstanceEdition, string ConfigDir, string LogDir, string ServiceAccount,
                ListBox StatusListBox, Button ButtonStart,
                int Interval, 
                bool AutoRestart, 
                int RolloverMaxMB, bool Rollover, 
                DateTime Start, bool UseStart, 
                DateTime End, bool UseEnd, 
                bool GetConfigDetails, bool GetProfiler, bool GetPerfMon, bool GetNetwork)
        {
            PerfMonAndUIPumpTimer.Interval = 1000;
            PerfMonAndUIPumpTimer.Elapsed += timerTick;
            sTracePrefix = TraceFilesPrefix; sInstanceName = InstanceName; sInstanceVersion = InstanceVersion; sInstanceMode = InstanceMode; sInstanceEdition = InstanceEdition; sConfigDir = ConfigDir; sLogDir = LogDir; sServiceAccount = ServiceAccount;
            lbStatus = StatusListBox; btnStart = ButtonStart;
            iInterval = Interval;
            bAutoRestart = AutoRestart;
            iRollover = RolloverMaxMB; bRollover = Rollover;
            dtStart = Start; bUseStart = UseStart;
            dtEnd = End; bUseEnd = UseEnd;
            bGetConfigDetails = GetConfigDetails; bGetProfiler = GetProfiler; bGetNetwork = GetNetwork; bGetPerfMon = GetPerfMon;
        }

        public void StartDiagnostics()
        {
            m_StartTime = DateTime.Now;
            bScheduledStartPending = false;

            if (bUseStart && DateTime.Now < dtStart)
            {
                AddItemToStatusListBox("Scheduled Diagnostic collection starts automatically at " + dtStart.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                TimeSpan ts = dtStart - DateTime.Now;
                AddItemToStatusListBox("Time remaining until collection starts: " + ts.ToString("hh\\:mm\\:ss"));
                bScheduledStartPending = true;
                PerfMonAndUIPumpTimer.Start();
                return;
            }
            else
            {
                TraceID = sTracePrefix
                    + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd_HH-mm-ss") + "_UTC"
                    + "_SSASDiag";

                AddItemToStatusListBox("Initializing SSAS diagnostics collection at " + m_StartTime.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                AddItemToStatusListBox("Collecting on server " + Environment.MachineName + ".");
                AddItemToStatusListBox("Collecting for instance " + sInstanceName + ".");
                AddItemToStatusListBox("The version of the instance is " + sInstanceVersion + ".");
                AddItemToStatusListBox("The edition of the instance is " + sInstanceEdition + ".");
                AddItemToStatusListBox("The instance mode is " + sInstanceMode + ".");
                AddItemToStatusListBox("The OLAP\\LOG folder for the instance is " + sLogDir + ".");
                AddItemToStatusListBox("The msmdsrv.ini configuration for the instance at " + sConfigDir + ".");

                if (!Directory.Exists("Output"))
                    Directory.CreateDirectory("Output");
                else
                {
                    if (MessageBox.Show("There is an existing output directory found.\r\nDelete these files to start with a fresh output folder?", "Existing output folder found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try { Directory.Delete("Output", true); } catch { }
                        Directory.CreateDirectory("Output");
                    }
                }
                // Add explicit full control access for AS service account to our temp output location since server trace is written under that identity.  Genius!
                DirectoryInfo dirInfo = new DirectoryInfo(Environment.CurrentDirectory + "\\Output");
                DirectorySecurity dirSec = dirInfo.GetAccessControl();
                dirSec.AddAccessRule(new FileSystemAccessRule(sServiceAccount, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
                dirInfo.SetAccessControl(dirSec);

                AddItemToStatusListBox("Created temporary folder Output to collect diagnostic files.");

                if (bGetConfigDetails)
                {
                    // Collect SSAS LOG dir, config and save log of this diagnostic capture
                    if (!Directory.Exists("Output\\Log")) Directory.CreateDirectory("Output\\Log");
                    foreach (string f in Directory.GetFiles(sLogDir))
                        File.Copy(f, "Output\\Log\\" + f.Substring(f.LastIndexOf("\\") + 1), true);
                    File.Copy(sConfigDir + "\\msmdsrv.ini", "Output\\msmdsrv.ini", true);
                    AddItemToStatusListBox("Captured OLAP\\Log contents and msmdsrv.ini config for the instance.");
                }
                
                if (bGetPerfMon)
                {
                    uint r = InitializePerfLog("Output\\" + TraceID + ".blg");

                    if (r != 0)
                    {
                        AddItemToStatusListBox("Error starting PerfMon logging: " + r.ToString("X"));
                        AddItemToStatusListBox("Other diagnostic collection will still be attempted.");
                    }
                    else
                    {

                        AddItemToStatusListBox("Performance logging every " + iInterval + " seconds.");
                        AddItemToStatusListBox("Performance logging started to file: " + TraceID + ".blg.");
                    }
                }

                if (bGetProfiler)
                {
                    string XMLABatch = Properties.Resources.ProfilerTraceStartXMLA
                        .Replace("<LogFileName/>", "<LogFileName>" + AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\Output\\" + TraceID + ".trc</LogFileName>")
                        .Replace("<LogFileSize/>", bRollover ? "<LogFileSize>" + iRollover + "</LogFileSize>" : "")
                        .Replace("<LogFileRollover/>", bRollover ? "<LogFileRollover>" + bRollover.ToString().ToLower() + "</LogFileRollover>" : "")
                        .Replace("<AutoRestart/>", "<AutoRestart>" + bAutoRestart.ToString().ToLower() + "</AutoRestart>")
                        .Replace("<StartTime/>", "")
                        .Replace("<StopTime/>", bUseEnd ? "<StopTime>" + dtEnd.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "</StopTime>" : "")
                        .Replace("<ID/>", "<ID>" + TraceID + "</ID>")
                        .Replace("<Name/>", "<Name>" + TraceID + "</Name>");

                    string ret = ServerExecute(XMLABatch);

                    if (ret.Substring(0, "Success".Length) != "Success")
                        AddItemToStatusListBox("Error starting profiler trace: " + ret);
                    else
                        AddItemToStatusListBox("Profiler tracing started to file: " + TraceID + ".trc.");
                }

                if (bGetNetwork)
                {
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.FileName = "netsh";
                    p.StartInfo.Arguments = "trace start persistent=yes capture=yes tracefile=" + Environment.CurrentDirectory + "\\Output\\" + TraceID + ".etl";
                    p.Start();

                    AddItemToStatusListBox("Network tracing started to file: " + TraceID + ".etl.");
                }

                if (bRollover) AddItemToStatusListBox("Log and trace files rollover after " + iRollover + "MB.");
                if (bUseEnd) AddItemToStatusListBox("Diagnostic collection stops automatically at " + dtEnd.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                AddItemToStatusListBox("Diagnostics captured for 00:00:00...");
                lbStatus.Invoke(new System.Action(() => lbStatus.TopIndex = lbStatus.Items.Count - 1));

                // Start the timer ticking...
                PerfMonAndUIPumpTimer.Start();
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
            if (sInstanceName == "Default instance (MSSQLServer)")
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

        private void timerTick(object sender, EventArgs e)
        {
            int SelectedIndex = 0;
            lbStatus.Invoke((MethodInvoker)delegate () { SelectedIndex = lbStatus.TopIndex; });

            if (bScheduledStartPending)
            {
                TimeSpan ts = dtStart - DateTime.Now;
                AddItemToStatusListBox("Time remaining until collection starts: " + ts.ToString("hh\\:mm\\:ss"), "Time remaining until collection starts: ");
                if (ts.TotalSeconds < 0)
                {
                    AddItemToStatusListBox("Scheduled start time reached at " + dtStart.ToString("MM/dd/yyyy HH:mm:ss UTCzzz")
                        + ".  Starting diagnostic collection now.");
                    SelectedIndex = lbStatus.Items.Count - 1;
                    StartDiagnostics();
                }
            }
            else
            {
                // Update elapsed time.
                AddItemToStatusListBox("Diagnostics captured for " + ((TimeSpan)(DateTime.Now - m_StartTime)).ToString("hh\\:mm\\:ss") + "...", "Diagnostics captured for ");

                if (iCurrentTimerTicksSinceLastInterval >= iInterval)
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
            lbStatus.Invoke(new System.Action(() => lbStatus.TopIndex = SelectedIndex));
        }

        public void StopAndFinalizeAllDiagnostics()
        {
            PerfMonAndUIPumpTimer.Stop();

            if (bGetPerfMon)
            {
                m_PdhHelperInstance.Dispose();
                AddItemToStatusListBox("Stopped performance monitor logging.");
            }

            if (bGetProfiler)
            {
                ServerExecute(Properties.Resources.ProfilerTraceStopXMLA.Replace("<TraceID/>", "<TraceID>" + TraceID + "</TraceID>"));
                AddItemToStatusListBox("Stopped profiler trace.");
            }

            if (bGetNetwork)
            {
                BackgroundWorker bg = new BackgroundWorker();
                bg.DoWork += bgStopNetworkTracesAsyncDoWork;
                bg.WorkerReportsProgress = false;
                bg.WorkerSupportsCancellation = false;
                bg.RunWorkerCompleted += bgStopNetworkTracesAsyncCompleted;
                bg.RunWorkerAsync();
            }

            FinalizeCollection();

            btnStart.Invoke(new MethodInvoker(() => { btnStart.Text = "Stop Capture"; btnStart.Image = Properties.Resources.stop_button_th; } ));
            bScheduledStartPending = false;
        }

        private void FinalizeCollection()
        {
            AddItemToStatusListBox("Stoppped SSAS diagnostics collection at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");

            string status = "";

            int i = lbStatus.Items.Count - 1;
            for (; i > 0; i--)
                if (((string)lbStatus.Items[i]).StartsWith("Initializing")) break;
            for (; i < lbStatus.Items.Count - 1; i++)
                status += lbStatus.Items[i] + "\r\n";
            File.WriteAllText("Output\\SSASDiag.log", status);

            // Zip up all output into a single zip file.
            ZipFile z = new ZipFile();
            z.AddDirectory("Output");
            z.MaxOutputSegmentSize = 1024 * 1024 * (int)iRollover;
            z.Encryption = EncryptionAlgorithm.WinZipAes256;
            z.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
            z.Save(TraceID + ".zip");

            AddItemToStatusListBox("Created zip file of output at " + Environment.CurrentDirectory + "\\" + TraceID + ".zip.");
            Directory.Delete("Output", true); 
            AddItemToStatusListBox("Deleted capture output folder.");

            AddItemToStatusListBox("");
            lbStatus.Invoke(new System.Action(() => lbStatus.TopIndex = lbStatus.Items.Count - 1));
        }

        private void bgStopNetworkTracesAsyncDoWork(object sender, DoWorkEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = "netsh";
            p.StartInfo.Arguments = "trace stop";
            p.Start();
            p.WaitForExit();
        }

        private void bgStopNetworkTracesAsyncCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            AddItemToStatusListBox("Stopped network trace.");
            Thread.Sleep(1000);
            FinalizeCollection();
        }

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

        void AddItemToStatusListBox(string Item, string AtLastLineStartingWith = "")
        {
            if (AtLastLineStartingWith == "")
                lbStatus.Invoke(new System.Action(() => lbStatus.Items.Add(Item)));
            else
                for (int i = lbStatus.Items.Count - 1; i >= 0; i--)
                    if (((string)lbStatus.Items[i]).StartsWith(AtLastLineStartingWith))
                    {
                        lbStatus.Invoke(new System.Action(() => lbStatus.Items[i] = Item));
                        return;
                    }
            return;
        }
    }
}
