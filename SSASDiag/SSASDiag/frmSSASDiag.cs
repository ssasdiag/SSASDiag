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
using Ionic.Zip;
using System.Threading;
using System.Management;

namespace SSASDiag
{
    public partial class frmSSASDiag : Form
    {
        string m_instanceVersion = "";
        string m_instanceType = "";
        string m_instanceEdition = "";
        CDiagnosticsCollector dc;
        string m_LogDir = "", m_ConfigDir = "";  
        
        List<ComboBoxServiceDetailsItem> LocalInstances = new List<ComboBoxServiceDetailsItem>();

        public frmSSASDiag()
        {
            InitializeComponent();
        }

        private void frmSSASDiag_Load(object sender, EventArgs e)
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.GetData("originalbinlocation") as string;
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
            if (!(Environment.OSVersion.Version.Major > 7  || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1)))
            {
                MessageBox.Show("Network tracing with this tool requires\nWindows 7 or Server 2008 R2 or greater.\nPlease upgrade your OS to use that feature.", "Network Capture Not Supported on Legacy OS", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                chkGetNetwork.Enabled = false;
            }
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

        private void cbInstances_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Server srv = new Server();
                srv.Connect("Data source=." + ((cbInstances.SelectedItem as ComboBoxServiceDetailsItem).Text == "Default instance (MSSQLServer)" ? "" : "\\" + (cbInstances.SelectedItem as ComboBoxServiceDetailsItem).Text));
                lblInstanceDetails.Text = "Instance Details:\r\n" + srv.Version + " (" + srv.ProductLevel + "), " + srv.ServerMode + ", " + srv.Edition;
                m_instanceType = srv.ServerMode.ToString();
                m_instanceVersion = srv.Version + " - " + srv.ProductLevel;
                m_instanceEdition = srv.Edition.ToString();
                m_LogDir = srv.ServerProperties["LogDir"].Value;
                m_ConfigDir = (cbInstances.SelectedItem as ComboBoxServiceDetailsItem).ConfigPath;
                srv.Disconnect();
                btnCapture.Enabled = true;
            }
            catch (Exception ex)
            {
                lblInstanceDetails.Text = "Instance details could not be obtained due to failure connecting:\r\n" + ex.Message;
                btnCapture.Enabled = false;
            }
        }

        class ComboBoxServiceDetailsItem
        {
            public string Text { get; set; }
            public string ConfigPath { get; set; }
            public string ServiceAccount { get; set; }
        }

        private void PopulateInstanceDropdown()
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                foreach (ServiceController s in services.OrderBy(ob => ob.DisplayName))
                    if (s.DisplayName.Contains("Analysis Services") && !s.DisplayName.Contains("SQL Server Analysis Services CEIP ("))
                    {
                        SelectQuery sQuery = new SelectQuery(string.Format("select name, startname, pathname from Win32_Service where name = \"" + s.ServiceName + "\""));
                        ManagementObjectSearcher mgmtSearcher = new ManagementObjectSearcher(sQuery);
                        string sSvcUser = "";
                        foreach (ManagementObject svc in mgmtSearcher.Get())
                            sSvcUser = svc["startname"] as string;
                        
                        string ConfigPath = Registry.LocalMachine.OpenSubKey("SYSTEM\\ControlSet001\\Services\\" + s.ServiceName, false).GetValue("ImagePath") as string;
                        ConfigPath = ConfigPath.Substring(ConfigPath.IndexOf("-s \"") + "-s \"".Length).TrimEnd('\"');
                        if (s.DisplayName.Replace("SQL Server Analysis Services (", "").Replace(")", "").ToUpper() == "MSSQLSERVER")
                            LocalInstances.Insert(0, new ComboBoxServiceDetailsItem() { Text = "Default instance (MSSQLServer)", ConfigPath = ConfigPath, ServiceAccount = sSvcUser});
                        else
                            LocalInstances.Add(new ComboBoxServiceDetailsItem() { Text = s.DisplayName.Replace("SQL Server Analysis Services (", "").Replace(")", ""), ConfigPath = ConfigPath, ServiceAccount = sSvcUser });
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
                ComboBoxServiceDetailsItem cbsdi = cbInstances.SelectedItem as ComboBoxServiceDetailsItem;
                btnCapture.Image = Properties.Resources.stop_button_th;
                groupBox1.Enabled = dtStopTime.Enabled = chkStopTime.Enabled = chkAutoRestart.Enabled = dtStartTime.Enabled = chkRollover.Enabled = chkStartTime.Enabled = udRollover.Enabled = udInterval.Enabled = cbInstances.Enabled = lblInterval.Enabled = lblInterval2.Enabled = false;
                string TracePrefix = Environment.MachineName + "_"
                    + (cbInstances.SelectedIndex == 0 ? "" : "_" + cbsdi.Text + "_");
                dc = new CDiagnosticsCollector(TracePrefix, cbInstances.SelectedIndex == 0 ? "" : "_" + cbsdi.Text, m_instanceVersion, m_instanceType, m_instanceEdition, m_ConfigDir, m_LogDir, cbsdi.ServiceAccount,
                    lbStatus, btnCapture,
                    (int)udInterval.Value, chkAutoRestart.Checked, (int)udRollover.Value, chkRollover.Checked, dtStartTime.Value, chkStartTime.Checked, dtStopTime.Value, chkStopTime.Checked,
                    chkGetConfigDetails.Checked, chkGetProfiler.Checked, chkGetPerfMon.Checked, chkGetNetwork.Checked);
                dc.StartDiagnostics();
            }
            else
            {
                dc.StopAndFinalizeAllDiagnostics();
                groupBox1.Enabled = chkStopTime.Enabled = chkAutoRestart.Enabled = chkRollover.Enabled = chkStartTime.Enabled = udInterval.Enabled = cbInstances.Enabled = lblInterval.Enabled = lblInterval2.Enabled = true;
                udRollover.Enabled = chkRollover.Checked;
                dtStartTime.Enabled = chkStartTime.Checked;
                dtStopTime.Enabled = chkStopTime.Checked;
                btnCapture.Text = "Start Capture";
                btnCapture.Image = Properties.Resources.play;
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
            Process.Start("mailto:SSASDiagChamps@microsoft.com?subject=Feedback on SSAS Diagnostics Collector Tool&cc=jburchel@microsoft.com");
        }

        private void lkBugs_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("mailto:jon.burchel@microsoft.com?subject=Issue with SSASDiag");
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

        private void SetRolloverAndStartStopEnabledStates()
        {
            chkRollover.Enabled = chkStartTime.Enabled = chkStopTime.Enabled = dtStartTime.Enabled = dtStopTime.Enabled
                = chkGetPerfMon.Checked | chkGetProfiler.Checked | chkGetNetwork.Checked;
            udRollover.Enabled = chkRollover.Enabled & chkRollover.Checked;
            dtStartTime.Enabled = chkStartTime.Enabled & chkStartTime.Checked;
            dtStopTime.Enabled = chkStopTime.Enabled & chkStopTime.Checked;
        }

        private void chkGetPerfMon_CheckedChanged(object sender, EventArgs e)
        {
            lblInterval.Enabled = udInterval.Enabled = lblInterval2.Enabled = chkGetPerfMon.Checked;
            SetRolloverAndStartStopEnabledStates();
        }

        private void chkGetProfiler_CheckedChanged(object sender, EventArgs e)
        {
            chkAutoRestart.Enabled = chkGetProfiler.Checked;
            SetRolloverAndStartStopEnabledStates();
        }

        private void chkGetNetwork_CheckedChanged(object sender, EventArgs e)
        {
            SetRolloverAndStartStopEnabledStates();
        }

        private void chkAutoRestart_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoRestart.Checked && chkAutoRestart.Checked == false)
            {
                ttStatus.Show("Stop time required for your protection with AutoRestart=true.", dtStopTime, 1750);
                chkStopTime.Checked = true;
            }
        }
    }
}
