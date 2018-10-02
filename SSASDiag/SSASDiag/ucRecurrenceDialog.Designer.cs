namespace SSASDiag
{
    partial class ucRecurrenceDialog
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chkRecurringSchedule = new System.Windows.Forms.CheckBox();
            this.chkThursday = new System.Windows.Forms.CheckBox();
            this.chkSunday = new System.Windows.Forms.CheckBox();
            this.chkWednesday = new System.Windows.Forms.CheckBox();
            this.chkMonday = new System.Windows.Forms.CheckBox();
            this.chkFriday = new System.Windows.Forms.CheckBox();
            this.chkTuesday = new System.Windows.Forms.CheckBox();
            this.chkSaturday = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkRecurringSchedule
            // 
            this.chkRecurringSchedule.AutoSize = true;
            this.chkRecurringSchedule.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkRecurringSchedule.Location = new System.Drawing.Point(5, 3);
            this.chkRecurringSchedule.Name = "chkRecurringSchedule";
            this.chkRecurringSchedule.Size = new System.Drawing.Size(267, 17);
            this.chkRecurringSchedule.TabIndex = 1;
            this.chkRecurringSchedule.Text = "Repeat the schedule starting on the following days:";
            this.chkRecurringSchedule.UseVisualStyleBackColor = true;
            this.chkRecurringSchedule.CheckedChanged += new System.EventHandler(this.chkRecurringSchedule_CheckedChanged);
            // 
            // chkThursday
            // 
            this.chkThursday.AutoSize = true;
            this.chkThursday.CheckAlign = System.Drawing.ContentAlignment.TopCenter;
            this.chkThursday.Enabled = false;
            this.chkThursday.Location = new System.Drawing.Point(155, 26);
            this.chkThursday.Name = "chkThursday";
            this.chkThursday.Size = new System.Drawing.Size(33, 31);
            this.chkThursday.TabIndex = 2;
            this.chkThursday.Text = "Thur";
            this.chkThursday.UseVisualStyleBackColor = true;
            // 
            // chkSunday
            // 
            this.chkSunday.AutoSize = true;
            this.chkSunday.CheckAlign = System.Drawing.ContentAlignment.TopCenter;
            this.chkSunday.Enabled = false;
            this.chkSunday.Location = new System.Drawing.Point(14, 26);
            this.chkSunday.Name = "chkSunday";
            this.chkSunday.Size = new System.Drawing.Size(30, 31);
            this.chkSunday.TabIndex = 3;
            this.chkSunday.Text = "Sun";
            this.chkSunday.UseVisualStyleBackColor = true;
            // 
            // chkWednesday
            // 
            this.chkWednesday.AutoSize = true;
            this.chkWednesday.CheckAlign = System.Drawing.ContentAlignment.TopCenter;
            this.chkWednesday.Enabled = false;
            this.chkWednesday.Location = new System.Drawing.Point(119, 26);
            this.chkWednesday.Name = "chkWednesday";
            this.chkWednesday.Size = new System.Drawing.Size(34, 31);
            this.chkWednesday.TabIndex = 4;
            this.chkWednesday.Text = "Wed";
            this.chkWednesday.UseVisualStyleBackColor = true;
            // 
            // chkMonday
            // 
            this.chkMonday.AutoSize = true;
            this.chkMonday.CheckAlign = System.Drawing.ContentAlignment.TopCenter;
            this.chkMonday.Enabled = false;
            this.chkMonday.Location = new System.Drawing.Point(49, 26);
            this.chkMonday.Name = "chkMonday";
            this.chkMonday.Size = new System.Drawing.Size(32, 31);
            this.chkMonday.TabIndex = 5;
            this.chkMonday.Text = "Mon";
            this.chkMonday.UseVisualStyleBackColor = true;
            // 
            // chkFriday
            // 
            this.chkFriday.AutoSize = true;
            this.chkFriday.CheckAlign = System.Drawing.ContentAlignment.TopCenter;
            this.chkFriday.Enabled = false;
            this.chkFriday.Location = new System.Drawing.Point(195, 26);
            this.chkFriday.Name = "chkFriday";
            this.chkFriday.Size = new System.Drawing.Size(22, 31);
            this.chkFriday.TabIndex = 6;
            this.chkFriday.Text = "Fri";
            this.chkFriday.UseVisualStyleBackColor = true;
            // 
            // chkTuesday
            // 
            this.chkTuesday.AutoSize = true;
            this.chkTuesday.CheckAlign = System.Drawing.ContentAlignment.TopCenter;
            this.chkTuesday.Enabled = false;
            this.chkTuesday.Location = new System.Drawing.Point(84, 26);
            this.chkTuesday.Name = "chkTuesday";
            this.chkTuesday.Size = new System.Drawing.Size(35, 31);
            this.chkTuesday.TabIndex = 7;
            this.chkTuesday.Text = "Tues";
            this.chkTuesday.UseVisualStyleBackColor = true;
            // 
            // chkSaturday
            // 
            this.chkSaturday.AutoSize = true;
            this.chkSaturday.CheckAlign = System.Drawing.ContentAlignment.TopCenter;
            this.chkSaturday.Enabled = false;
            this.chkSaturday.Location = new System.Drawing.Point(227, 26);
            this.chkSaturday.Name = "chkSaturday";
            this.chkSaturday.Size = new System.Drawing.Size(27, 31);
            this.chkSaturday.TabIndex = 8;
            this.chkSaturday.Text = "Sat";
            this.chkSaturday.UseVisualStyleBackColor = true;
            // 
            // ucRecurrenceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.chkWednesday);
            this.Controls.Add(this.chkThursday);
            this.Controls.Add(this.chkSaturday);
            this.Controls.Add(this.chkTuesday);
            this.Controls.Add(this.chkFriday);
            this.Controls.Add(this.chkMonday);
            this.Controls.Add(this.chkSunday);
            this.Controls.Add(this.chkRecurringSchedule);
            this.Enabled = false;
            this.Name = "ucRecurrenceDialog";
            this.Size = new System.Drawing.Size(269, 60);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.CheckBox chkRecurringSchedule;
        public System.Windows.Forms.CheckBox chkThursday;
        public System.Windows.Forms.CheckBox chkSunday;
        public System.Windows.Forms.CheckBox chkWednesday;
        public System.Windows.Forms.CheckBox chkMonday;
        public System.Windows.Forms.CheckBox chkFriday;
        public System.Windows.Forms.CheckBox chkTuesday;
        public System.Windows.Forms.CheckBox chkSaturday;
    }
}
