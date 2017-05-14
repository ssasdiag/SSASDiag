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
        #region CaptureStartAndStop
        #region StatusHandlingDuringCapture
        // Minor functions used only while running diagnostic
        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);
        private void txtStatus_GotFocusWhileRunning(object sender, EventArgs e)
        {
            HideCaret(txtStatus.Handle);
        }
        private void txtStatus_EnterWhileRunning(object sender, EventArgs e)
        {
            ActiveControl = btnCapture;
        }
        #endregion StatusHandlingDuringCapture
        private void callback_StartDiagnosticsComplete()
        {
            btnCapture.Image = imgStop;
            btnCapture.Click += btnCapture_Click;
            dc.CompletionCallback = callback_StopAndFinalizeAllDiagnosticsComplete;
        }
        private void callback_StopAndFinalizeAllDiagnosticsComplete()
        {
            chkRunAsService.Enabled = txtSaveLocation.Enabled = btnSaveLocation.Enabled = tbAnalysis.Enabled = chkZip.Enabled = chkDeleteRaw.Enabled = grpDiagsToCapture.Enabled = chkStopTime.Enabled = chkAutoRestart.Enabled = chkRollover.Enabled = chkStartTime.Enabled = udInterval.Enabled = cbInstances.Enabled = lblInterval.Enabled = lblInterval2.Enabled = true;
            udRollover.Enabled = chkRollover.Checked;
            dtStartTime.Enabled = chkStartTime.Checked;
            dtStopTime.Enabled = chkStopTime.Checked;
            btnCapture.Image = imgPlay;
            btnCapture.Click += btnCapture_Click;
            txtStatus.Enter -= txtStatus_EnterWhileRunning;
            txtStatus.GotFocus -= txtStatus_GotFocusWhileRunning;
            txtStatus.Cursor = Cursors.Default;
            if (bClosing)
                Close();
            tbAnalysis.ForeColor = SystemColors.ControlText;
            tcCollectionAnalysisTabs.Refresh();
            dc.CompletionCallback = null;
            LogFeatureUse("Collection Stopped", "");
            if (!Environment.UserInteractive)
            {
                // Reinitialize service config to start with no command options - immediately terminating hereafter in service mode then, until UI configures new settings, should someone try to start manually.
                List<string> svcconfig = new List<string>(File.ReadAllLines(Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\SSASDiagService.ini"));
                svcconfig[svcconfig.FindIndex(s => s.StartsWith("CommandLine="))] = "CommandLine=" + (AppDomain.CurrentDomain.GetData("originalbinlocation") as string) + "\\SSASDiag.exe";
                File.WriteAllLines(Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\SSASDiagService.ini", svcconfig.ToArray());

                ProcessStartInfo p = new ProcessStartInfo("cmd.exe", "/c ping 1.1.1.1 -n 1 -w 1500 > nul & net stop SSASDiagService");
                p.WindowStyle = ProcessWindowStyle.Hidden;
                p.UseShellExecute = false;
                p.CreateNoWindow = true;
                Process.Start(p);
                Application.Exit();
            }
        }
        #endregion CaptureStartAndStop
    }
}