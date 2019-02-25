using System;
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
            this.Text += string.Format(" {0}", Program.srtVersion);

            debugCheckBox.Checked = (Program.programSpecialOptions.Flags & ProgramFlags.Debug) == ProgramFlags.Debug;
            noTitlebarCheckBox.Checked = (Program.programSpecialOptions.Flags & ProgramFlags.NoTitleBar) == ProgramFlags.NoTitleBar;
            alwaysOnTopCheckBox.Checked = (Program.programSpecialOptions.Flags & ProgramFlags.AlwaysOnTop) == ProgramFlags.AlwaysOnTop;
            transparentBackgroundCheckBox.Checked = (Program.programSpecialOptions.Flags & ProgramFlags.Transparent) == ProgramFlags.Transparent;
            noInventoryCheckBox.Checked = (Program.programSpecialOptions.Flags & ProgramFlags.NoInventory) == ProgramFlags.NoInventory;
            directxOverlayCheckBox.Checked = (Program.programSpecialOptions.Flags & ProgramFlags.DirectXOverlay) == ProgramFlags.DirectXOverlay;
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
            // Warn the user, informing them to restart the SRT.
            MessageBox.Show("Some options do not take effect immediately and you may experience weird display glitches until you restart the SRT.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Set flag changes prior to saving.
            if (debugCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.Debug) != ProgramFlags.Debug)
                Program.programSpecialOptions.Flags |= ProgramFlags.Debug;
            else if (!debugCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.Debug) == ProgramFlags.Debug)
                Program.programSpecialOptions.Flags &= ~ProgramFlags.Debug;

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

            if (noInventoryCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.NoInventory) != ProgramFlags.NoInventory)
                Program.programSpecialOptions.Flags |= ProgramFlags.NoInventory;
            else if (!noInventoryCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.NoInventory) == ProgramFlags.NoInventory)
                Program.programSpecialOptions.Flags &= ~ProgramFlags.NoInventory;

            if (directxOverlayCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.DirectXOverlay) != ProgramFlags.DirectXOverlay)
                Program.programSpecialOptions.Flags |= ProgramFlags.DirectXOverlay;
            else if (!directxOverlayCheckBox.Checked && (Program.programSpecialOptions.Flags & ProgramFlags.DirectXOverlay) == ProgramFlags.DirectXOverlay)
                Program.programSpecialOptions.Flags &= ~ProgramFlags.DirectXOverlay;

            Program.programSpecialOptions.ScalingFactor = (double)scalingFactorNumericUpDown.Value;

            // Write registry values.
            Program.programSpecialOptions.SetOptions();

            // Close form.
            this.Close();
        }
    }
}
