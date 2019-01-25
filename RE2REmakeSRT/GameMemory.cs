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

        // Pointers
        public ulong BaseAddress { get; private set;}
        public uint PointerPlayer1 { get; private set; }
        public uint PointerPlayer2 { get; private set; }
        public uint PointerPlayerHP { get; private set; }

        // Values
        public int CurrentHealth { get; private set; }
        public float RawInGameTimer { get; private set; }

        public TimeSpan InGameTimer { get { return TimeSpan.FromSeconds(RawInGameTimer); } }
        public string InGameTimerString { get { return this.InGameTimer.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture); } }

        public GameMemory(Process proc)
        {
            this.proc = proc;
            this.gameVersion = REmake2VersionDetector.GetVersion(this.proc);
            this.memoryAccess = new ProcessMemory.ProcessMemory(this.proc.Id);
            this.BaseAddress = (ulong)this.proc.MainModule.BaseAddress.ToInt64();

            this.CurrentHealth = 0;
            this.RawInGameTimer = 0f;
        }

        /// <summary>
        /// This call refreshes everything. This should be used less often. Inventory rendering can be more expensive and doesn't change as often.
        /// </summary>
        /// <param name="cToken"></param>
        public async Task Refresh(CancellationToken cToken)
        {
            switch (this.gameVersion)
            {
                case REmake2VersionEnumeration.Stock_1p00:
                case REmake2VersionEnumeration.Stock_1p01:
                    {
                        // Perform slim lookups first.
                        await RefreshSlim(cToken);

                        // Now lookup the rest.

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
        public async Task RefreshSlim(CancellationToken cToken)
        {
            switch (this.gameVersion)
            {
                case REmake2VersionEnumeration.Stock_1p00:
                case REmake2VersionEnumeration.Stock_1p01:
                    {
                        this.PointerPlayer1 = await memoryAccess.GetUIntAtAsync(this.BaseAddress + 0x070ACA88U, cToken);
                        this.PointerPlayer2 = await memoryAccess.GetUIntAtAsync(this.PointerPlayer1 + 0x50U, cToken);
                        this.PointerPlayerHP = await memoryAccess.GetUIntAtAsync(this.PointerPlayer2 + 0x20U, cToken);

                        this.CurrentHealth = await memoryAccess.GetIntAtAsync(this.PointerPlayerHP + 0x58U, cToken);

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
