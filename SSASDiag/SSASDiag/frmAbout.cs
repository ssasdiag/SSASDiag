using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSASDiag
{
    public partial class frmAbout : Form
    {
        Timer t = new Timer();
        bool bOpening = true;

        public frmAbout()
        {
            InitializeComponent();
        }

        private void frmAbout_Shown(object sender, EventArgs e)
        {
            t.Interval = 1;
            t.Tick += T_Tick;
            t.Start();
        }

        private void T_Tick(object sender, EventArgs e)
        {
            if (bOpening)
            {
                Opacity += .2;
                if (Opacity >= 1)
                {
                    t.Interval = 3000;
                    bOpening = false;
                }
            }
            else
            {
                t.Interval = 6;
                Opacity -= .02;
                if (Opacity == 0)
                {
                    t.Stop();
                    Close();
                }
            }
        }
    }
}
