﻿using System;
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
    public partial class PasswordPrompt : Form
    {
        public string Password
        {
            get { return txtPwd.Text; }
        }

        public string User
        {
            get { return txtUser.Text.Substring(txtUser.Text.IndexOf("\\") + 1, txtUser.Text.Length - txtUser.Text.IndexOf("\\") - 1); }
        }

        public string Domain
        {
            get { return txtUser.Text.Substring(0, txtUser.Text.IndexOf("\\")); }
        }

        public string UserMessage
        {
            set { txtStatus.Text = value; }
        }

        public PasswordPrompt()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void txtUser_TextChanged(object sender, EventArgs e)
        {
            string u = txtUser.Text.Trim().TrimStart(' ');
            if (u.Contains("\\") && !u.StartsWith("\\") && !u.EndsWith("\\"))
                btnOK.Enabled = true;
            else
                btnOK.Enabled = false;
        }
    }
}
