using ProcessMemory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
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
        public long BaseAddress { get; private set;}
        public MultilevelPointer PointerIGT { get; private set; }
        public MultilevelPointer PointerPlayerHP { get; private set; }
        public MultilevelPointer PointerBossHP { get; private set; }

        // Values
        public int PlayerCurrentHealth { get; private set; }
        public int PlayerMaxHealth { get; private set; }
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
            this.PointerBossHP = new MultilevelPointer(memoryAccess, this.BaseAddress + 0x0707B758, 0x80, 0x88, 0x18, 0x1A0);

            this.PlayerCurrentHealth = 0;
            this.PlayerMaxHealth = 0;
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
                        this.PointerBossHP.UpdatePointers();

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
                        this.BossMaxHealth = this.PointerBossHP.DerefInt(0x58);
                        this.BossCurrentHealth = this.PointerBossHP.DerefInt(0x54);

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
                        // ...

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
