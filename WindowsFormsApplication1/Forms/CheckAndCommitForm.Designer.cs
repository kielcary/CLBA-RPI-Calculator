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
            this.btnCommit.Location = new System.Drawing.Point(275, 17);
            this.btnCommit.Name = "btnCommit";
            this.btnCommit.Size = new System.Drawing.Size(75, 23);
            this.btnCommit.TabIndex = 2;
            this.btnCommit.Text = "Commit";
            this.btnCommit.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(257, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Please review this data thoroughly before committing.";
            // 
            // CheckAndCommitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(762, 752);
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
    }
}