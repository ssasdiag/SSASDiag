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
    public partial class frmAbout : ShadowedForm
    {
        Timer t = new Timer();
        bool bOpening = true, bDecelerating = false;

        public frmAbout()
        {
            InitializeComponent();
        }

        private void frmAbout_Shown(object sender, EventArgs e)
        {
            t.Interval = 20;
            t.Tick += T_Tick;
            t.Start();
        }

        private void T_Tick(object sender, EventArgs e)
        {
            if (bOpening)
            {
                if (Opacity >= .75)
                {
                    t.Interval = 40;
                    bOpening = false;
                    bDecelerating = true;
                }
                Opacity += .05;
            }
            else if (bDecelerating)
            {
                if (Opacity >= 1)
                {
                    t.Interval = 2700;
                    bDecelerating = false;
                }
                Opacity += .01;
            }
            else
            {
                t.Interval = 10;
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
