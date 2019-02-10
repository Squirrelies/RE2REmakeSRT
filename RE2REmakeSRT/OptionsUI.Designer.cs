namespace RE2REmakeSRT
{
    partial class OptionsUI
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
            this.debugCheckBox = new System.Windows.Forms.CheckBox();
            this.skipChecksumCheckBox = new System.Windows.Forms.CheckBox();
            this.noTitlebarCheckBox = new System.Windows.Forms.CheckBox();
            this.alwaysOnTopCheckBox = new System.Windows.Forms.CheckBox();
            this.transparentBackgroundCheckBox = new System.Windows.Forms.CheckBox();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.scalingFactorNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.cancelButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.optionsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scalingFactorNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // debugCheckBox
            // 
            this.debugCheckBox.AutoSize = true;
            this.debugCheckBox.Location = new System.Drawing.Point(6, 122);
            this.debugCheckBox.Name = "debugCheckBox";
            this.debugCheckBox.Size = new System.Drawing.Size(88, 17);
            this.debugCheckBox.TabIndex = 0;
            this.debugCheckBox.Text = "Debug Mode";
            this.debugCheckBox.UseVisualStyleBackColor = true;
            // 
            // skipChecksumCheckBox
            // 
            this.skipChecksumCheckBox.AutoSize = true;
            this.skipChecksumCheckBox.Location = new System.Drawing.Point(6, 99);
            this.skipChecksumCheckBox.Name = "skipChecksumCheckBox";
            this.skipChecksumCheckBox.Size = new System.Drawing.Size(148, 17);
            this.skipChecksumCheckBox.TabIndex = 1;
            this.skipChecksumCheckBox.Text = "Skip Checksum Checking";
            this.skipChecksumCheckBox.UseVisualStyleBackColor = true;
            // 
            // noTitlebarCheckBox
            // 
            this.noTitlebarCheckBox.AutoSize = true;
            this.noTitlebarCheckBox.Location = new System.Drawing.Point(6, 53);
            this.noTitlebarCheckBox.Name = "noTitlebarCheckBox";
            this.noTitlebarCheckBox.Size = new System.Drawing.Size(78, 17);
            this.noTitlebarCheckBox.TabIndex = 2;
            this.noTitlebarCheckBox.Text = "No Titlebar";
            this.noTitlebarCheckBox.UseVisualStyleBackColor = true;
            // 
            // alwaysOnTopCheckBox
            // 
            this.alwaysOnTopCheckBox.AutoSize = true;
            this.alwaysOnTopCheckBox.Location = new System.Drawing.Point(6, 30);
            this.alwaysOnTopCheckBox.Name = "alwaysOnTopCheckBox";
            this.alwaysOnTopCheckBox.Size = new System.Drawing.Size(96, 17);
            this.alwaysOnTopCheckBox.TabIndex = 3;
            this.alwaysOnTopCheckBox.Text = "Always on Top";
            this.alwaysOnTopCheckBox.UseVisualStyleBackColor = true;
            // 
            // transparentBackgroundCheckBox
            // 
            this.transparentBackgroundCheckBox.AutoSize = true;
            this.transparentBackgroundCheckBox.Location = new System.Drawing.Point(6, 76);
            this.transparentBackgroundCheckBox.Name = "transparentBackgroundCheckBox";
            this.transparentBackgroundCheckBox.Size = new System.Drawing.Size(144, 17);
            this.transparentBackgroundCheckBox.TabIndex = 4;
            this.transparentBackgroundCheckBox.Text = "Transparent Background";
            this.transparentBackgroundCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionsGroupBox
            // 
            this.optionsGroupBox.Controls.Add(this.label1);
            this.optionsGroupBox.Controls.Add(this.scalingFactorNumericUpDown);
            this.optionsGroupBox.Controls.Add(this.skipChecksumCheckBox);
            this.optionsGroupBox.Controls.Add(this.debugCheckBox);
            this.optionsGroupBox.Controls.Add(this.transparentBackgroundCheckBox);
            this.optionsGroupBox.Controls.Add(this.noTitlebarCheckBox);
            this.optionsGroupBox.Controls.Add(this.alwaysOnTopCheckBox);
            this.optionsGroupBox.Location = new System.Drawing.Point(2, 2);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(169, 174);
            this.optionsGroupBox.TabIndex = 5;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Options";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(65, 147);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Scaling Factor";
            // 
            // scalingFactorNumericUpDown
            // 
            this.scalingFactorNumericUpDown.DecimalPlaces = 2;
            this.scalingFactorNumericUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.scalingFactorNumericUpDown.Location = new System.Drawing.Point(6, 145);
            this.scalingFactorNumericUpDown.Name = "scalingFactorNumericUpDown";
            this.scalingFactorNumericUpDown.Size = new System.Drawing.Size(53, 20);
            this.scalingFactorNumericUpDown.TabIndex = 5;
            this.scalingFactorNumericUpDown.Value = new decimal(new int[] {
            75,
            0,
            0,
            131072});
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(2, 182);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(96, 182);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 7;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // OptionsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(173, 207);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.optionsGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "OptionsUI";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "RE2 (2019) SRT";
            this.optionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scalingFactorNumericUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox debugCheckBox;
        private System.Windows.Forms.CheckBox skipChecksumCheckBox;
        private System.Windows.Forms.CheckBox noTitlebarCheckBox;
        private System.Windows.Forms.CheckBox alwaysOnTopCheckBox;
        private System.Windows.Forms.CheckBox transparentBackgroundCheckBox;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown scalingFactorNumericUpDown;
    }
}