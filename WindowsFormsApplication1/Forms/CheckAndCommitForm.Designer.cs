namespace WindowsFormsApplication1
{
    partial class CheckAndCommitForm
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
            this.TeamsGridView = new System.Windows.Forms.DataGridView();
            this.btnCommit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpGameDate = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.bntClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.TeamsGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // TeamsGridView
            // 
            this.TeamsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.TeamsGridView.Location = new System.Drawing.Point(12, 46);
            this.TeamsGridView.Name = "TeamsGridView";
            this.TeamsGridView.Size = new System.Drawing.Size(742, 687);
            this.TeamsGridView.TabIndex = 1;
            // 
            // btnCommit
            // 
            this.btnCommit.Enabled = false;
            this.btnCommit.Location = new System.Drawing.Point(589, 12);
            this.btnCommit.Name = "btnCommit";
            this.btnCommit.Size = new System.Drawing.Size(75, 23);
            this.btnCommit.TabIndex = 2;
            this.btnCommit.Text = "Commit";
            this.btnCommit.UseVisualStyleBackColor = true;
            this.btnCommit.Click += new System.EventHandler(this.btnCommit_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(330, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(257, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Please review this data thoroughly before committing.";
            // 
            // dtpGameDate
            // 
            this.dtpGameDate.Location = new System.Drawing.Point(115, 12);
            this.dtpGameDate.Name = "dtpGameDate";
            this.dtpGameDate.Size = new System.Drawing.Size(200, 20);
            this.dtpGameDate.TabIndex = 4;
            this.dtpGameDate.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dtpGameDate_MouseDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(47, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Game Date:";
            // 
            // bntClose
            // 
            this.bntClose.Location = new System.Drawing.Point(670, 12);
            this.bntClose.Name = "bntClose";
            this.bntClose.Size = new System.Drawing.Size(75, 23);
            this.bntClose.TabIndex = 6;
            this.bntClose.Text = "Skip Commit";
            this.bntClose.UseVisualStyleBackColor = true;
            this.bntClose.Click += new System.EventHandler(this.bntClose_Click);
            // 
            // CheckAndCommitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(762, 752);
            this.Controls.Add(this.bntClose);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dtpGameDate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCommit);
            this.Controls.Add(this.TeamsGridView);
            this.Name = "CheckAndCommitForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CheckAndCommitForm";
            ((System.ComponentModel.ISupportInitialize)(this.TeamsGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView TeamsGridView;
        private System.Windows.Forms.Button btnCommit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpGameDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button bntClose;
    }
}