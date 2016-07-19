namespace WindowsFormsApplication1
{
    partial class CLBRPIForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CLBRPIForm));
            this.TeamsGridView = new System.Windows.Forms.DataGridView();
            this.btnRPI = new System.Windows.Forms.Button();
            this.btnSoS = new System.Windows.Forms.Button();
            this.btnAll = new System.Windows.Forms.Button();
            this.btnPyth = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.TeamsGridView)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TeamsGridView
            // 
            this.TeamsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.TeamsGridView.Location = new System.Drawing.Point(12, 58);
            this.TeamsGridView.Name = "TeamsGridView";
            this.TeamsGridView.Size = new System.Drawing.Size(970, 700);
            this.TeamsGridView.TabIndex = 0;
            // 
            // btnRPI
            // 
            this.btnRPI.Location = new System.Drawing.Point(102, 6);
            this.btnRPI.Name = "btnRPI";
            this.btnRPI.Size = new System.Drawing.Size(75, 23);
            this.btnRPI.TabIndex = 1;
            this.btnRPI.Text = "RPI";
            this.btnRPI.UseVisualStyleBackColor = true;
            this.btnRPI.Click += new System.EventHandler(this.btnRPI_Click);
            // 
            // btnSoS
            // 
            this.btnSoS.Location = new System.Drawing.Point(183, 6);
            this.btnSoS.Name = "btnSoS";
            this.btnSoS.Size = new System.Drawing.Size(75, 23);
            this.btnSoS.TabIndex = 2;
            this.btnSoS.Text = "SoS";
            this.btnSoS.UseVisualStyleBackColor = true;
            this.btnSoS.Click += new System.EventHandler(this.btnSoS_Click);
            // 
            // btnAll
            // 
            this.btnAll.Location = new System.Drawing.Point(21, 6);
            this.btnAll.Name = "btnAll";
            this.btnAll.Size = new System.Drawing.Size(75, 23);
            this.btnAll.TabIndex = 3;
            this.btnAll.Text = "All";
            this.btnAll.UseVisualStyleBackColor = true;
            this.btnAll.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // btnPyth
            // 
            this.btnPyth.Location = new System.Drawing.Point(264, 6);
            this.btnPyth.Name = "btnPyth";
            this.btnPyth.Size = new System.Drawing.Size(75, 23);
            this.btnPyth.TabIndex = 4;
            this.btnPyth.Text = "Pyth";
            this.btnPyth.UseVisualStyleBackColor = true;
            this.btnPyth.Click += new System.EventHandler(this.btnPyth_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(427, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(555, 48);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Legend";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(146, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(139, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "SoS = Strength of Schedule";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(146, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(197, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "OOWP = Opponent\'s Opponent\'s Win %";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "OWP = Opponent\'s Win %";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "WP = Win Percentage";
            // 
            // CLBRPIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(994, 773);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnPyth);
            this.Controls.Add(this.btnAll);
            this.Controls.Add(this.btnSoS);
            this.Controls.Add(this.btnRPI);
            this.Controls.Add(this.TeamsGridView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CLBRPIForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CLB Rankings Calculator";
            ((System.ComponentModel.ISupportInitialize)(this.TeamsGridView)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView TeamsGridView;
        private System.Windows.Forms.Button btnRPI;
        private System.Windows.Forms.Button btnSoS;
        private System.Windows.Forms.Button btnAll;
        private System.Windows.Forms.Button btnPyth;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
    }
}

