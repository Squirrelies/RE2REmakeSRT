﻿using System;
using System.Windows.Forms;

namespace RE2REmakeSRT
{
    public partial class OptionsUI : Form
    {
        private bool alwaysOnTop;

        public OptionsUI()
        {
            InitializeComponent();

            // Set titlebar.
            this.Text += string.Format(" v{0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            debugCheckBox.Checked = (Program.programSpecialOptions.Flags & ProgramFlags.Debug) == ProgramFlags.Debug;
            skipChecksumCheckBox.Checked = (Program.programSpecialOptions.Flags & ProgramFlags.SkipChecksumCheck) == ProgramFlags.SkipChecksumCheck;
            noTitlebarCheckBox.Checked = (Program.programSpecialOptions.Flags & ProgramFlags.NoTitleBar) == ProgramFlags.NoTitleBar;
            alwaysOnTopCheckBox.Checked = (Program.programSpecialOptions.Flags & ProgramFlags.AlwaysOnTop) == ProgramFlags.AlwaysOnTop;
            transparentBackgroundCheckBox.Checked = (Program.programSpecialOptions.Flags & ProgramFlags.Transparent) == ProgramFlags.Transparent;
            scalingFactorNumericUpDown.Value = (decimal)Program.programSpecialOptions.ScalingFactor;

            // Temporarily disable always on top so the MainUI form doesn't take control over this form.
            alwaysOnTop = alwaysOnTopCheckBox.Checked;
            Program.programSpecialOptions.Flags &= ~ProgramFlags.AlwaysOnTop;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            // If always on top was on prior to opening options, re-enable it.
            if (alwaysOnTop)
                Program.programSpecialOptions.Flags |= ProgramFlags.AlwaysOnTop;

            // Close form.
            this.Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            // Set flag changes prior to saving.
            if (debugCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.Debug) != ProgramFlags.Debug)
                Program.programSpecialOptions.Flags |= ProgramFlags.Debug;
            else if (!debugCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.Debug) == ProgramFlags.Debug)
                Program.programSpecialOptions.Flags &= ~ProgramFlags.Debug;

            if (skipChecksumCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.SkipChecksumCheck) != ProgramFlags.SkipChecksumCheck)
                Program.programSpecialOptions.Flags |= ProgramFlags.SkipChecksumCheck;
            else if (!skipChecksumCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.SkipChecksumCheck) == ProgramFlags.SkipChecksumCheck)
                Program.programSpecialOptions.Flags &= ~ProgramFlags.SkipChecksumCheck;

            if (noTitlebarCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.NoTitleBar) != ProgramFlags.NoTitleBar)
                Program.programSpecialOptions.Flags |= ProgramFlags.NoTitleBar;
            else if (!noTitlebarCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.NoTitleBar) == ProgramFlags.NoTitleBar)
                Program.programSpecialOptions.Flags &= ~ProgramFlags.NoTitleBar;

            if (alwaysOnTopCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.AlwaysOnTop) != ProgramFlags.AlwaysOnTop)
                Program.programSpecialOptions.Flags |= ProgramFlags.AlwaysOnTop;
            else if (!alwaysOnTopCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.AlwaysOnTop) == ProgramFlags.AlwaysOnTop)
                Program.programSpecialOptions.Flags &= ~ProgramFlags.AlwaysOnTop;

            if (transparentBackgroundCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.Transparent) != ProgramFlags.Transparent)
                Program.programSpecialOptions.Flags |= ProgramFlags.Transparent;
            else if (!transparentBackgroundCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.Transparent) == ProgramFlags.Transparent)
                Program.programSpecialOptions.Flags &= ~ProgramFlags.Transparent;

            Program.programSpecialOptions.ScalingFactor = (double)scalingFactorNumericUpDown.Value;

            // Write registry values.
            Program.programSpecialOptions.SetOptions();

            // Close form.
            this.Close();
        }
    }
}
