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

namespace SSASDiag
{
    public partial class frmSSASDiag : Form
    {
        #region DiagnosticsLocals
        DateTime dtLastScrollTime = DateTime.Now;
        System.Windows.Forms.Timer tmScrollStart = new System.Windows.Forms.Timer();
        #endregion DiagnosticsLocals

        #region SimpleDiagnosticsUI
        private void cmbProblemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkGetConfigDetails.Checked = chkGetNetwork.Checked = chkGetPerfMon.Checked = chkGetProfiler.Checked = chkProfilerPerfDetails.Checked = false;

            switch (cmbProblemType.SelectedItem as string)
            {
                case "Performance":
                    rtbProblemDescription.Height = 170;
                    rtbProblemDescription.Text = "Performance issues require minimal collection of config details, performance monitor logs, and extended profiler traces including performance relevant details.\r\n\r\n"
                                           + "Including AS backups can allow further investigation to review data structures, rerun problematic queries, or test changes to calculations.\r\n\r\n"
                                           + "Including SQL data source backups can further allow experimental changes and full reprocessing of data structures.";
                    chkProfilerPerfDetails.Checked = chkGetProfiler.Checked = chkGetPerfMon.Checked = chkXMLA.Checked = chkGetConfigDetails.Checked = true;
                    break;
                case "Errors/Hangs (non-connectivity)":
                    rtbProblemDescription.Height = 170;
                    rtbProblemDescription.Text = "Non-connectivity related errors require minimal collection of config details, performance monitor logs, and basic profiler traces.\r\n\r\n"
                                           + "Including AS backups can allow further investigation to review data structures, rerun problematic queries, or test changes to calculations.\r\n\r\n"
                                           + "Including SQL data source backups can further allow experimental changes and full reprocessing of data structures.";
                    chkGetPerfMon.Checked = chkGetProfiler.Checked = chkGetConfigDetails.Checked = true;
                    break;
                case "Connectivity Failures":
                    rtbProblemDescription.Height = 287;
                    rtbProblemDescription.Text = "Connectivity failures require minimal collection of config details, performance monitor logs, basic profiler traces, and network traces.\r\n\r\n"
                                           + "Network traces should be captured on a failing client, and any middle tier server, for multi-tier scenarios.\r\n\r\n"
                                           + "Service Principle Names registered in Active Directory are captured if the tool is run as a domain administrator.\r\n\r\n"
                                           + "Including AS backups can allow further investigation to review data structures, rerun problematic queries, or test changes to calculations.\r\n\r\n"
                                           + "Including SQL data source backups can further allow experimental changes and full reprocessing of data structures.";
                    chkGetConfigDetails.Checked = chkGetProfiler.Checked = chkGetPerfMon.Checked = chkGetNetwork.Checked = true;
                    break;
                case "Connectivity (client/middle-tier only)":
                    rtbProblemDescription.Height = 130;
                    rtbProblemDescription.Text = "Connectivity failures on client and middle tier require collection of network traces.\r\n\r\n"
                                           + "Network traces should be captured on a failing client, and any middle tier server, for multi-tier scenarios.\r\n\r\n"
                                           + "Service Principle Names registered in Active Directory are captured if the tool is run as a domain administrator.";
                    chkXMLA.Checked = chkABF.Checked = chkBAK.Checked = false;
                    chkGetNetwork.Checked = true;
                    break;
                case "Incorrect Query Results":
                    rtbProblemDescription.Height = 170;
                    rtbProblemDescription.Text = "Incorrect results require minimal collection of config details and basic profiler traces, as well as full SQL data source backups.\r\n\r\n"
                                           + "Including AS backups can allow further investigation to review data structures, rerun problematic queries, or test changes to calculations.\r\n\r\n"
                                           + "Including SQL data source backups allows all experimental changes and full reprocessing of data structures.";
                    chkGetConfigDetails.Checked = chkGetProfiler.Checked = true;
                    tbLevelOfData.Value = 2;
                    ttStatus.Show("Including SQL data source backups can increase data collection size and time required to stop collection.", tbLevelOfData, 1500);
                    break;
                case "Data Corruption":
                    rtbProblemDescription.Height = 210;
                    rtbProblemDescription.Text = "Data corruption issues require minimal collection of config details (including Application and System Event logs), performance monitor logs, basic profiler traces, and AS backups.\r\n\r\n"
                                           + "Including AS backups allows investigation to review corrupt data, in some cases allowing partial or full recovery.\r\n\r\n"
                                           + "Including SQL data source backups can further allow experimental changes and full reprocessing of data structures.";
                    chkGetConfigDetails.Checked = chkGetProfiler.Checked = chkGetPerfMon.Checked = true;
                    if (tbLevelOfData.Value != 1)
                    {
                        tbLevelOfData.Value = 1;
                        ProcessSliderMiddlePosition();
                    }
                    ttStatus.Show("Including AS backups can increase data collection size and time required to stop collection.", tbLevelOfData, 1500);
                    break;
            }
        }
        private void tbLevelOfData_ValueChanged(object sender, EventArgs e)
        {
            chkABF.Checked = chkBAK.Checked = chkXMLA.Checked = false;
            lblBAK.ForeColor = lblABF.ForeColor = lblXMLA.ForeColor = SystemColors.ControlDark;
            switch (tbLevelOfData.Value)
            {
                case 0:
                    lblXMLA.ForeColor = SystemColors.ControlText;
                    chkXMLA.Checked = true;
                    ttStatus.SetToolTip(tbLevelOfData, null);
                    break;
                case 1:
                    lblABF.ForeColor = Color.Red;
                    chkABF.Checked = true;
                    ttStatus.SetToolTip(tbLevelOfData, "Including AS backups can increase data collection size and time required to stop collection.");
                    break;
                case 2:
                    lblBAK.ForeColor = Color.Red;
                    chkXMLA.Checked = true;
                    chkBAK.Checked = true;
                    ttStatus.SetToolTip(tbLevelOfData, "Including SQL data source backups can increase data collection size and time required to stop collection.");
                    break;
            }
        }
        private void tbLevelOfData_Scroll(object sender, EventArgs e)
        {
            dtLastScrollTime = DateTime.Now;
            tmScrollStart.Start();
        }
        private void tmLevelOfDataScroll_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = DateTime.Now - dtLastScrollTime;
            if (ts.TotalMilliseconds > 250 && tbLevelOfData.Value == 1)
            {
                tmScrollStart.Stop();
                ProcessSliderMiddlePosition();
            }
        }
        private void ProcessSliderMiddlePosition()
        {
            if (chkABF.Checked)
            {
                tmScrollStart.Stop();
                chkGetProfiler.Checked = true;
                string baseMsg = "AS .abf backups provide data to execute queries and obtain results, and allow modification of calculation definitions, but not changes "
                                + "to data definitions requiring reprocessing.  They are the second most optimal dataset to reproduce and investigate issues.\r\n\r\n"
                                + "However, please note that including database or data source backups may siginificantly increase size of data collected and time required to stop collection.";
                if (chkXMLA.Checked)
                    MessageBox.Show("AS backups include database definitions.\nDatabase definitions will be unchecked after you click OK.\r\n\r\n"
                                    + baseMsg,
                                  "Backup Collection Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    MessageBox.Show(baseMsg, "Backup Collection Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                chkXMLA.Checked = false;
            }
        }
        #endregion SimpleDiagnosticsUI

        #region AdvandedDiagnosticsUI
        private void chkGetConfigDetails_CheckedChanged(object sender, EventArgs e)
        {
            EnsureSomethingToCapture();
            UpdateUIIfOnlyNetworkingEnabled();
        }
        private void chkGetPerfMon_CheckedChanged(object sender, EventArgs e)
        {
            lblInterval.Enabled = udInterval.Enabled = lblInterval2.Enabled = chkGetPerfMon.Checked;
            SetRolloverAndStartStopEnabledStates();
            EnsureSomethingToCapture();
            UpdateUIIfOnlyNetworkingEnabled();
        }
        private void chkProfilerPerfDetails_CheckedChanged(object sender, EventArgs e)
        {
            if (chkProfilerPerfDetails.Checked)
            {
                chkGetProfiler.Checked = true;
                if (MessageBox.Show("Adding verbose performance details to profiler traces accumulates data much more quickly than without, and is often not required even to understand many performance issues.\r\n\r\nDo you want to enable verbose tracing anyway?", "Verbose Trace Details Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.No)
                    chkProfilerPerfDetails.Checked = false;
            }
            UpdateUIIfOnlyNetworkingEnabled();
        }
        private void chkXMLA_CheckedChanged(object sender, EventArgs e)
        {
            if (chkXMLA.Checked)
            {
                if (chkABF.Checked)
                    chkABF.Checked = false;
                chkGetProfiler.Checked = true;
            }
            else
                chkBAK.Checked = false;
            UpdateUIIfOnlyNetworkingEnabled();
        }
        private void chkABF_CheckedChanged(object sender, EventArgs e)
        {
            if (chkABF.Checked)
            {
                chkGetProfiler.Checked = true;
                if (tcSimpleAdvanced.SelectedIndex == 1)
                {
                    string baseMsg = "AS .abf backups provide data to execute queries and obtain results, and allow modification of calculation definitions, but not changes "
                                + "to data definitions requiring reprocessing.  They are the second most optimal dataset to reproduce and investigate issues.\r\n\r\n"
                                + "However, please note that including database or data source backups may siginificantly increase size of data collected and time required to stop collection.";
                    if (chkXMLA.Checked)
                        MessageBox.Show("AS backups include database definitions.\nDatabase definitions will be unchecked after you click OK.\r\n\r\n"
                                        + baseMsg,
                                      "Backup Collection Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else
                        MessageBox.Show(baseMsg, "Backup Collection Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                chkXMLA.Checked = false;
                chkBAK.Checked = false;
            }
            UpdateUIIfOnlyNetworkingEnabled();
        }
        private void chkBAK_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBAK.Checked)
            {
                chkGetProfiler.Checked = chkXMLA.Checked = true;
                MessageBox.Show("AS database definitions with SQL data source backups provide the optimal dataset to reproduce and investigate any issue.\r\n"
                    + "\r\nHowever, please note that including database or data source backups may significantly increase size of data collected and time required to stop collection.",
                    "Backup Collection Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            UpdateUIIfOnlyNetworkingEnabled();
        }
        private void chkGetProfiler_CheckedChanged(object sender, EventArgs e)
        {
            chkAutoRestart.Enabled = chkGetProfiler.Checked;
            SetRolloverAndStartStopEnabledStates();
            if (!chkGetProfiler.Checked)
            {
                chkProfilerPerfDetails.Checked = false;
                chkABF.Checked = false;
                chkBAK.Checked = false;
                chkXMLA.Checked = false;
            }
            UpdateUIIfOnlyNetworkingEnabled();
            EnsureSomethingToCapture();
        }
        private void chkGetNetwork_CheckedChanged(object sender, EventArgs e)
        {
            SetRolloverAndStartStopEnabledStates();
            if (chkGetNetwork.Checked && chkRollover.Checked)
                ttStatus.Show("NOTE: Network traces rollover circularly,\n"
                            + "always deleting older data automatically.", chkGetNetwork, 2000);
            if (chkGetNetwork.Checked)
                MessageBox.Show("Please note that including network traces may significantly increase size of data collected and time required to stop collection.",
                    "Network Trace Collection Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            UpdateUIIfOnlyNetworkingEnabled();
            EnsureSomethingToCapture();
        }
        private void tcCollectionAnalysisTabs_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (!tbAnalysis.Enabled)
                e.Cancel = true;
        }
        private void UpdateUIIfOnlyNetworkingEnabled()
        {
            if (chkGetNetwork.Checked && !chkGetProfiler.Checked && !chkGetPerfMon.Checked && !chkGetConfigDetails.Checked && !chkXMLA.Checked && !chkABF.Checked && !chkBAK.Checked)
            {
                lblLevelOfReproData.ForeColor = lblABF.ForeColor = lblBAK.ForeColor = lblXMLA.ForeColor = SystemColors.ControlDark;
                cbInstances.Enabled = tbLevelOfData.Enabled = false;
            }
            else
            {
                lblXMLA.ForeColor = tbLevelOfData.Value == 0 ? SystemColors.ControlText : SystemColors.ControlDarkDark;
                lblABF.ForeColor = tbLevelOfData.Value == 1 ? Color.Red : SystemColors.ControlDarkDark;
                lblBAK.ForeColor = tbLevelOfData.Value == 2 ? Color.Red : SystemColors.ControlDarkDark;
                lblLevelOfReproData.ForeColor = SystemColors.ControlText;
                tbLevelOfData.Enabled = true;
                if (cbInstances.Items.Count > 0) cbInstances.Enabled = true;
            }
        }
        private void EnsureSomethingToCapture()
        {
            btnCapture.Enabled = false;
            if (chkGetConfigDetails.Checked || chkGetPerfMon.Checked || chkGetProfiler.Checked)
            {
                btnCapture.Enabled = true;
                cbInstances_SelectedIndexChanged(null, null);
            }
            else
                if (chkGetNetwork.Checked)
                btnCapture.Enabled = true;
        }

        #endregion AdvandedDiagnosticsUI
    }
}