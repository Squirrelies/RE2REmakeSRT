using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace RE2REmakeSRT
{
    public static class Program
    {
        public static ApplicationContext mainContext;
        public static ProgramFlags programSpecialOptions;
        public static Process gameProc;
        public static GameMemory gameMem;
        public static Bitmap inventoryImage;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            // Handle command-line parameters.
            programSpecialOptions = ProgramFlags.None;
            foreach (string arg in args)
            {
                if (string.Equals(arg, "--Help", StringComparison.InvariantCultureIgnoreCase))
                {
                    MessageBox.Show(null, "Command-line arguments:\r\n\r\n--Skip-Checksum\t\tSkips the checksum file validation step.\r\n--Debug\t\tDebug mode.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Environment.Exit(0);
                }

                if (string.Equals(arg, "--Skip-Checksum", StringComparison.InvariantCultureIgnoreCase))
                    programSpecialOptions |= ProgramFlags.SkipChecksumCheck;

                // Assigning here because debug will always be the sum of all of the options being on.
                if (string.Equals(arg, "--Debug", StringComparison.InvariantCultureIgnoreCase))
                    programSpecialOptions = ProgramFlags.Debug;
            }

            // Standard WinForms stuff.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Transform the inventory image in resources down from 32bpp w/ Alpha to 16bpp w/o Alpha. This greatly improve performance especially when coupled with CompositingMode.SourceCopy because no complex alpha blending needs to occur.
            inventoryImage = Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(0, 0, Properties.Resources.ui0100_iam_texout.Width, Properties.Resources.ui0100_iam_texout.Height), PixelFormat.Format16bppRgb555);

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
