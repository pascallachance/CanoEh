namespace BaseClient
{
    partial class frmStaffList
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
            txtStaffList = new TextBox();
            btnRefresh = new Button();
            SuspendLayout();
            // 
            // txtStaffList
            // 
            txtStaffList.Location = new Point(12, 12);
            txtStaffList.Multiline = true;
            txtStaffList.Name = "txtStaffList";
            txtStaffList.Size = new Size(776, 371);
            txtStaffList.TabIndex = 0;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(354, 397);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(112, 34);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // frmStaffList
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnRefresh);
            Controls.Add(txtStaffList);
            Name = "frmStaffList";
            Text = "Staff List";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtStaffList;
        private Button btnRefresh;
    }
}