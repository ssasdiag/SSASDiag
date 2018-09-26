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
using System.Diagnostics;

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
            if (chkHangDumps.Checked) btnHangDumps.Enabled = btnHangDumps.Visible = true;
            btnCapture.Image = imgStop;
            btnCapture.Click += btnCapture_Click;
            if (dc != null)
                dc.CompletionCallback = callback_StopAndFinalizeAllDiagnosticsComplete;
        }
        private void callback_StopAndFinalizeAllDiagnosticsComplete()
        {
            btnHangDumps.Visible = btnHangDumps.Enabled = false;
            txtSaveLocation.Enabled = btnSaveLocation.Enabled = tbAnalysis.Enabled = chkZip.Enabled = chkDeleteRaw.Enabled = grpDiagsToCapture.Enabled = chkStopTime.Enabled = chkAutoRestart.Enabled = chkRollover.Enabled = chkStartTime.Enabled = udInterval.Enabled = cbInstances.Enabled = lblInterval.Enabled = lblInterval2.Enabled = true;
            udRollover.Enabled = chkRollover.Checked;
            dtStartTime.Enabled = chkStartTime.Checked;
            dtStopTime.Enabled = chkStopTime.Checked;
            btnCapture.Image = imgPlay;
            btnCapture.Click += btnCapture_Click;
            txtStatus.Enter -= txtStatus_EnterWhileRunning;
            txtStatus.GotFocus -= txtStatus_GotFocusWhileRunning;
            txtStatus.Cursor = Cursors.Default;
            if (bClosing)
                BeginInvoke(new System.Action(()=>Close()));
            tbAnalysis.ForeColor = SystemColors.ControlText;
            tcCollectionAnalysisTabs.Refresh();

            if(File.Exists(svcOutputPath)) 
                File.Delete(svcOutputPath);

            if (Environment.UserInteractive)
            {
                // Reinitialize service config to start with no command options - immediately terminating hereafter in service mode then, until UI configures new settings, should someone try to start manually.
                string svcIniPath = svcOutputPath.Substring(0, svcOutputPath.IndexOf(".output.log")) + ".ini";
                svcIniPath = AppDomain.CurrentDomain.BaseDirectory + svcIniPath.Substring(svcIniPath.LastIndexOf("\\") + 1);
                List <string> svcconfig = new List<string>();
                if (File.Exists(svcIniPath))
                    svcconfig = new List<string>(File.ReadAllLines(svcIniPath));
                string svcName = svcIniPath.Substring(svcIniPath.LastIndexOf("\\") + 1).Replace(".ini", "");
                svcconfig[svcconfig.FindIndex(s => s.StartsWith("CommandLine="))] = "CommandLine=" + (AppDomain.CurrentDomain.GetData("originalbinlocation") as string) + "\\SSASDiag.exe";
                File.WriteAllLines(svcIniPath, svcconfig.ToArray());
            }
            else
            {
                try
                {
                    ProcessStartInfo p = new ProcessStartInfo("cmd.exe", "/c ping 1.1.1.1 -n 1 -w 2000 > nul & net stop " + svcName);
                    p.WindowStyle = ProcessWindowStyle.Hidden;
                    p.Verb = "runas";
                    p.UseShellExecute = false;
                    p.CreateNoWindow = true;
                    p.RedirectStandardOutput = true;
                    p.RedirectStandardError = true;
                    Process proc = Process.Start(p);
                }
                catch (Exception e)
                { LogException(e); }
                Application.Exit();
            }
            if (bExitAfterStop)
                Invoke(new System.Action(() => Close()));
            Program.SetupDebugTrace();
            LogFeatureUse("Collection " + (Environment.UserInteractive ? "stopped and delivered to client." : "service stopped."));
        }
        #endregion CaptureStartAndStop
    }
}