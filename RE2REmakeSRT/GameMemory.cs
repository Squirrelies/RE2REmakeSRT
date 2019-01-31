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
                        PointerRank = new MultilevelPointer(memoryAccess, BaseAddress + 0x0707DF80, 0x168, 0x40, 0xF0, 0x138, 0x20);
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

            // Player HP
            PlayerMaxHealth = PointerPlayerHP.DerefInt(0x54);
            PlayerCurrentHealth = PointerPlayerHP.DerefInt(0x58);

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
            // Inventory
            for (int i = 0; i < PointerInventoryEntries.Length; ++i)
                PlayerInventory[i] = new InventoryEntry(PointerInventoryEntries[i].DerefByteArray(-0x90, 0xF0));

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
