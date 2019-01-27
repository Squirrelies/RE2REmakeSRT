using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
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
        public uint PointerPlayer3 { get; private set; }
        public uint PointerPlayer4 { get; private set; }
        public uint PointerPlayer5 { get; private set; }

        public uint PointerPlayerHP { get; private set; }
        public uint PointerPlayerWeapon1 { get; private set; }
        public uint PointerPlayerQuantity1 { get; private set; }

        // Values
        public int PlayerCurrentHealth { get; private set; }
        public int PlayerMaxHealth { get; private set; }
        public int BossCurrentHealth { get; private set; }
        public int BossMaxHealth { get; private set; }
        public float RawInGameTimer { get; private set; }

        public TimeSpan InGameTimer { get { return TimeSpan.FromSeconds(RawInGameTimer); } }
        //public string InGameTimerString { get { return this.InGameTimer.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture); } }
        public string InGameTimerString { get; private set; }

        public GameMemory(Process proc)
        {
            this.proc = proc;
            this.gameVersion = REmake2VersionDetector.GetVersion(this.proc);
            this.memoryAccess = new ProcessMemory.ProcessMemory(this.proc.Id);
            this.BaseAddress = (ulong)this.proc.MainModule.BaseAddress.ToInt64();

            this.PlayerCurrentHealth = 0;
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
                        this.PointerPlayer3 = await memoryAccess.GetUIntAtAsync(this.PointerPlayer2 + 0xA0U, cToken); // Alt 0x98U
                        this.PointerPlayer4 = await memoryAccess.GetUIntAtAsync(this.PointerPlayer3 + 0x18U, cToken); // Alt 0x78U
                        this.PointerPlayer5 = await memoryAccess.GetUIntAtAsync(this.PointerPlayer4 + 0x10U, cToken);

                        this.PointerPlayerWeapon1 = await memoryAccess.GetUIntAtAsync(this.PointerPlayer5 + 0x14U, cToken);
                        this.PointerPlayerQuantity1 = await memoryAccess.GetUIntAtAsync(this.PointerPlayer5 + 0x20U, cToken);

                        uint bossPtr1 = await memoryAccess.GetUIntAtAsync(this.BaseAddress + 0x0707B758U, cToken);
                        uint bossPtr2 = await memoryAccess.GetUIntAtAsync(bossPtr1 + 0x80U, cToken);
                        uint bossPtr3 = await memoryAccess.GetUIntAtAsync(bossPtr2 + 0x88U, cToken);
                        uint bossPtr4 = await memoryAccess.GetUIntAtAsync(bossPtr3 + 0x18U, cToken);
                        uint bossPtr5 = await memoryAccess.GetUIntAtAsync(bossPtr4 + 0x1A0U, cToken);

                        this.BossMaxHealth = await memoryAccess.GetIntAtAsync(bossPtr5 + 0x54U, cToken);
                        this.BossCurrentHealth = await memoryAccess.GetIntAtAsync(bossPtr5 + 0x58U, cToken);

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

                        this.PlayerMaxHealth = await memoryAccess.GetIntAtAsync(this.PointerPlayerHP + 0x54U, cToken);
                        this.PlayerCurrentHealth = await memoryAccess.GetIntAtAsync(this.PointerPlayerHP + 0x58U, cToken);

                        // Temporary. This is the rendered string in the pause menu and is only updated when paused... Searching for the backing value that is the basis for that string still...
                        uint test1 = await memoryAccess.GetUIntAtAsync(this.BaseAddress + 0x0707E130U, cToken);
                        uint test2 = await memoryAccess.GetUIntAtAsync(test1 + 0x80U, cToken);
                        uint test3 = await memoryAccess.GetUIntAtAsync(test2 + 0x110U, cToken);
                        uint test4 = await memoryAccess.GetUIntAtAsync(test3 + 0x58U, cToken);
                        byte[] igtStringBytes = await memoryAccess.GetByteArrayAtAsync(test4, 8 * 2, cToken);
                        InGameTimerString = Encoding.Unicode.GetString(igtStringBytes, 0, 8 * 2);

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
