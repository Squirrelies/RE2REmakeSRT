using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace RE2REmakeSRT
{
    public static class Program
    {
        public static ContextMenu contextMenu;
        public static Options programSpecialOptions;
        public static int gamePID;
        public static IntPtr gameWindowHandle;
        public static GameMemory gameMemory;

        public static readonly string srtVersion = string.Format("v{0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        public static readonly string srtTitle = string.Format("RE2(2019) SRT - {0}", srtVersion);

        public static int INV_SLOT_WIDTH;
        public static int INV_SLOT_HEIGHT;

        public static IReadOnlyDictionary<ItemEnumeration, System.Drawing.Rectangle> ItemToImageTranslation;
        public static IReadOnlyDictionary<Weapon, System.Drawing.Rectangle> WeaponToImageTranslation;

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

                GenerateClipping();

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
            using (gameMemory = new GameMemory(gamePID))
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

        public static void GenerateClipping()
        {
            int itemColumnInc = -1;
            int itemRowInc = -1;
            ItemToImageTranslation = new Dictionary<ItemEnumeration, System.Drawing.Rectangle>()
            {
                { ItemEnumeration.None, new System.Drawing.Rectangle(0, 0, 0, 0) },

                // Row 0.
                { ItemEnumeration.FirstAidSpray, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 0), Program.INV_SLOT_HEIGHT * ++itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Herb_Green2, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Herb_Red2, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Herb_Blue2, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Herb_Mixed_GG, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Herb_Mixed_GR, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Herb_Mixed_GB, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Herb_Mixed_GGB, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Herb_Mixed_GGG, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Herb_Mixed_GRB, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Herb_Mixed_RB, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Herb_Green1, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Herb_Red1, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Herb_Blue1, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },

                // Row 1.
                { ItemEnumeration.HandgunBullets, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 0), Program.INV_SLOT_HEIGHT * ++itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.ShotgunShells, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.SubmachineGunAmmo, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.MAGAmmo, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.HandgunLargeCaliberAmmo, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.SLS60HighPoweredRounds, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.GrenadeAcidRounds, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.GrenadeFlameRounds, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.NeedleCartridges, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Fuel, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.InkRibbon, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.WoodenBoard, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Gunpowder, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.GunpowderLarge, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.GunpowderHighGradeYellow, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.GunpowderHighGradeWhite, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.HipPouch, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },

                // Row 2.
                { ItemEnumeration.MatildaHighCapacityMagazine, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 0), Program.INV_SLOT_HEIGHT * ++itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.MatildaMuzzleBrake, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.MatildaGunStock, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.SLS60SpeedLoader, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.JMBHp3LaserSight, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.SLS60ReinforcedFrame, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.JMBHp3HighCapacityMagazine, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.W870ShotgunStock, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.W870LongBarrel, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.MQ11HighCapacityMagazine, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.MQ11Suppressor, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.LightningHawkRedDotSight, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.LightningHawkLongBarrel, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.GM79ShoulderStock, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.FlamethrowerRegulator, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.SparkShotHighVoltageCondenser, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },

                //Row 3.
                { ItemEnumeration.Film_HidingPlace, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 9), Program.INV_SLOT_HEIGHT * ++itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Film_RisingRookie, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Film_Commemorative, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Film_3FLocker, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Film_LionStatue, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.PortableSafe, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.TinStorageBox1, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.TinStorageBox2, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },

                // Row 4.
                { ItemEnumeration.Detonator, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 0), Program.INV_SLOT_HEIGHT * ++itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.ElectronicGadget, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Battery9Volt, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.KeyStorageRoom, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 4), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.JackHandle, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 6), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.SquareCrank, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.MedallionUnicorn, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.KeySpade, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.KeyCardParkingGarage, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.KeyCardWeaponsLocker, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.ValveHandle, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 13), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.STARSBadge, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Scepter, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.RedJewel, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.BejeweledBox, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },

                // Row 5.
                { ItemEnumeration.PlugBishop, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 1), Program.INV_SLOT_HEIGHT * ++itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.PlugRook, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.PlugKing, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.PictureBlock, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.USBDongleKey, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.SpareKey, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.RedBook, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 8), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.StatuesLeftArm, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.StatuesLeftArmWithRedBook, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.MedallionLion, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.KeyDiamond, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.KeyCar, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.MedallionMaiden, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 15), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.PowerPanelPart1, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.PowerPanelPart2, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },

                // Row 6.
                { ItemEnumeration.LoversRelief, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 0), Program.INV_SLOT_HEIGHT * ++itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.KeyOrphanage, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.KeyClub, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.KeyHeart, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.USSDigitalVideoCassette, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.TBarValveHandle, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.SignalModulator, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.KeySewers, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 8), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.IDWristbandVisitor1, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.IDWristbandGeneralStaff1, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.IDWristbandSeniorStaff1, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.UpgradeChipGeneralStaff, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.UpgradeChipSeniorStaff, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.FuseMainHall, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 15), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Scissors, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.BoltCutter, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },

                // Row 7.
                { ItemEnumeration.StuffedDoll, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 0), Program.INV_SLOT_HEIGHT * ++itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.IDWristbandVisitor2, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 2), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.IDWristbandGeneralStaff2, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.IDWristbandSeniorStaff2, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.LabDigitalVideoCassette, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.DispersalCartridgeEmpty, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.DispersalCartridgeSolution, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.DispersalCartridgeHerbicide, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.JointPlug, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 10), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Trophy1, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 12), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.Trophy2, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.GearSmall, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.GearLarge, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 14), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.PlugKnight, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 16), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.PlugPawn, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },

                // Row 8.
                { ItemEnumeration.PlugQueen, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 0), Program.INV_SLOT_HEIGHT * ++itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.BoxedElectronicPart1, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 2), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.BoxedElectronicPart2, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.UpgradeChipAdministrator, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 5), Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.IDWristbandAdministrator, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.KeyCourtyard, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.FuseBreakRoom, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.JointPlug2, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.GearLarge2, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },

                // Row 9.
                //     No items.

                // Row 10.
                //     No items.

                // Row 11.
                //     No items.

                // Row 12.
                //     No items.

                // Row 13.
                //     No items.

                // Row 14.
                { ItemEnumeration.WoodenBox1, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (itemColumnInc = 9), Program.INV_SLOT_HEIGHT * ++itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { ItemEnumeration.WoodenBox2, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++itemColumnInc, Program.INV_SLOT_HEIGHT * itemRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },

                // Patch Items.
                { ItemEnumeration.OldKey, new System.Drawing.Rectangle(0, 0, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
            };

            int weaponColumnInc = -1;
            int weaponRowInc = 8;
            WeaponToImageTranslation = new Dictionary<Weapon, System.Drawing.Rectangle>()
            {
                { new Weapon() { WeaponID = WeaponEnumeration.None, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(0, 0, 0, 0) },

                // Row 1.
                //     No weapons.

                // Row 2.
                //     No weapons.

                // Row 3.
                //     No weapons.

                // Row 4.
                //     No weapons.

                // Row 5.
                //     No weapons.

                // Row 6.
                //     No weapons.

                // Row 7.
                //     No weapons.

                // Row 8.
                //     No weapons.

                // Row 9.
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 0), Program.INV_SLOT_HEIGHT * ++weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.First }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.Second }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 3), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.Third }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.First | AttachmentsFlag.Third }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 7), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.Second | AttachmentsFlag.Third }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 9), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second | AttachmentsFlag.Third }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_JMB_Hp3, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 12), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_JMB_Hp3, Attachments = AttachmentsFlag.Second }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_JMB_Hp3, Attachments = AttachmentsFlag.Third }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_JMB_Hp3, Attachments = AttachmentsFlag.Second | AttachmentsFlag.Third }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_MUP, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_BroomHc, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },

                // Row 10.
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SLS60, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 0), Program.INV_SLOT_HEIGHT * ++weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SLS60, Attachments = AttachmentsFlag.Third }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH , Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_M19, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH , Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SLS60, Attachments = AttachmentsFlag.Second }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH , Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SLS60, Attachments = AttachmentsFlag.Second | AttachmentsFlag.Third }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Shotgun_W870, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 6), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Shotgun_W870, Attachments = AttachmentsFlag.First }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Shotgun_W870, Attachments = AttachmentsFlag.Second }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 9), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Shotgun_W870, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH* 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.SMG_MQ11, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 12), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.SMG_MQ11, Attachments = AttachmentsFlag.First }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH* 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.SMG_MQ11, Attachments = AttachmentsFlag.Second }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 15), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.SMG_MQ11, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH* 2, Program.INV_SLOT_HEIGHT) },

                // Row 11.
                { new Weapon() { WeaponID = WeaponEnumeration.SMG_LE5_Infinite2, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 0), Program.INV_SLOT_HEIGHT * ++weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_LightningHawk, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_LightningHawk, Attachments = AttachmentsFlag.First }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_LightningHawk, Attachments = AttachmentsFlag.Second }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 4), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_LightningHawk, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.GrenadeLauncher_GM79, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 7), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.GrenadeLauncher_GM79, Attachments = AttachmentsFlag.First }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.ChemicalFlamethrower, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 10), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.ChemicalFlamethrower, Attachments = AttachmentsFlag.Second }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 12), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.SparkShot, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 14), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.SparkShot, Attachments = AttachmentsFlag.Second }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 16), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },

                // Row 12.
                { new Weapon() { WeaponID = WeaponEnumeration.ATM4, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 0), Program.INV_SLOT_HEIGHT * ++weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.ATM4_Infinite, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 0), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 2), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher_Infinite, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 2), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Minigun, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 4), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Minigun_Infinite, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 4), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.EMF_Visualizer, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 6), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Quickdraw_Army, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.SMG_LE5_Infinite, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 9), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SamuraiEdge_Infinite, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 11), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SamuraiEdge_AlbertWesker, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 11), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SamuraiEdge_ChrisRedfield, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 11), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SamuraiEdge_JillValentine, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 11), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.ATM42, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 14), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher2, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 16), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },


                // Row 13.
                { new Weapon() { WeaponID = WeaponEnumeration.CombatKnife, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 0), Program.INV_SLOT_HEIGHT * ++weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.CombatKnife_Infinite, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH , Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Minigun2, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 2), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH * 2, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.ChemicalFlamethrower2, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 4), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.SparkShot2, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH , Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.ATM43, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH , Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher3, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH , Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.Minigun3, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH , Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.HandGrenade, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH , Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.FlashGrenade, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH , Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.ATM44, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 16), Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
                { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher4, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * ++weaponColumnInc, Program.INV_SLOT_HEIGHT * weaponRowInc, Program.INV_SLOT_WIDTH , Program.INV_SLOT_HEIGHT) },

                // Row 14.
                { new Weapon() { WeaponID = WeaponEnumeration.Minigun4, Attachments = AttachmentsFlag.None }, new System.Drawing.Rectangle(Program.INV_SLOT_WIDTH * (weaponColumnInc = 4), Program.INV_SLOT_HEIGHT * ++weaponRowInc, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT) },
            };
        }
    }
}
