namespace RE2REmakeSRT
{
    partial class MainUI
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
            this.playerHealthStatus = new System.Windows.Forms.PictureBox();
            this.statisticsPanel = new DoubleBuffered.DoubleBufferedPanel();
            this.inventoryPanel = new DoubleBuffered.DoubleBufferedPanel();
            ((System.ComponentModel.ISupportInitialize)(this.playerHealthStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // playerHealthStatus
            // 
            this.playerHealthStatus.Image = global::RE2REmakeSRT.Properties.Resources.EMPTY;
            this.playerHealthStatus.InitialImage = global::RE2REmakeSRT.Properties.Resources.EMPTY;
            this.playerHealthStatus.Location = new System.Drawing.Point(1, 1);
            this.playerHealthStatus.Name = "playerHealthStatus";
            this.playerHealthStatus.Size = new System.Drawing.Size(150, 60);
            this.playerHealthStatus.TabIndex = 0;
            this.playerHealthStatus.TabStop = false;
            this.playerHealthStatus.Paint += new System.Windows.Forms.PaintEventHandler(this.playerHealthStatus_Paint);
            // 
            // statisticsPanel
            // 
            this.statisticsPanel.Location = new System.Drawing.Point(1, 67);
            this.statisticsPanel.Name = "statisticsPanel";
            this.statisticsPanel.Size = new System.Drawing.Size(150, 494);
            this.statisticsPanel.TabIndex = 2;
            this.statisticsPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.statisticsPanel_Paint);
            // 
            // inventoryPanel
            // 
            this.inventoryPanel.Location = new System.Drawing.Point(157, 1);
            this.inventoryPanel.Name = "inventoryPanel";
            this.inventoryPanel.Size = new System.Drawing.Size(448, 560);
            this.inventoryPanel.TabIndex = 3;
            this.inventoryPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.inventoryPanel_Paint);
            // 
            // MainUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(607, 563);
            this.Controls.Add(this.inventoryPanel);
            this.Controls.Add(this.statisticsPanel);
            this.Controls.Add(this.playerHealthStatus);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainUI";
            this.ShowIcon = false;
            this.Text = "RE2 (2019) SRT";
            ((System.ComponentModel.ISupportInitialize)(this.playerHealthStatus)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox playerHealthStatus;
        private DoubleBuffered.DoubleBufferedPanel statisticsPanel;
        private DoubleBuffered.DoubleBufferedPanel inventoryPanel;
    }
}