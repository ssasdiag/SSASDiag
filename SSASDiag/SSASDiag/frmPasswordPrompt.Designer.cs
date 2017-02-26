namespace SSASDiag
{
    partial class frmPasswordPrompt
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPasswordPrompt));
            lblUser = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            txtUser = new System.Windows.Forms.TextBox();
            txtPwd = new System.Windows.Forms.TextBox();
            btnCancel = new System.Windows.Forms.Button();
            txtStatus = new System.Windows.Forms.TextBox();
            btnOK = new System.Windows.Forms.Button();
            lblUserPasswordError = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // lblUser
            // 
            lblUser.AutoSize = true;
            lblUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblUser.Location = new System.Drawing.Point(88, 138);
            lblUser.Name = "lblUser";
            lblUser.Size = new System.Drawing.Size(108, 13);
            lblUser.TabIndex = 1;
            lblUser.Text = "Domain\\Username";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label1.Location = new System.Drawing.Point(112, 180);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(61, 13);
            label1.TabIndex = 2;
            label1.Text = "Password";
            // 
            // txtUser
            // 
            txtUser.Location = new System.Drawing.Point(12, 156);
            txtUser.Name = "txtUser";
            txtUser.Size = new System.Drawing.Size(260, 20);
            txtUser.TabIndex = 0;
            txtUser.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            txtUser.TextChanged += new System.EventHandler(txtUser_TextChanged);
            // 
            // txtPwd
            // 
            txtPwd.Location = new System.Drawing.Point(12, 196);
            txtPwd.Name = "txtPwd";
            txtPwd.Size = new System.Drawing.Size(260, 20);
            txtPwd.TabIndex = 1;
            txtPwd.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            txtPwd.UseSystemPasswordChar = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(196, 244);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(76, 23);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += new System.EventHandler(btnCancel_Click);
            // 
            // txtStatus
            // 
            txtStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtStatus.Location = new System.Drawing.Point(12, 12);
            txtStatus.Multiline = true;
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.Size = new System.Drawing.Size(260, 117);
            txtStatus.TabIndex = 6;
            txtStatus.TabStop = false;
            txtStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnOK
            // 
            btnOK.Enabled = false;
            btnOK.Location = new System.Drawing.Point(114, 244);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(76, 23);
            btnOK.TabIndex = 2;
            btnOK.Text = "&OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += new System.EventHandler(btnOK_Click);
            // 
            // lblUserPasswordError
            // 
            lblUserPasswordError.AutoSize = true;
            lblUserPasswordError.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblUserPasswordError.ForeColor = System.Drawing.Color.DarkRed;
            lblUserPasswordError.Location = new System.Drawing.Point(71, 219);
            lblUserPasswordError.Name = "lblUserPasswordError";
            lblUserPasswordError.Size = new System.Drawing.Size(143, 12);
            lblUserPasswordError.TabIndex = 7;
            lblUserPasswordError.Text = "Incorrect user name or password.";
            lblUserPasswordError.Visible = false;
            // 
            // frmPasswordPrompt
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(284, 279);
            Controls.Add(lblUserPasswordError);
            Controls.Add(btnOK);
            Controls.Add(txtStatus);
            Controls.Add(btnCancel);
            Controls.Add(txtPwd);
            Controls.Add(txtUser);
            Controls.Add(label1);
            Controls.Add(lblUser);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$Icon")));
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmPasswordPrompt";
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            Text = "Remote Credentials Required";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Button btnOK;
        public System.Windows.Forms.Label lblUserPasswordError;
        public System.Windows.Forms.TextBox txtPwd;
    }
}