using Microsoft.AnalysisServices;
using PdhNative;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO.Compression;

namespace SSASDiag
{
    public partial class frmSSASDiag : Form
    {
        string m_instanceVersion = "";
        string m_instanceType = "";
        string m_instanceEdition = "";
        string m_LogDir = "";
        string m_ConfigDir = "";
        DateTime m_StartTime = DateTime.Now;
        List<ComboBoxTaggedItem> LocalInstances = new List<ComboBoxTaggedItem>();
        bool bScheduledStartPending = false;
        PdhHelper m_PdhHelperInstance = new PdhHelper(false);
        string TraceID = "";

        public frmSSASDiag()
        {
            InitializeComponent();
        }

        private void frmSSASDiag_Load(object sender, EventArgs e)
        {
            PopulateInstanceDropdown();
            dtStopTime.Value = DateTime.Now.AddHours(1);
            dtStopTime.MinDate = DateTime.Now.AddMinutes(1);
            dtStopTime.CustomFormat += TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours > 0 ? "+" 
                + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString() : TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString();
            dtStartTime.CustomFormat = dtStopTime.CustomFormat;
            dtStartTime.MinDate = DateTime.Now;
            dtStartTime.MaxDate = DateTime.Now.AddDays(30);
            cbInstances.DataSource = LocalInstances;
            cbInstances.DisplayMember = "Text";
        }

        private void frmSSASDiag_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnCapture.Text == "Stop Capture")
            {
                if (MessageBox.Show("Capture in progress, exiting will stop.\r\nExit anyway?", "Capture in progress", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    btnCapture_Click(this, new EventArgs());
                else
                    e.Cancel = true;
            }
        }

        private void timerPerfMon_Tick(object sender, EventArgs e)
        {
            int SelectedIndex = lbStatus.TopIndex;
            lbStatus.SuspendLayout();

            if (bScheduledStartPending)
            {
                TimeSpan t = dtStartTime.Value - DateTime.Now;
                lbStatus.Items[lbStatus.Items.Count - 1] = "Time remaining until collection starts: " + t.TotalHours.ToString("hh\\:mm\\:ss");
                if (t.TotalSeconds < 0)
                {
                    lbStatus.Items.Add("Scheduled start time reached at " + dtStartTime.Value.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") 
                        + ".  Starting diagnostic collection now.");
                    SelectedIndex = lbStatus.Items.Count - 1;
                    StartAllDiagnostics();
                }
            }
            else
            {

                // Update elapsed time.
                for (int i = lbStatus.Items.Count - 1; i > 0; i--)
                    if (((string)lbStatus.Items[i]).StartsWith("Diagnostics captured for "))
                    {
                        TimeSpan t = DateTime.Now - m_StartTime;
                        lbStatus.Items[i] = "Diagnostics captured for " + t.ToString("hh\\:mm\\:ss") + "...";
                        break;
                    }

                int secondsSinceLastPerfMon = 0;
                int.TryParse((string)timerPerfMon.Tag, out secondsSinceLastPerfMon);
                if (secondsSinceLastPerfMon >= udInterval.Value)
                {
                    // If perfmon logging failed we still want to tick our timer so just fail past this with try/catch anything...
                    try { m_PdhHelperInstance.UpdateLog("SSASDiag"); } catch (Exception ex) { MessageBox.Show(ex.Message); }
                    timerPerfMon.Tag = "0";
                }
                else
                    timerPerfMon.Tag = (secondsSinceLastPerfMon + 1).ToString();

                if (DateTime.Now > dtStopTime.Value && chkStopTime.Checked)
                    btnCapture_Click(timerPerfMon, new EventArgs());
            }
            lbStatus.TopIndex = SelectedIndex;
            lbStatus.ResumeLayout();
        }

        private void cbInstances_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Server srv = new Server();
                srv.Connect("Data source=." + ((cbInstances.SelectedItem as ComboBoxTaggedItem).Text == "Default instance (MSSQLServer)" ? "" : "\\" + (cbInstances.SelectedItem as ComboBoxTaggedItem).Text));
                lblInstanceDetails.Text = "Instance Details:\r\n" + srv.Version + " (" + srv.ProductLevel + "), " + srv.ServerMode + ", " + srv.Edition;
                m_instanceType = srv.ServerMode.ToString();
                m_instanceVersion = srv.Version + " - " + srv.ProductLevel;
                m_instanceEdition = srv.Edition.ToString();
                m_LogDir = srv.ServerProperties["LogDir"].Value;
                m_ConfigDir = (cbInstances.SelectedItem as ComboBoxTaggedItem).Tag;
                srv.Disconnect();
                btnCapture.Enabled = true;
            }
            catch (Exception ex)
            {
                lblInstanceDetails.Text = "Instance details could not be obtained due to failure connecting:\r\n" + ex.Message;
                btnCapture.Enabled = false;
            }
        }

        class ComboBoxTaggedItem
        {
            public string Text { get; set; }
            public string Tag { get; set; }
        }

        private void PopulateInstanceDropdown()
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                foreach (ServiceController s in services.OrderBy(ob => ob.DisplayName))
                    if (s.DisplayName.Contains("Analysis Services") && !s.DisplayName.Contains("SQL Server Analysis Services CEIP ("))
                    {
                        string ConfigPath = Registry.LocalMachine.OpenSubKey("SYSTEM\\ControlSet001\\Services\\" + s.ServiceName, false).GetValue("ImagePath") as string;
                        ConfigPath = ConfigPath.Substring(ConfigPath.IndexOf("-s \"") + "-s \"".Length).TrimEnd('\"');
                        if (s.DisplayName.Replace("SQL Server Analysis Services (", "").Replace(")", "").ToUpper() == "MSSQLSERVER")
                            LocalInstances.Insert(0, new ComboBoxTaggedItem() { Text = "Default instance (MSSQLServer)", Tag = ConfigPath });
                        else
                            LocalInstances.Add(new ComboBoxTaggedItem() { Text = s.DisplayName.Replace("SQL Server Analysis Services (", "").Replace(")", ""), Tag = ConfigPath });
                        
                        
                    }
                cbInstances.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failure during instance enumeration - could be because no instances were there.  Move on quietly then.");
                Debug.WriteLine(ex);
            }
            if (LocalInstances.Count == 0) lblInstanceDetails.Text = "There were no Analysis Services instances found on this server.\r\nPlease run on a server with a SQL 2008 or later SSAS instance.";
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            if (btnCapture.Text == "Start Capture")
            {
                btnCapture.Text = "Stop Capture";
                dtStopTime.Enabled = chkStopTime.Enabled = chkAutoRestart.Enabled = dtStartTime.Enabled = chkRollover.Enabled = chkStartTime.Enabled = udRollover.Enabled = udInterval.Enabled = cbInstances.Enabled = lblInterval.Enabled = lblInterval2.Enabled = false;

                TraceID = Environment.MachineName + "_"
                    + (cbInstances.SelectedIndex == 0 ? "" : "_" + (cbInstances.SelectedItem as ComboBoxTaggedItem).Text + "_")
                    + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd_HH-mm-ss") + "_UTC"
                    + "_SSASDiag";

                lbStatus.Items.Add("Initializing SSAS diagnostics collection at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                lbStatus.Items.Add("Collecting on server " + Environment.MachineName + ".");
                lbStatus.Items.Add("Collecting for instance " + (cbInstances.SelectedItem as ComboBoxTaggedItem).Text + ".");
                lbStatus.Items.Add("The version of the instance is " + m_instanceVersion + ".");
                lbStatus.Items.Add("The edition of the instance is " + m_instanceEdition + ".");
                lbStatus.Items.Add("The instance mode is " + m_instanceType + ".");
                lbStatus.Items.Add("The OLAP\\LOG folder for the instance is " + m_LogDir + ".");
                lbStatus.Items.Add("The msmdsrv.ini configuration for the instance at " + m_ConfigDir + ".");
               
                if (!Directory.Exists("Output"))
                    Directory.CreateDirectory("Output");
                else
                {
                    if (MessageBox.Show("There is an existing output directory found.\r\nDelete these files to start with a fresh output folder?", "Existing output folder found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try { Directory.Delete("Output"); } catch { }
                        Directory.CreateDirectory("Output");
                    }
                }
                lbStatus.Items.Add("Created temporary folder Output to collect diagnostic files.");

                // Collect SSAS LOG dir, config and save log of this diagnostic capture
                if (!Directory.Exists("Output\\Log")) Directory.CreateDirectory("Output\\Log");
                foreach (string f in Directory.GetFiles(m_LogDir))
                    File.Copy(f, "Output\\Log\\" + f.Substring(f.LastIndexOf("\\") + 1), true);
                File.Copy(m_ConfigDir + "\\msmdsrv.ini", "Output\\msmdsrv.ini", true);
                lbStatus.Items.Add("Captured OLAP\\Log contents and msmdsrv.ini config for the instance.");

                if (chkStartTime.Checked && DateTime.Now < dtStartTime.Value)
                {
                    lbStatus.Items.Add("Diagnostic collection starts automatically at " + dtStartTime.Value.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                    TimeSpan t = dtStartTime.Value - DateTime.Now;
                    lbStatus.Items.Add("Time remaining until collection starts: " + (t.TotalHours > 0 ? ((int)t.TotalHours).ToString("00") : "00") + ":" + t.Minutes.ToString("00") + ":" + t.Seconds.ToString("00"));
                    bScheduledStartPending = true;
                    timerPerfMon.Interval = 1000;
                    timerPerfMon.Start();
                }
                else
                    StartAllDiagnostics();
            }
            else
            {
                StopAndFinalizeAllDiagnostics();
            }
        }

        private void StopAndFinalizeAllDiagnostics()
        {
            bScheduledStartPending = false;
            chkStopTime.Enabled = chkAutoRestart.Enabled = chkRollover.Enabled = chkStartTime.Enabled = udInterval.Enabled = cbInstances.Enabled = lblInterval.Enabled = lblInterval2.Enabled = true;
            udRollover.Enabled = chkRollover.Checked;
            dtStartTime.Enabled = chkStartTime.Checked;
            dtStopTime.Enabled = chkStopTime.Checked;

            timerPerfMon.Stop();
            m_PdhHelperInstance.Dispose();
            lbStatus.Items.Add("Stopped performance monitor logging.");

            ServerExecute(Properties.Resources.ProfilerTraceStopXMLA.Replace("<TraceID/>", "<TraceID>" + TraceID + "</TraceID>"));
            lbStatus.Items.Add("Stopped profiler trace.");

            lbStatus.Items.Add("Stoppped SSAS diagnostics collection at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");

            string status = "";
            int i = lbStatus.Items.Count - 1;
            for (; i > 0; i--)
                if (((string)lbStatus.Items[i]).StartsWith("Initializing")) break;
            for (; i < lbStatus.Items.Count - 1; i++)
                status += lbStatus.Items[i] + "\r\n";
            File.WriteAllText("Output\\SSASDiag.log", status);

            // Zip up all output into a single zip file.
            ZipFile.CreateFromDirectory("output", TraceID + ".zip", CompressionLevel.Optimal, false);
            lbStatus.Items.Add("Created zip file of output at " + Environment.CurrentDirectory + "\\" + TraceID + ".zip.");
            Directory.Delete("Output", true);
            lbStatus.Items.Add("Deleted capture output folder.");
            
            lbStatus.Items.Add("");
            lbStatus.TopIndex = lbStatus.Items.Count - 1;



            btnCapture.Text = "Start Capture";
        }

        private void StartAllDiagnostics()
        {
            m_StartTime = DateTime.Now;
            bScheduledStartPending = false;
            uint r = InitializePerfLog();

            if (r != 0)
            {
                lbStatus.Items.Add("Error starting PerfMon logging: " + r.ToString("X"));
                lbStatus.Items.Add("Other diagnostic collection will still be attempted.");
            }
            else
            {
                lbStatus.Items.Add("Performance logging every " + udInterval.Value + " seconds.");
                lbStatus.Items.Add("Performance logging started to file: " + TraceID + ".blg.");
            }

            string XMLABatch = Properties.Resources.ProfilerTraceStartXMLA
                .Replace("<LogFileName/>", "<LogFileName>" + Environment.CurrentDirectory + "\\Output\\" + TraceID + ".trc</LogFileName>")
                .Replace("<LogFileSize/>", chkRollover.Checked ? "<LogFileSize>" + udRollover.Value + "</LogFileSize>" : "")
                .Replace("<LogFileRollover/>", chkRollover.Checked ? "<LogFileRollover>" + chkRollover.Checked.ToString().ToLower() + "</LogFileRollover>" : "")
                .Replace("<AutoRestart/>", "<AutoRestart>" + chkAutoRestart.Checked.ToString().ToLower() + "</AutoRestart>")
                .Replace("<StartTime/>", "")
                .Replace("<StopTime/>", chkStopTime.Checked ? "<StopTime>" + dtStopTime.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "</StopTime>" : "")
                .Replace("<ID/>", "<ID>" + TraceID + "</ID>")
                .Replace("<Name/>", "<Name>" + TraceID + "</Name>");

            string ret = ServerExecute(XMLABatch);

            if (ret.Substring(0, "Success".Length) != "Success")
                lbStatus.Items.Add("Error starting profiler trace: " + ret);
            else
                lbStatus.Items.Add("Profiler tracing started to file: " + TraceID + ".trc.");
            if (chkRollover.Checked) lbStatus.Items.Add("Log and trace files rollover after " + udRollover.Value + "MB.");
            if (chkStopTime.Checked) lbStatus.Items.Add("Diagnostic collection stops automatically at " + dtStopTime.Value.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
            lbStatus.Items.Add("Diagnostics captured for 00:00:00...");
            lbStatus.TopIndex = lbStatus.Items.Count - 1;
            // Start the timer ticking...
            timerPerfMon.Start();
        }

        private uint InitializePerfLog()
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
            if (cbInstances.SelectedIndex == 0)
                PerfMonInstanceID = "\\MSAS" + m_instanceVersion.Substring(0,2);
            else
                PerfMonInstanceID = "\\MSOLAP$" + (cbInstances.SelectedItem as ComboBoxTaggedItem).Text;
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
                            Environment.CurrentDirectory + "\\Output\\" + TraceID + ".blg", 
                            PdhLogFileType.PDH_LOG_TYPE_BINARY, 
                            true, 
                            chkRollover.Checked ? (uint)udRollover.Value : 0, 
                            chkRollover.Checked ? true : false, 
                            "SSAS Diagnostics Performance Monitor Log");

            return ret;
        }

        private string ServerExecute(string command)
        {
            string ret = "Success!";
            try
            {
                Server s = new Server();
                s.Connect("Data source=." + (cbInstances.SelectedIndex != 0 ? "\\" + (cbInstances.SelectedItem as ComboBoxTaggedItem).Text : ";Integrated Security=SSPI;"));
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

        private void chkAutoRestart_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoRestart.Checked && chkStopTime.Checked == false)
            {
                ttStatus.Show("Stop time required for your protection with AutoRestart=true.", dtStopTime, 1750);
                chkStopTime.Checked = true;
            }
        }

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

        private void lbStatus_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                string clip = "";
                foreach (string Item in lbStatus.Items)
                    clip += Item + "\r\n";
                Clipboard.SetData(DataFormats.StringFormat, clip);
                ttStatus.Show("Status output copied to clipboard.", lblRightClick, 10, 10, 2000);
            }
        }

        private void lkFeedback_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("mailto:SSASDiagChamps@mcirosoft.com?subject=Feedback on SSAS Diagnostics Collector Tool&cc=jburchel@microsoft.com");
        }

        private void lkBugs_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("mailto:jon.burchel@mcirosoft.com?subject=Issue with SSASDiag");
        }

        private void lkDiscussion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("mailto:SSASDiagChamps@microsoft.com");
        }

        private void frmSSASDiag_Resize(object sender, EventArgs e)
        {
            lbStatus.Width = this.Width - 50;
            lbStatus.Height = this.Height - 267;
            lblRightClick.Top = lkDiscussion.Top = lkFeedback.Top = lkBugs.Top = this.Height - 67;
            lblRightClick.Left = this.Width - 257;
            
        }
    }
}
