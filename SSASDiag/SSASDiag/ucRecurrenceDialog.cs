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
            Program.MainForm.pnlRecurrence.BackgroundImage = chkRecurringSchedule.Checked ? Properties.Resources.RecurrenceEnabled : Properties.Resources.RecurrenceDisabled;
            UpdateDaysLabel();
        }

        public void UpdateDaysLabel()
        {
            Program.MainForm.lblRecurrenceDays.Text = chkRecurringSchedule.Checked ?
                (chkSunday.Checked ? "S" : "") +
                (chkMonday.Checked ? "M" : "") +
                (chkTuesday.Checked ? "T" : "") +
                (chkWednesday.Checked ? "W" : "") +
                (chkThursday.Checked ? "Th" : "") +
                (chkFriday.Checked ? "F" : "") +
                (chkSaturday.Checked ? "Sa" : "")
                : "";
            Program.MainForm.pnlRecurrence.Width = Program.MainForm.lblRecurrenceDays.Width < 37 ? 37 : Program.MainForm.lblRecurrenceDays.Width;
            Program.MainForm.lblRecurrenceDays.Left = Program.MainForm.pnlRecurrence.Width / 2 - Program.MainForm.lblRecurrenceDays.Width / 2 + 1;
            if (Program.MainForm.lblRecurrenceDays.Width < 37) Program.MainForm.Width = 37;
            if (Program.MainForm.lblRecurrenceDays.Text != "") Program.MainForm.lblRecurrenceDays.Visible = true;
        }

        private void chkDays_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDaysLabel();
        }
    }
}
