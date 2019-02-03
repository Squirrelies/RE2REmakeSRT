using ProcessMemory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace RE2REmakeSRT
{
    public class GameMemory : IDisposable
    {
        // Private Variables
        private REmake2VersionEnumeration gameVersion;
        public ProcessMemory.ProcessMemory memoryAccess;
        private const string IGT_TIMESPAN_STRING_FORMAT = @"hh\:mm\:ss\.fff";

        // Pointers
        public long BaseAddress { get; private set; }
        public MultilevelPointer PointerIGT { get; private set; }
        public MultilevelPointer PointerRank { get; private set; }
        public MultilevelPointer PointerPlayerHP { get; private set; }
        public MultilevelPointer[] PointerEnemyEntries { get; private set; }
        public MultilevelPointer[] PointerInventoryEntries { get; private set; }

        // Public Properties
        public int PlayerCurrentHealth { get; private set; }
        public int PlayerMaxHealth { get; private set; }
        public InventoryEntry[] PlayerInventory { get; private set; }
        public int[] EnemyCurrentHealth { get; private set; }
        public int[] EnemyMaxHealth { get; private set; }
        public long IGTRunningTimer { get; private set; }
        public long IGTCutsceneTimer { get; private set; }
        public long IGTMenuTimer { get; private set; }
        public long IGTPausedTimer { get; private set; }
        public int Rank { get; private set; }
        public float RankScore { get; private set; }

        // Public Properties - Calculated
        public long IGTRaw => unchecked(IGTRunningTimer - IGTCutsceneTimer - IGTPausedTimer);
        public long IGTCalculated => unchecked(IGTRaw * 10L);
        public TimeSpan IGTTimeSpan
        {
            get
            {
                TimeSpan timespanIGT;

                if (IGTCalculated <= TimeSpan.MaxValue.Ticks)
                    timespanIGT = new TimeSpan(IGTCalculated);
                else
                    timespanIGT = new TimeSpan();

                return timespanIGT;
            }
        }
        public string IGTString => IGTTimeSpan.ToString(IGT_TIMESPAN_STRING_FORMAT, CultureInfo.InvariantCulture);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proc"></param>
        public GameMemory(Process proc)
        {
            gameVersion = REmake2VersionDetector.GetVersion(proc);
            memoryAccess = new ProcessMemory.ProcessMemory(proc.Id);
            BaseAddress = proc.MainModule.BaseAddress.ToInt64();

            // Setup the pointers.
            switch (gameVersion)
            {
                case REmake2VersionEnumeration.Stock_1p00:
                case REmake2VersionEnumeration.Stock_1p01:
                    {
                        PointerIGT = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACAE0, 0x2E0, 0x218, 0x610, 0x710, 0x60);
                        PointerRank = new MultilevelPointer(memoryAccess, BaseAddress + 0x07086DB0);
                        PointerPlayerHP = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x20);

                        PointerEnemyEntries = new MultilevelPointer[32];
                        for (int i = 0; i < PointerEnemyEntries.Length; ++i)
                            PointerEnemyEntries[i] = new MultilevelPointer(memoryAccess, BaseAddress + 0x0707B758, 0x80 + (i * 0x08), 0x88, 0x18, 0x1A0);

                        PointerInventoryEntries = new MultilevelPointer[20];
                        for (int i = 0; i < PointerInventoryEntries.Length; ++i)
                            PointerInventoryEntries[i] = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x20 + (i * 0x08), 0x18, 0x10);

                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            // Initialize variables to default values.
            PlayerCurrentHealth = 0;
            PlayerMaxHealth = 0;
            PlayerInventory = new InventoryEntry[20];
            EnemyCurrentHealth = new int[32];
            EnemyMaxHealth = new int[32];
            IGTRunningTimer = 0L;
            IGTCutsceneTimer = 0L;
            IGTMenuTimer = 0L;
            IGTPausedTimer = 0L;
            Rank = 0;
            RankScore = 0f;
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdatePointers()
        {
            PointerIGT.UpdatePointers();
            PointerRank.UpdatePointers();
            PointerPlayerHP.UpdatePointers();

            for (int i = 0; i < PointerEnemyEntries.Length; ++i)
                PointerEnemyEntries[i].UpdatePointers();

            for (int i = 0; i < PointerInventoryEntries.Length; ++i)
                PointerInventoryEntries[i].UpdatePointers();
        }

        /// <summary>
        /// This call refreshes impotant variables such as IGT, HP and Ammo.
        /// </summary>
        /// <param name="cToken"></param>
        public void RefreshSlim()
        {
            // IGT
            IGTRunningTimer = PointerIGT.DerefLong(0x18);
            IGTCutsceneTimer = PointerIGT.DerefLong(0x20);
            IGTMenuTimer = PointerIGT.DerefLong(0x28);
            IGTPausedTimer = PointerIGT.DerefLong(0x30);

            // Enemy HP
            for (int i = 0; i < PointerEnemyEntries.Length; ++i)
            {
                EnemyMaxHealth[i] = PointerEnemyEntries[i].DerefInt(0x54);
                EnemyCurrentHealth[i] = PointerEnemyEntries[i].DerefInt(0x58);
            }
        }

        /// <summary>
        /// This call refreshes everything. This should be used less often. Inventory rendering can be more expensive and doesn't change as often.
        /// </summary>
        /// <param name="cToken"></param>
        public void Refresh()
        {
            // Perform slim lookups first.
            RefreshSlim();

            // Other lookups that don't need to update as often.
            // Player HP
            PlayerMaxHealth = PointerPlayerHP.DerefInt(0x54);
            PlayerCurrentHealth = PointerPlayerHP.DerefInt(0x58);

            // Inventory
            for (int i = 0; i < PointerInventoryEntries.Length; ++i)
                PlayerInventory[i] = new InventoryEntry(PointerInventoryEntries[i].DerefByteArray(-0x90, 0xF0));

            // Rank
            Rank = PointerRank.DerefInt(0x58);
            RankScore = PointerRank.DerefFloat(0x5C);
        }

        public const int INV_SLOT_WIDTH = 112;
        public const int INV_SLOT_HEIGHT = 112;

        //private static int itemColumnInc = -1;
        //private static int itemRowInc = -1;
        //public static IReadOnlyDictionary<ItemEnumeration, Bitmap> ItemToImageTranslation = new Dictionary<ItemEnumeration, Bitmap>()
        //{
        //    { ItemEnumeration.None, Program.blankInvImage },

        //    // Row 0.
        //    { ItemEnumeration.FirstAidSpray, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Herb_Green1, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Herb_Red1, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Herb_Blue1, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Herb_Mixed_GG, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Herb_Mixed_GR, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Herb_Mixed_GB, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Herb_Mixed_GGB, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Herb_Mixed_GGG, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Herb_Mixed_GRB, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Herb_Mixed_RB, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Herb_Green2, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Herb_Red2, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Herb_Blue2, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },

        //    // Row 1.
        //    { ItemEnumeration.HandgunBullets, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.ShotgunShells, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.SubmachineGunAmmo, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.MAGAmmo, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.HandgunLargeCaliberAmmo, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.SLS60HighPoweredRounds, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.GrenadeAcidRounds, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.GrenadeFlameRounds, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.NeedleCartridges, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Fuel, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.InkRibbon, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.WoodenBoard, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Gunpowder, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.GunpowderLarge, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.GunpowderHighGradeYellow, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.GunpowderHighGradeWhite, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.HipPouch, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },

        //    // Row 2.
        //    { ItemEnumeration.MatildaHighCapacityMagazine, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.MatildaMuzzleBrake, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.MatildaGunStock, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.SLS60SpeedLoader, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.JMBHp3LaserSight, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.SLS60ReinforcedFrame, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.JMBHp3HighCapacityMagazine, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.W870ShotgunStock, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.W870LongBarrel, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.MQ11HighCapacityMagazine, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.MQ11Suppressor, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.LightningHawkRedDotSight, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.LightningHawkLongBarrel, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.GM79ShoulderStock, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.FlamethrowerRegulator, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.SparkShotHighVoltageCondenser, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },

        //    //Row 3.
        //    { ItemEnumeration.Film_HidingPlace, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 9), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Film_RisingRookie, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Film_Commemorative, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Film_3FLocker, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Film_LionStatue, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.PortableSafe, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.TinStorageBox1, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.TinStorageBox2, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },

        //    // Row 4.
        //    { ItemEnumeration.Detonator, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.ElectronicGadget, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Battery9Volt, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.KeyStorageRoom, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 4), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.JackHandle, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 6), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.SquareCrank, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.MedallionUnicorn, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.KeySpade, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.KeyCardParkingGarage, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.KeyCardWeaponsLocker, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.ValveHandle, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 13), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.STARSBadge, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Scepter, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.RedJewel, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.BejeweledBox, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },

        //    // Row 5.
        //    { ItemEnumeration.PlugBishop, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 1), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.PlugRook, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.PlugKing, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.PictureBlock, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.USBDongleKey, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.SpareKey, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.RedBook, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 8), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.StatuesLeftArm, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.StatuesLeftArmWithRedBook, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.MedallionLion, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.KeyDiamond, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.KeyCar, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.MedallionMaiden, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 15), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.PowerPanelPart1, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.PowerPanelPart2, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },

        //    // Row 6.
        //    { ItemEnumeration.LoversRelief, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.KeyOrphanage, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.KeyClub, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.KeyHeart, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.USSDigitalVideoCassette, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.TBarValveHandle, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.SignalModulator, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.KeySewers, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 8), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.IDWristbandVisitor1, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.IDWristbandGeneralStaff1, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.IDWristbandSeniorStaff1, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.UpgradeChipGeneralStaff, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.UpgradeChipSeniorStaff, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.FuseMainHall, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 15), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Scissors, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.BoltCutter, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },

        //    // Row 7.
        //    { ItemEnumeration.StuffedDoll, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.IDWristbandVisitor2, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 2), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.IDWristbandGeneralStaff2, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.IDWristbandSeniorStaff2, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.LabDigitalVideoCassette, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.DispersalCartridgeEmpty, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.DispersalCartridgeSolution, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.DispersalCartridgeHerbicide, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.JointPlug, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 10), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Trophy1, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 12), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.Trophy2, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.GearSmall, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.GearLarge, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 14), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.PlugKnight, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 16), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.PlugPawn, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },

        //    // Row 8.
        //    { ItemEnumeration.PlugQueen, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.BoxedElectronicPart1, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 2), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.BoxedElectronicPart2, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.UpgradeChipAdministrator, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 5), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.IDWristbandAdministrator, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.KeyCourtyard, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.FuseBreakRoom, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.JointPlug2, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.GearLarge2, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },

        //    // Row 9.
        //    //     No items.

        //    // Row 10.
        //    //     No items.

        //    // Row 11.
        //    //     No items.

        //    // Row 12.
        //    //     No items.

        //    // Row 13.
        //    //     No items.

        //    // Row 14.
        //    { ItemEnumeration.WoodenBox1, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 9), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { ItemEnumeration.WoodenBox2, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //};

        //private static int weaponColumnInc = -1;
        //private static int weaponRowInc = 8;
        //public static IReadOnlyDictionary<Weapon, Bitmap> WeaponToImageTranslation = new Dictionary<Weapon, Bitmap>()
        //{
        //    { new Weapon() { WeaponID = WeaponEnumeration.None, Attachments = AttachmentsFlag.None }, Program.blankInvImage },

        //    // Row 1.
        //    //     No weapons.

        //    // Row 2.
        //    //     No weapons.

        //    // Row 3.
        //    //     No weapons.

        //    // Row 4.
        //    //     No weapons.

        //    // Row 5.
        //    //     No weapons.

        //    // Row 6.
        //    //     No weapons.

        //    // Row 7.
        //    //     No weapons.

        //    // Row 8.
        //    //     No weapons.

        //    // Row 9.
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 0), INV_SLOT_HEIGHT * ++weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.First }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.Second }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 3), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.Third }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.First | AttachmentsFlag.Third }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 7), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.Second | AttachmentsFlag.Third }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 9), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second | AttachmentsFlag.Third }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_JMB_Hp3, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 12), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_JMB_Hp3, Attachments = AttachmentsFlag.Second }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_JMB_Hp3, Attachments = AttachmentsFlag.Third }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_JMB_Hp3, Attachments = AttachmentsFlag.Second | AttachmentsFlag.Third }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_MUP, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_BroomHc, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },

        //    // Row 10.
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SLS60, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 0), INV_SLOT_HEIGHT * ++weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SLS60, Attachments = AttachmentsFlag.Third }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_M19, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SLS60, Attachments = AttachmentsFlag.Second }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SLS60, Attachments = AttachmentsFlag.Second | AttachmentsFlag.Third }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Shotgun_W870, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 6), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Shotgun_W870, Attachments = AttachmentsFlag.First }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Shotgun_W870, Attachments = AttachmentsFlag.Second }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 9), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Shotgun_W870, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH* 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.SMG_MQ11, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 12), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.SMG_MQ11, Attachments = AttachmentsFlag.First }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH* 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.SMG_MQ11, Attachments = AttachmentsFlag.Second }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 15), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.SMG_MQ11, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH* 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },

        //    // Row 11.
        //    { new Weapon() { WeaponID = WeaponEnumeration.SMG_LE5_Infinite2, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 0), INV_SLOT_HEIGHT * ++weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_LightningHawk, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_LightningHawk, Attachments = AttachmentsFlag.First }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_LightningHawk, Attachments = AttachmentsFlag.Second }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 4), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_LightningHawk, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.GrenadeLauncher_GM79, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 7), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.GrenadeLauncher_GM79, Attachments = AttachmentsFlag.First }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.ChemicalFlamethrower, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 10), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.ChemicalFlamethrower, Attachments = AttachmentsFlag.First }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 12), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.SparkShot, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 14), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.SparkShot, Attachments = AttachmentsFlag.First }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 16), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },

        //    // Row 12.
        //    { new Weapon() { WeaponID = WeaponEnumeration.ATM4, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 0), INV_SLOT_HEIGHT * ++weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.ATM4_Infinite, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 0), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 2), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher_Infinite, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 2), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Minigun, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 4), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Minigun_Infinite, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 4), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.EMF_Visualizer, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 6), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Quickdraw_Army, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.SMG_LE5_Infinite, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 9), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SamuraiEdge_Infinite, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 11), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SamuraiEdge_AlbertWesker, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 11), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SamuraiEdge_ChrisRedfield, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 11), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SamuraiEdge_JillValentine, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 11), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.ATM42, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 14), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher2, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 16), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },


        //    // Row 13.
        //    { new Weapon() { WeaponID = WeaponEnumeration.CombatKnife, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 0), INV_SLOT_HEIGHT * ++weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.CombatKnife_Infinite, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Minigun2, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 2), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.ChemicalFlamethrower2, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 4), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.SparkShot2, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.ATM43, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher3, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.Minigun3, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.HandGrenade, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.FlashGrenade, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.ATM44, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 16), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //    { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher4, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },

        //    // Row 14.
        //    { new Weapon() { WeaponID = WeaponEnumeration.Minigun4, Attachments = AttachmentsFlag.None }, Properties.Resources.ui0100_iam_texout.Clone(new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 4), INV_SLOT_HEIGHT * ++weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT), System.Drawing.Imaging.PixelFormat.Format32bppArgb) },
        //};







        private static int itemColumnInc = -1;
        private static int itemRowInc = -1;
        public static IReadOnlyDictionary<ItemEnumeration, Rectangle> ItemToImageTranslation = new Dictionary<ItemEnumeration, Rectangle>()
        {
            { ItemEnumeration.None, new Rectangle(0, 0, 0, 0) },

            // Row 0.
            { ItemEnumeration.FirstAidSpray, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Herb_Green1, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Herb_Red1, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Herb_Blue1, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Herb_Mixed_GG, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Herb_Mixed_GR, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Herb_Mixed_GB, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Herb_Mixed_GGB, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Herb_Mixed_GGG, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Herb_Mixed_GRB, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Herb_Mixed_RB, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Herb_Green2, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Herb_Red2, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Herb_Blue2, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },

            // Row 1.
            { ItemEnumeration.HandgunBullets, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.ShotgunShells, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.SubmachineGunAmmo, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.MAGAmmo, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.HandgunLargeCaliberAmmo, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.SLS60HighPoweredRounds, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.GrenadeAcidRounds, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.GrenadeFlameRounds, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.NeedleCartridges, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Fuel, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.InkRibbon, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.WoodenBoard, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Gunpowder, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.GunpowderLarge, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.GunpowderHighGradeYellow, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.GunpowderHighGradeWhite, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.HipPouch, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },

            // Row 2.
            { ItemEnumeration.MatildaHighCapacityMagazine, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.MatildaMuzzleBrake, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.MatildaGunStock, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.SLS60SpeedLoader, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.JMBHp3LaserSight, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.SLS60ReinforcedFrame, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.JMBHp3HighCapacityMagazine, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.W870ShotgunStock, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.W870LongBarrel, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.MQ11HighCapacityMagazine, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.MQ11Suppressor, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.LightningHawkRedDotSight, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.LightningHawkLongBarrel, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.GM79ShoulderStock, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.FlamethrowerRegulator, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.SparkShotHighVoltageCondenser, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },

            //Row 3.
            { ItemEnumeration.Film_HidingPlace, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 9), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Film_RisingRookie, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Film_Commemorative, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Film_3FLocker, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Film_LionStatue, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.PortableSafe, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.TinStorageBox1, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.TinStorageBox2, new Rectangle(INV_SLOT_WIDTH * itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },

            // Row 4.
            { ItemEnumeration.Detonator, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.ElectronicGadget, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Battery9Volt, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.KeyStorageRoom, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 4), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.JackHandle, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 6), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.SquareCrank, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.MedallionUnicorn, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.KeySpade, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.KeyCardParkingGarage, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.KeyCardWeaponsLocker, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.ValveHandle, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 13), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.STARSBadge, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Scepter, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.RedJewel, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.BejeweledBox, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },

            // Row 5.
            { ItemEnumeration.PlugBishop, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 1), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.PlugRook, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.PlugKing, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.PictureBlock, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.USBDongleKey, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.SpareKey, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.RedBook, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 8), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.StatuesLeftArm, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.StatuesLeftArmWithRedBook, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.MedallionLion, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.KeyDiamond, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.KeyCar, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.MedallionMaiden, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 15), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.PowerPanelPart1, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.PowerPanelPart2, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },

            // Row 6.
            { ItemEnumeration.LoversRelief, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.KeyOrphanage, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.KeyClub, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.KeyHeart, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.USSDigitalVideoCassette, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.TBarValveHandle, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.SignalModulator, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.KeySewers, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 8), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.IDWristbandVisitor1, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.IDWristbandGeneralStaff1, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.IDWristbandSeniorStaff1, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.UpgradeChipGeneralStaff, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.UpgradeChipSeniorStaff, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.FuseMainHall, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 15), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Scissors, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.BoltCutter, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },

            // Row 7.
            { ItemEnumeration.StuffedDoll, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.IDWristbandVisitor2, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 2), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.IDWristbandGeneralStaff2, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.IDWristbandSeniorStaff2, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.LabDigitalVideoCassette, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.DispersalCartridgeEmpty, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.DispersalCartridgeSolution, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.DispersalCartridgeHerbicide, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.JointPlug, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 10), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Trophy1, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 12), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.Trophy2, new Rectangle(INV_SLOT_WIDTH * itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.GearSmall, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.GearLarge, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 14), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { ItemEnumeration.PlugKnight, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 16), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.PlugPawn, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },

            // Row 8.
            { ItemEnumeration.PlugQueen, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 0), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.BoxedElectronicPart1, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 2), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.BoxedElectronicPart2, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.UpgradeChipAdministrator, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 5), INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.IDWristbandAdministrator, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.KeyCourtyard, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.FuseBreakRoom, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.JointPlug2, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.GearLarge2, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },

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
            { ItemEnumeration.WoodenBox1, new Rectangle(INV_SLOT_WIDTH * (itemColumnInc = 9), INV_SLOT_HEIGHT * ++itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { ItemEnumeration.WoodenBox2, new Rectangle(INV_SLOT_WIDTH * ++itemColumnInc, INV_SLOT_HEIGHT * itemRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
        };

        private static int weaponColumnInc = -1;
        private static int weaponRowInc = 8;
        public static IReadOnlyDictionary<Weapon, Rectangle> WeaponToImageTranslation = new Dictionary<Weapon, Rectangle>()
        {
            { new Weapon() { WeaponID = WeaponEnumeration.None, Attachments = AttachmentsFlag.None }, new Rectangle(0, 0, 0, 0) },

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
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 0), INV_SLOT_HEIGHT * ++weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.First }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.Second }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 3), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.Third }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.First | AttachmentsFlag.Third }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 7), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.Second | AttachmentsFlag.Third }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 9), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Matilda, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second | AttachmentsFlag.Third }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_JMB_Hp3, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 12), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_JMB_Hp3, Attachments = AttachmentsFlag.Second }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_JMB_Hp3, Attachments = AttachmentsFlag.Third }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_JMB_Hp3, Attachments = AttachmentsFlag.Second | AttachmentsFlag.Third }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_MUP, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_BroomHc, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },

            // Row 10.
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SLS60, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 0), INV_SLOT_HEIGHT * ++weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SLS60, Attachments = AttachmentsFlag.Third }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_M19, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SLS60, Attachments = AttachmentsFlag.Second }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SLS60, Attachments = AttachmentsFlag.Second | AttachmentsFlag.Third }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Shotgun_W870, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 6), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Shotgun_W870, Attachments = AttachmentsFlag.First }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Shotgun_W870, Attachments = AttachmentsFlag.Second }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 9), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Shotgun_W870, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH* 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.SMG_MQ11, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 12), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.SMG_MQ11, Attachments = AttachmentsFlag.First }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH* 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.SMG_MQ11, Attachments = AttachmentsFlag.Second }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 15), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.SMG_MQ11, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH* 2, INV_SLOT_HEIGHT) },

            // Row 11.
            { new Weapon() { WeaponID = WeaponEnumeration.SMG_LE5_Infinite2, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 0), INV_SLOT_HEIGHT * ++weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_LightningHawk, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_LightningHawk, Attachments = AttachmentsFlag.First }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_LightningHawk, Attachments = AttachmentsFlag.Second }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 4), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_LightningHawk, Attachments = AttachmentsFlag.First | AttachmentsFlag.Second }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.GrenadeLauncher_GM79, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 7), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.GrenadeLauncher_GM79, Attachments = AttachmentsFlag.First }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.ChemicalFlamethrower, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 10), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.ChemicalFlamethrower, Attachments = AttachmentsFlag.First }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 12), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.SparkShot, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 14), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.SparkShot, Attachments = AttachmentsFlag.First }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 16), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },

            // Row 12.
            { new Weapon() { WeaponID = WeaponEnumeration.ATM4, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 0), INV_SLOT_HEIGHT * ++weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.ATM4_Infinite, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 0), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 2), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher_Infinite, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 2), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Minigun, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 4), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Minigun_Infinite, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 4), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.EMF_Visualizer, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 6), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_Quickdraw_Army, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.SMG_LE5_Infinite, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 9), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SamuraiEdge_Infinite, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 11), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SamuraiEdge_AlbertWesker, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 11), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SamuraiEdge_ChrisRedfield, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 11), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Handgun_SamuraiEdge_JillValentine, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 11), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.ATM42, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 14), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher2, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 16), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },


            // Row 13.
            { new Weapon() { WeaponID = WeaponEnumeration.CombatKnife, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 0), INV_SLOT_HEIGHT * ++weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.CombatKnife_Infinite, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Minigun2, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 2), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH * 2, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.ChemicalFlamethrower2, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 4), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.SparkShot2, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.ATM43, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher3, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.Minigun3, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.HandGrenade, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.FlashGrenade, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.ATM44, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 16), INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
            { new Weapon() { WeaponID = WeaponEnumeration.AntiTankRocketLauncher4, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * ++weaponColumnInc, INV_SLOT_HEIGHT * weaponRowInc, INV_SLOT_WIDTH , INV_SLOT_HEIGHT) },

            // Row 14.
            { new Weapon() { WeaponID = WeaponEnumeration.Minigun4, Attachments = AttachmentsFlag.None }, new Rectangle(INV_SLOT_WIDTH * (weaponColumnInc = 4), INV_SLOT_HEIGHT * ++weaponRowInc, INV_SLOT_WIDTH, INV_SLOT_HEIGHT) },
        };




        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (memoryAccess != null)
                        memoryAccess.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~REmake1Memory() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
