using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RE2REmakeSRT
{
    public static class Program
    {
        public static ContextMenu contextMenu;
        public static Options programSpecialOptions;
        public static int gamePID;
        public static IntPtr gameWindowHandle;
        public static GameMemory gameMem;

        public static readonly string srtVersion = string.Format("v{0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        public static readonly string srtTitle = string.Format("RE2(2019) SRT - {0}", srtVersion);

        public static int INV_SLOT_WIDTH;
        public static int INV_SLOT_HEIGHT;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // Handle command-line parameters.
                programSpecialOptions = new Options();
                programSpecialOptions.GetOptions();

                foreach (string arg in args)
                {
                    if (arg.Equals("--Help", StringComparison.InvariantCultureIgnoreCase))
                    {
                        StringBuilder message = new StringBuilder("Command-line arguments:\r\n\r\n");
                        message.AppendFormat("{0}\r\n\t{1}\r\n\r\n", "--No-Titlebar", "Hide the titlebar and window frame.");
                        message.AppendFormat("{0}\r\n\t{1}\r\n\r\n", "--Always-On-Top", "Always appear on top of other windows.");
                        message.AppendFormat("{0}\r\n\t{1}\r\n\r\n", "--Transparent", "Make the background transparent.");
                        message.AppendFormat("{0}\r\n\t{1}\r\n\r\n", "--ScalingFactor=n", "Set the inventory slot scaling factor on a scale of 0.0 to 1.0. Default: 0.75 (75%)");
                        message.AppendFormat("{0}\r\n\t{1}\r\n\r\n", "--NoInventory", "Disables the inventory display.");
                        message.AppendFormat("{0}\r\n\t{1}\r\n\r\n", "--DirectX", "Enables the DirectX overlay.");
                        message.AppendFormat("{0}\r\n\t{1}\r\n\r\n", "--Debug", "Debug mode.");

                        MessageBox.Show(null, message.ToString().Trim(), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Environment.Exit(0);
                    }

                    if (arg.Equals("--No-Titlebar", StringComparison.InvariantCultureIgnoreCase))
                        programSpecialOptions.Flags |= ProgramFlags.NoTitleBar;

                    if (arg.Equals("--Always-On-Top", StringComparison.InvariantCultureIgnoreCase))
                        programSpecialOptions.Flags |= ProgramFlags.AlwaysOnTop;

                    if (arg.Equals("--Transparent", StringComparison.InvariantCultureIgnoreCase))
                        programSpecialOptions.Flags |= ProgramFlags.Transparent;

                    if (arg.Equals("--NoInventory", StringComparison.InvariantCultureIgnoreCase))
                        programSpecialOptions.Flags |= ProgramFlags.NoInventory;

                    if (arg.Equals("--DirectX", StringComparison.InvariantCultureIgnoreCase))
                        programSpecialOptions.Flags |= ProgramFlags.DirectXOverlay;

                    if (arg.StartsWith("--ScalingFactor=", StringComparison.InvariantCultureIgnoreCase))
                        if (!double.TryParse(arg.Split(new char[1] { '=' }, 2, StringSplitOptions.None)[1], out programSpecialOptions.ScalingFactor))
                            programSpecialOptions.ScalingFactor = 0.75d; // Default scaling factor for the inventory images. If we fail to process the user input, ensure this gets set to the default value just in case.

                    if (arg.Equals("--Debug", StringComparison.InvariantCultureIgnoreCase))
                        programSpecialOptions.Flags |= ProgramFlags.Debug;
                }

                // Context menu.
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

                // Set item slot sizes after scaling is determined.
                INV_SLOT_WIDTH = (int)Math.Round(112d * programSpecialOptions.ScalingFactor, MidpointRounding.AwayFromZero); // Individual inventory slot width.
                INV_SLOT_HEIGHT = (int)Math.Round(112d * programSpecialOptions.ScalingFactor, MidpointRounding.AwayFromZero); // Individual inventory slot height.

                // Standard WinForms stuff.
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                AttachAndShowUI();
            }
            catch (Exception ex)
            {
                FailFast(string.Format("[{0}] An unhandled exception has occurred. Please see below for details.\r\n\r\n[{1}] {2}\r\n{3}.", srtVersion, ex.GetType().ToString(), ex.Message, ex.StackTrace), ex);
            }
        }

        public static void AttachAndShowUI()
        {
            // This form finds the process for re2.exe (assigned to gameProc) or waits until it is found.
            using (AttachUI attachUI = new AttachUI())
            using (ApplicationContext mainContext = new ApplicationContext(attachUI))
            {
                Application.Run(mainContext);
            }

            // If we exited the attach UI without finding a PID, bail out completely.
            Debug.WriteLine("Checking PID for -1...");
            if (gamePID == -1)
                return;

            // Attach to the re2.exe process now that we've found it and show the UI.
            Debug.WriteLine("Showing MainUI...");
            using (gameMem = new GameMemory(gamePID))
            using (MainUI mainUI = new MainUI())
            using (ApplicationContext mainContext = new ApplicationContext(mainUI))
            {
                Application.Run(mainContext);
            }
        }

        public static void GetProcessInfo()
        {
            Process[] gameProcesses = Process.GetProcessesByName("re2");
            Debug.WriteLine("RE2 (2019) processes found: {0}", gameProcesses.Length);
            if (gameProcesses.Length != 0)
            {
                foreach (Process p in gameProcesses)
                {
                    Debug.WriteLine("PID: {0}", p.Id);
                }
                gamePID = gameProcesses[0].Id;
                gameWindowHandle = gameProcesses[0].MainWindowHandle;
            }
            else
            {
                gamePID = -1;
                gameWindowHandle = IntPtr.Zero;
            }
        }

        public static void FailFast(string message, Exception ex)
        {
            ShowError(message);
            Environment.FailFast(message, ex);
        }

        public static void ShowError(string message)
        {
            MessageBox.Show(message, srtTitle, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        public static string GetExceptionMessage(Exception ex) => string.Format("[{0}] An unhandled exception has occurred. Please see below for details.\r\n\r\n[{1}] {2}\r\n{3}.", srtVersion, ex.GetType().ToString(), ex.Message, ex.StackTrace);
    }
}
