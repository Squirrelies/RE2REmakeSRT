using ProcessMemory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RE2REmakeSRT
{
    public class GameMemory : IDisposable
    {
        // Private Variables
        private Process proc;
        private REmake2VersionEnumeration gameVersion;
        public ProcessMemory.ProcessMemory memoryAccess;
        private const string IGT_TIMESPAN_STRING_FORMAT = @"hh\:mm\:ss\.fff";

        // Pointers
        public long BaseAddress { get; private set; }
        public MultilevelPointer PointerIGT { get; private set; }
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


        // Values
        public int PlayerCurrentHealth { get; private set; }
        public int PlayerMaxHealth { get; private set; }
        public InventoryEntry[] PlayerInventory { get; private set; }
        public int BossCurrentHealth { get; private set; }
        public int BossMaxHealth { get; private set; }

        public ulong IGTRunningTimer { get; private set; }
        public ulong IGTCutsceneTimer { get; private set; }
        public ulong IGTMenuTimer { get; private set; }
        public ulong IGTPausedTimer { get; private set; }

        public ulong IGTRaw => this.IGTRunningTimer - this.IGTCutsceneTimer - this.IGTPausedTimer;
        public double IGTCalculated => this.IGTRaw / 1000d;
        public TimeSpan IGTTimeSpan
        {
            get
            {
                TimeSpan timespanIGT;

                if (this.IGTCalculated <= TimeSpan.MaxValue.TotalMilliseconds)
                    timespanIGT = TimeSpan.FromMilliseconds(this.IGTCalculated);
                else
                    timespanIGT = new TimeSpan();

                return timespanIGT;
            }
        }
        public string IGTString => this.IGTTimeSpan.ToString(IGT_TIMESPAN_STRING_FORMAT, CultureInfo.InvariantCulture);

        public GameMemory(Process proc)
        {
            this.proc = proc;
            this.gameVersion = REmake2VersionDetector.GetVersion(this.proc);
            this.memoryAccess = new ProcessMemory.ProcessMemory(this.proc.Id);
            this.BaseAddress = this.proc.MainModule.BaseAddress.ToInt64();
            this.PointerIGT = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACAE0, 0x2E0, 0x218, 0x610, 0x710, 0x60);
            this.PointerPlayerHP = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x20);
            this.PointerEnemyHP = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x0707B758, 0x80, 0x88, 0x18, 0x1A0);
            this.PointerInventoryEntry1 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x20, 0x18, 0x10);
            this.PointerInventoryEntry2 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x28, 0x18, 0x10);
            this.PointerInventoryEntry3 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x30, 0x18, 0x10);
            this.PointerInventoryEntry4 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x38, 0x18, 0x10);
            this.PointerInventoryEntry5 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x40, 0x18, 0x10);
            this.PointerInventoryEntry6 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x48, 0x18, 0x10);
            this.PointerInventoryEntry7 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x50, 0x18, 0x10);
            this.PointerInventoryEntry8 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x58, 0x18, 0x10);
            this.PointerInventoryEntry9 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x60, 0x18, 0x10);
            this.PointerInventoryEntry10 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x68, 0x18, 0x10);
            this.PointerInventoryEntry11 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x70, 0x18, 0x10);
            this.PointerInventoryEntry12 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x78, 0x18, 0x10);
            this.PointerInventoryEntry13 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x80, 0x18, 0x10);
            this.PointerInventoryEntry14 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x88, 0x18, 0x10);
            this.PointerInventoryEntry15 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x90, 0x18, 0x10);
            this.PointerInventoryEntry16 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0x98, 0x18, 0x10);
            this.PointerInventoryEntry17 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0xA0, 0x18, 0x10);
            this.PointerInventoryEntry18 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0xA8, 0x18, 0x10);
            this.PointerInventoryEntry19 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0xB0, 0x18, 0x10);
            this.PointerInventoryEntry20 = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x070ACA88, 0x50, 0x98, 0x10, 0xB8, 0x18, 0x10);

            this.PlayerCurrentHealth = 0;
            this.PlayerMaxHealth = 0;
            this.PlayerInventory = new InventoryEntry[20];
            this.BossCurrentHealth = 0;
            this.BossMaxHealth = 0;
        }

        public void UpdatePointers()
        {
            switch (this.gameVersion)
            {
                case REmake2VersionEnumeration.Stock_1p00:
                case REmake2VersionEnumeration.Stock_1p01:
                    {
                        this.PointerIGT.UpdatePointers();
                        this.PointerPlayerHP.UpdatePointers();
                        this.PointerEnemyHP.UpdatePointers();

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// This call refreshes impotant variables such as IGT, HP and Ammo.
        /// </summary>
        /// <param name="cToken"></param>
        public void RefreshSlim()
        {
            switch (this.gameVersion)
            {
                case REmake2VersionEnumeration.Stock_1p00:
                case REmake2VersionEnumeration.Stock_1p01:
                    {
                        // IGT
                        this.IGTRunningTimer = this.PointerIGT.DerefULong(0x18);
                        this.IGTCutsceneTimer = this.PointerIGT.DerefULong(0x20);
                        this.IGTMenuTimer = this.PointerIGT.DerefULong(0x28);
                        this.IGTPausedTimer = this.PointerIGT.DerefULong(0x30);

                        // Player HP
                        this.PlayerMaxHealth = this.PointerPlayerHP.DerefInt(0x54);
                        this.PlayerCurrentHealth = this.PointerPlayerHP.DerefInt(0x58);

                        // Boss HP
                        this.BossMaxHealth = this.PointerEnemyHP.DerefInt(0x58);
                        this.BossCurrentHealth = this.PointerEnemyHP.DerefInt(0x54);

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// This call refreshes everything. This should be used less often. Inventory rendering can be more expensive and doesn't change as often.
        /// </summary>
        /// <param name="cToken"></param>
        public void Refresh()
        {
            switch (this.gameVersion)
            {
                case REmake2VersionEnumeration.Stock_1p00:
                case REmake2VersionEnumeration.Stock_1p01:
                    {
                        // Perform slim lookups first.
                        RefreshSlim();

                        // Other lookups that don't need to update as often.
                        // Inventory
                        PlayerInventory[0] = new InventoryEntry(this.PointerInventoryEntry1.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[1] = new InventoryEntry(this.PointerInventoryEntry2.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[2] = new InventoryEntry(this.PointerInventoryEntry3.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[3] = new InventoryEntry(this.PointerInventoryEntry4.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[4] = new InventoryEntry(this.PointerInventoryEntry5.DerefByteArray(-0x90, 0xF0));

                        PlayerInventory[5] = new InventoryEntry(this.PointerInventoryEntry6.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[6] = new InventoryEntry(this.PointerInventoryEntry7.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[7] = new InventoryEntry(this.PointerInventoryEntry8.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[8] = new InventoryEntry(this.PointerInventoryEntry9.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[9] = new InventoryEntry(this.PointerInventoryEntry10.DerefByteArray(-0x90, 0xF0));

                        PlayerInventory[10] = new InventoryEntry(this.PointerInventoryEntry11.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[11] = new InventoryEntry(this.PointerInventoryEntry12.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[12] = new InventoryEntry(this.PointerInventoryEntry13.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[13] = new InventoryEntry(this.PointerInventoryEntry14.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[14] = new InventoryEntry(this.PointerInventoryEntry15.DerefByteArray(-0x90, 0xF0));

                        PlayerInventory[15] = new InventoryEntry(this.PointerInventoryEntry16.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[16] = new InventoryEntry(this.PointerInventoryEntry17.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[17] = new InventoryEntry(this.PointerInventoryEntry18.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[18] = new InventoryEntry(this.PointerInventoryEntry19.DerefByteArray(-0x90, 0xF0));
                        PlayerInventory[19] = new InventoryEntry(this.PointerInventoryEntry20.DerefByteArray(-0x90, 0xF0));

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

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
                    if (this.memoryAccess != null)
                        this.memoryAccess.Dispose();
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
