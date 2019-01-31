using ProcessMemory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

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
        public MultilevelPointer PointerEnemyHP { get; private set; }
        public MultilevelPointer PointerInventoryEntry1 { get; private set; }
        public MultilevelPointer PointerInventoryEntry2 { get; private set; }
        public MultilevelPointer PointerInventoryEntry3 { get; private set; }
        public MultilevelPointer PointerInventoryEntry4 { get; private set; }
        public MultilevelPointer PointerInventoryEntry5 { get; private set; }
        public MultilevelPointer PointerInventoryEntry6 { get; private set; }
        public MultilevelPointer PointerInventoryEntry7 { get; private set; }
        public MultilevelPointer PointerInventoryEntry8 { get; private set; }
        public MultilevelPointer PointerInventoryEntry9 { get; private set; }
        public MultilevelPointer PointerInventoryEntry10 { get; private set; }
        public MultilevelPointer PointerInventoryEntry11 { get; private set; }
        public MultilevelPointer PointerInventoryEntry12 { get; private set; }
        public MultilevelPointer PointerInventoryEntry13 { get; private set; }
        public MultilevelPointer PointerInventoryEntry14 { get; private set; }
        public MultilevelPointer PointerInventoryEntry15 { get; private set; }
        public MultilevelPointer PointerInventoryEntry16 { get; private set; }
        public MultilevelPointer PointerInventoryEntry17 { get; private set; }
        public MultilevelPointer PointerInventoryEntry18 { get; private set; }
        public MultilevelPointer PointerInventoryEntry19 { get; private set; }
        public MultilevelPointer PointerInventoryEntry20 { get; private set; }

        // Public Properties
        public int PlayerCurrentHealth { get; private set; }
        public int PlayerMaxHealth { get; private set; }
        public InventoryEntry[] PlayerInventory { get; private set; }
        public int BossCurrentHealth { get; private set; }
        public int BossMaxHealth { get; private set; }
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
                        PointerRank = new MultilevelPointer(memoryAccess, BaseAddress + 0x0707DF80, 0x168, 0x40, 0xF0, 0x138, 0x20);
                        PointerPlayerHP = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x20);
                        PointerEnemyHP = new MultilevelPointer(memoryAccess, BaseAddress + 0x0707B758, 0x80, 0x88, 0x18, 0x1A0);
                        PointerInventoryEntry1 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x20, 0x18, 0x10);
                        PointerInventoryEntry2 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x28, 0x18, 0x10);
                        PointerInventoryEntry3 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x30, 0x18, 0x10);
                        PointerInventoryEntry4 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x38, 0x18, 0x10);
                        PointerInventoryEntry5 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x40, 0x18, 0x10);
                        PointerInventoryEntry6 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x48, 0x18, 0x10);
                        PointerInventoryEntry7 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x50, 0x18, 0x10);
                        PointerInventoryEntry8 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x58, 0x18, 0x10);
                        PointerInventoryEntry9 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x60, 0x18, 0x10);
                        PointerInventoryEntry10 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x68, 0x18, 0x10);
                        PointerInventoryEntry11 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x70, 0x18, 0x10);
                        PointerInventoryEntry12 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x78, 0x18, 0x10);
                        PointerInventoryEntry13 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x80, 0x18, 0x10);
                        PointerInventoryEntry14 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x88, 0x18, 0x10);
                        PointerInventoryEntry15 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x90, 0x18, 0x10);
                        PointerInventoryEntry16 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x98, 0x18, 0x10);
                        PointerInventoryEntry17 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0xA0, 0x18, 0x10);
                        PointerInventoryEntry18 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0xA8, 0x18, 0x10);
                        PointerInventoryEntry19 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0xB0, 0x18, 0x10);
                        PointerInventoryEntry20 = new MultilevelPointer(memoryAccess, BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0xB8, 0x18, 0x10);

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
            BossCurrentHealth = 0;
            BossMaxHealth = 0;
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
            PointerPlayerHP.UpdatePointers();
            PointerEnemyHP.UpdatePointers();
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

            // Player HP
            PlayerMaxHealth = PointerPlayerHP.DerefInt(0x54);
            PlayerCurrentHealth = PointerPlayerHP.DerefInt(0x58);

            // Boss HP
            BossMaxHealth = PointerEnemyHP.DerefInt(0x58);
            BossCurrentHealth = PointerEnemyHP.DerefInt(0x54);
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
            // Inventory
            PlayerInventory[0] = new InventoryEntry(PointerInventoryEntry1.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[1] = new InventoryEntry(PointerInventoryEntry2.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[2] = new InventoryEntry(PointerInventoryEntry3.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[3] = new InventoryEntry(PointerInventoryEntry4.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[4] = new InventoryEntry(PointerInventoryEntry5.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[5] = new InventoryEntry(PointerInventoryEntry6.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[6] = new InventoryEntry(PointerInventoryEntry7.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[7] = new InventoryEntry(PointerInventoryEntry8.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[8] = new InventoryEntry(PointerInventoryEntry9.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[9] = new InventoryEntry(PointerInventoryEntry10.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[10] = new InventoryEntry(PointerInventoryEntry11.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[11] = new InventoryEntry(PointerInventoryEntry12.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[12] = new InventoryEntry(PointerInventoryEntry13.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[13] = new InventoryEntry(PointerInventoryEntry14.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[14] = new InventoryEntry(PointerInventoryEntry15.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[15] = new InventoryEntry(PointerInventoryEntry16.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[16] = new InventoryEntry(PointerInventoryEntry17.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[17] = new InventoryEntry(PointerInventoryEntry18.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[18] = new InventoryEntry(PointerInventoryEntry19.DerefByteArray(-0x90, 0xF0));
            PlayerInventory[19] = new InventoryEntry(PointerInventoryEntry20.DerefByteArray(-0x90, 0xF0));

            // Rank
            Rank = PointerRank.DerefInt(0x58);
            RankScore = PointerRank.DerefFloat(0x5C);
        }

        /// <summary>
        /// 
        /// </summary>
        public static IReadOnlyDictionary<ItemEnumeration, Bitmap> ItemToImageTranslation = new Dictionary<ItemEnumeration, Bitmap>()
        {
            //{ (ItemEnumeration)0x00, Program.emptySlot }, // Does not exist!
            //{ (ItemEnumeration)0x01, Properties.Resources._001 },
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
