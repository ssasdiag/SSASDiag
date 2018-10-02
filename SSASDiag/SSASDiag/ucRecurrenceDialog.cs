using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSASDiag
{
    public partial class ucRecurrenceDialog : UserControl
    {
        public ucRecurrenceDialog()
        {
            InitializeComponent();
        }

        private void chkRecurringSchedule_CheckedChanged(object sender, EventArgs e)
        {
            chkSunday.Enabled = chkMonday.Enabled = chkTuesday.Enabled = chkWednesday.Enabled = chkThursday.Enabled = chkFriday.Enabled = chkSaturday.Enabled = chkRecurringSchedule.Checked;
            Program.MainForm.btnSchedule.Image = chkRecurringSchedule.Checked ? Properties.Resources.RecurrenceEnabled : Properties.Resources.RecurrenceDisabled;
        }
    }
}
