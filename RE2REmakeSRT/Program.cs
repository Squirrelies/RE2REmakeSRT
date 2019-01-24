using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace RE2REmakeSRT
{
    public static class Program
    {
        public static ApplicationContext mainContext;
        public static ProgramFlags programSpecialOptions;
        public static Process gameProc;
        public static GameMemory gameMem;

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
                    //MessageBox.Show(null, "Command-line arguments:\r\n\r\n--Coords\t\tShows player coordinates.\r\n--Debug\t\tShows debug info.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MessageBox.Show(null, "Command-line arguments:\r\n\r\n--Debug\t\tShows debug info.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Environment.Exit(0);
                }

                //if (string.Equals(arg, "--Coords", StringComparison.InvariantCultureIgnoreCase))
                //    programSpecialOptions |= ProgramFlags.Coords;

                // Assigning here because debug will always be the sum of all of the options being on.
                if (string.Equals(arg, "--Debug", StringComparison.InvariantCultureIgnoreCase))
                    programSpecialOptions = ProgramFlags.Debug;
            }

            // Standard WinForms stuff.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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
