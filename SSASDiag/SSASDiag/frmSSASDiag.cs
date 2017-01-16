using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.ServiceProcess;
using System.Data.Sql;
using PdhNative;
using Microsoft.SqlServer.Management.Trace;

namespace SSASDiag
{
    public partial class frmSSASDiag : Form
    {
        string m_instanceVersion = "";
        string m_instanceType = "";
        string m_instanceEdition = "";
        PdhHelper m_PdhHelperInstance = new PdhHelper(false);
        Timer m_PerfMonIntervalTimer = new Timer();
        string TraceID = "";

        public frmSSASDiag()
        {
            InitializeComponent();
        }

        private void frmSSASDiag_Load(object sender, EventArgs e)
        {
            PopulateInstanceDropdown();

            m_PerfMonIntervalTimer.Tick += PerfMonIntervalTimer_Tick;
            dtStopTime.Value = DateTime.Now.AddHours(1);
            dtStopTime.MinDate = DateTime.Now.AddMinutes(1);
            dtStopTime.CustomFormat += TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours > 0 ? "+" + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString() : TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString();
        }

        private void PerfMonIntervalTimer_Tick(object sender, EventArgs e)
        {
            m_PdhHelperInstance.UpdateLog("SSASDiag");
            if (!((string)lbStatus.Items[lbStatus.Items.Count - 1]).StartsWith("."))
                lbStatus.Items.Add(".");
            else
            {
                // I wish the character width were exposed directly but I just counted the number of dots at the selected font that fit before it fills up...  :|
                if (((string)lbStatus.Items[lbStatus.Items.Count - 1]).Length > 183) 
                    lbStatus.Items.Add(".");
                else
                    lbStatus.Items[lbStatus.Items.Count - 1] += ".";
            }
            lbStatus.TopIndex = lbStatus.Items.Count - 1;

            if (DateTime.Now > dtStopTime.Value && chkStopTime.Checked)
                btnCapture_Click(m_PerfMonIntervalTimer, new EventArgs());
        }

        private void cbInstances_SelectedIndexChanged(object sender, EventArgs e)
        {
            Server srv = new Server();
            srv.Connect("." + (cbInstances.SelectedItem.ToString() == "Default instance (MSSQLServer)" ? "" : "\\" + cbInstances.SelectedItem));
            lblInstanceDetails.Text = "Instance Details:\r\n" + srv.Version + " (" + srv.ProductLevel + "), " + srv.ServerMode + ", " + srv.Edition;
            m_instanceType = srv.ServerMode.ToString();
            m_instanceVersion = srv.Version + " - " + srv.ProductLevel;
            m_instanceEdition = srv.Edition.ToString();
            srv.Disconnect();
            btnCapture.Enabled = true;
        }

        private void PopulateInstanceDropdown()
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services.OrderBy(ob => ob.DisplayName))
                if (s.DisplayName.Contains("Analysis Services") && !s.DisplayName.Contains("SQL Server Analysis Services CEIP ("))
                if (s.DisplayName.Replace("SQL Server Analysis Services (", "").Replace(")", "").ToUpper() == "MSSQLSERVER")
                    cbInstances.Items.Insert(0, "Default instance (MSSQLServer)");
                else
                    cbInstances.Items.Add(s.DisplayName.Replace("SQL Server Analysis Services (", "").Replace(")", ""));
            cbInstances.SelectedIndex = 0;
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            if (btnCapture.Text == "Start Capture")
            {
                btnCapture.Text = "Stop Capture";
                chkAutoRestart.Enabled = dtStopTime.Enabled = chkRollover.Enabled = chkStopTime.Enabled = udRollover.Enabled = udInterval.Enabled = cbInstances.Enabled = lblInterval.Enabled = lblInterval2.Enabled = false;

                TraceID = Environment.MachineName + "_" 
                    + (cbInstances.SelectedIndex == 0 ? "" : "_" + cbInstances.SelectedItem.ToString() + "_") 
                    + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd_HH-mm-ss") + "_UTC" 
                    + "_SSASDiag";

                lbStatus.Items.Add("Initializing SSAS diagnostics collection at "
                    + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz")
                    + ".");
                lbStatus.Items.Add("Collecting on server " + Environment.MachineName + ".");
                lbStatus.Items.Add("Collecting for instance " + cbInstances.SelectedItem + ".");
                lbStatus.Items.Add("The version of the instance is " + m_instanceVersion + ".");
                lbStatus.Items.Add("The edition of the instance is " + m_instanceEdition + ".");

                StartPerfMon();

                lbStatus.Items.Add("Performance logging to file: " + TraceID + ".blg.");
                lbStatus.Items.Add("Performance logging every " + udInterval.Value + " seconds.");

                string XMLABatch = Properties.Resources.ProfilerTraceStartXMLA
                    .Replace("<LogFileName/>", "<LogFileName>" + Environment.CurrentDirectory + "\\" + TraceID + ".trc</LogFileName>")
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
                    lbStatus.Items.Add("Started profiler tracing to file: " + TraceID + ".trc.");
                if (chkRollover.Checked) lbStatus.Items.Add("Log and trace files rollover after " + udRollover.Value + "MB.");
                if (chkStopTime.Checked) lbStatus.Items.Add("Diagnostic collection stops automatically at " + dtStopTime.Value.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".");
                lbStatus.TopIndex = lbStatus.Items.Count - 1;
            }
            else
            {
                btnCapture.Text = "Start Capture";
                chkAutoRestart.Enabled = chkRollover.Enabled = chkStopTime.Enabled = udInterval.Enabled = cbInstances.Enabled = lblInterval.Enabled = lblInterval2.Enabled = true;
                udRollover.Enabled = chkRollover.Checked;
                dtStopTime.Enabled = chkStopTime.Checked;

                StopPerfMon();
                lbStatus.Items.Add("Stopped performance monitor logging.");
                ServerExecute(Properties.Resources.ProfilerTraceStopXMLA.Replace("<TraceID/>", "<TraceID>" + TraceID + "</TraceID>"));
                lbStatus.Items.Add("Stopped profiler trace.");
                lbStatus.Items.Add("Stoppped SSAS diagnostics collection at "
                    + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz")
                    + ".");
                lbStatus.Items.Add("");
                lbStatus.TopIndex = lbStatus.Items.Count - 1;
            }
        }

        private uint StopPerfMon()
        {
            m_PerfMonIntervalTimer.Stop();
            m_PdhHelperInstance.Dispose();
            
            return 0;
        }

        private uint StartPerfMon()
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
                PerfMonInstanceID = "\\MSOLAP$" + cbInstances.SelectedItem;
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
            uint ret = m_PdhHelperInstance.OpenLogForWriting(TraceID + ".blg", PdhLogFileType.PDH_LOG_TYPE_BINARY, true, chkRollover.Checked ? (uint)udRollover.Value : 0, chkRollover.Checked ? true : false, "SSAS Diagnostics Performance Monitor Log");
            if (ret == 0)
            {
                m_PerfMonIntervalTimer.Interval = (int)udInterval.Value * 1000;
                m_PerfMonIntervalTimer.Start();
            }
            return ret;
        }

        private string ServerExecute(string command)
        {
            string ret = "Success!";
            try
            {
                Server s = new Server();
                s.Connect("Data source=." + (cbInstances.SelectedIndex != 0 ? "\\" + cbInstances.SelectedItem.ToString() : ""));
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
        private void ChkRollover_CheckedChanged(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void chkAutoRestart_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoRestart.Checked) chkStopTime.Checked = true;
        }

        private void chkRollover_CheckedChanged_1(object sender, EventArgs e)
        {
            if (chkRollover.Checked) udRollover.Enabled = true; else udRollover.Enabled = false;
        }

        private void chkStopTime_CheckedChanged(object sender, EventArgs e)
        {
            dtStopTime.Enabled = chkStopTime.Checked;
            if (!chkStopTime.Checked) chkAutoRestart.Checked = false;
        }

        private void lbStatus_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                string clip = "";
                foreach (string Item in lbStatus.Items)
                    clip += Item + "\r\n";
                Clipboard.SetData(DataFormats.StringFormat, clip);
                ttStatus.Show("Status output copied to clipboard.", lblRightClick, 1000);
            }
        }
    }
}
