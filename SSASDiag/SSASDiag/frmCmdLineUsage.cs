using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSASDiag
{
    public partial class frmCmdLineUsage : Form  // Use Form instead if you need to open in designer, then revert to Shadowed for the cool effect in use...  :)
    {
        public frmCmdLineUsage()
        {
            InitializeComponent();
        }

        private void lkSSASDiagSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/ssasdiag/SSASDiag/wiki/SSAS-Diagnostics-Documentation-Home");
        }
    }
}
