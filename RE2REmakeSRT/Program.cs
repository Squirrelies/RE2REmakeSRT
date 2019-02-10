using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RE2REmakeSRT
{
    public static class Program
    {
        public static ApplicationContext mainContext;
        public static ContextMenu contextMenu;
        public static Options programSpecialOptions;
        public static Process gameProc;
        public static GameMemory gameMem;
        public static Bitmap inventoryImage; // The inventory item sheet.
        public static Bitmap inventoryError; // An error image.
        public static int INV_SLOT_WIDTH;
        public static int INV_SLOT_HEIGHT;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            // Handle command-line parameters.
            programSpecialOptions = new Options();
            programSpecialOptions.GetOptions();

            foreach (string arg in args)
            {
                if (arg.Equals("--Help", StringComparison.InvariantCultureIgnoreCase))
                {
                    StringBuilder message = new StringBuilder("Command-line arguments:\r\n\r\n");
                    message.AppendFormat("{0}\r\n\t{1}\r\n\r\n", "--Skip-Checksum", "Skip the checksum file validation step.");
                    message.AppendFormat("{0}\r\n\t{1}\r\n\r\n", "--No-Titlebar", "Hide the titlebar and window frame.");
                    message.AppendFormat("{0}\r\n\t{1}\r\n\r\n", "--Always-On-Top", "Always appear on top of other windows.");
                    message.AppendFormat("{0}\r\n\t{1}\r\n\r\n", "--Transparent", "Make the background transparent.");
                    message.AppendFormat("{0}\r\n\t{1}\r\n\r\n", "--ScalingFactor=n", "Set the inventory slot scaling factor on a scale of 0.0 to 1.0. Default: 0.75 (75%)");
                    message.AppendFormat("{0}\r\n\t{1}\r\n\r\n", "--Debug", "Debug mode.");

                    MessageBox.Show(null, message.ToString().Trim(), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Environment.Exit(0);
                }

                if (arg.Equals("--Skip-Checksum", StringComparison.InvariantCultureIgnoreCase))
                    programSpecialOptions.Flags |= ProgramFlags.SkipChecksumCheck;

                if (arg.Equals("--No-Titlebar", StringComparison.InvariantCultureIgnoreCase))
                    programSpecialOptions.Flags |= ProgramFlags.NoTitleBar;

                if (arg.Equals("--Always-On-Top", StringComparison.InvariantCultureIgnoreCase))
                    programSpecialOptions.Flags |= ProgramFlags.AlwaysOnTop;

                if (arg.Equals("--Transparent", StringComparison.InvariantCultureIgnoreCase))
                    programSpecialOptions.Flags |= ProgramFlags.Transparent;

                if (arg.StartsWith("--ScalingFactor=", StringComparison.InvariantCultureIgnoreCase))
                    if (!double.TryParse(arg.Split(new char[1] { '=' }, 2, StringSplitOptions.None)[1], out programSpecialOptions.ScalingFactor))
                        programSpecialOptions.ScalingFactor = 0.75d; // Default scaling factor for the inventory images. If we fail to process the user input, ensure this gets set to the default value just in case.

                if (arg.Equals("--Debug", StringComparison.InvariantCultureIgnoreCase))
                    programSpecialOptions.Flags |= ProgramFlags.Debug;
            }

            // Set item slot sizes after scaling is determined.
            INV_SLOT_WIDTH = (int)Math.Round(112d * programSpecialOptions.ScalingFactor, MidpointRounding.AwayFromZero); // Individual inventory slot width.
            INV_SLOT_HEIGHT = (int)Math.Round(112d * programSpecialOptions.ScalingFactor, MidpointRounding.AwayFromZero); // Individual inventory slot height.

            // Standard WinForms stuff.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Transform the inventory image in resources to 32bpp w/ pre-multiplied Alpha.
            inventoryImage = Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(0, 0, Properties.Resources.ui0100_iam_texout.Width, Properties.Resources.ui0100_iam_texout.Height), PixelFormat.Format32bppPArgb);
            
            // Rescales the image down if the scaling factor is not 1.
            if (programSpecialOptions.ScalingFactor != 1d)
            {
                int sheetWidth = (int)Math.Round(inventoryImage.Width * programSpecialOptions.ScalingFactor, MidpointRounding.AwayFromZero);
                int sheetHeight = (int)Math.Round(inventoryImage.Height * programSpecialOptions.ScalingFactor, MidpointRounding.AwayFromZero);
                inventoryImage = new Bitmap(inventoryImage, sheetWidth, sheetHeight);
            }

            // Create a black slot image for when side-pack is not equipped.
            inventoryError = new Bitmap(INV_SLOT_WIDTH, INV_SLOT_HEIGHT, PixelFormat.Format32bppPArgb);
            using (Graphics grp = Graphics.FromImage(inventoryError))
            {
                grp.FillRectangle(new SolidBrush(Color.FromArgb(255, 0, 0, 0)), 0, 0, inventoryError.Width, inventoryError.Height);
                grp.DrawLine(new Pen(Color.FromArgb(150, 255, 0, 0), 3), 0, 0, inventoryError.Width, inventoryError.Height);
                grp.DrawLine(new Pen(Color.FromArgb(150, 255, 0, 0), 3), inventoryError.Width, 0, 0, inventoryError.Height);
            }

            contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("Options", (object sender, EventArgs e) =>
            {
                using (OptionsUI optionsForm = new OptionsUI())
                    optionsForm.ShowDialog();
            });
            contextMenu.MenuItems.Add("-", (object sender, EventArgs e) => { });
            contextMenu.MenuItems.Add("Exit", (object sender, EventArgs e) =>
            {
                Environment.Exit(0);
            });

            // This form finds the process for re2.exe (assigned to gameProc) or waits until it is found.
            using (mainContext = new ApplicationContext(new AttachUI()))
                Application.Run(mainContext);

            // Attach to the re2.exe process now that we've found it and show the UI.
            using (gameMem = new GameMemory(gameProc))
            using (mainContext = new ApplicationContext(new MainUI()))
                Application.Run(mainContext);
        }
    }
}
