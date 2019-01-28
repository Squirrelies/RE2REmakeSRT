using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        public uint PointerIGT { get; private set; }
        public uint PointerPlayerHP { get; private set; }
        public uint PointerBossHP { get; private set; }

        // Values
        public int PlayerCurrentHealth { get; private set; }
        public int PlayerMaxHealth { get; private set; }
        public int BossCurrentHealth { get; private set; }
        public int BossMaxHealth { get; private set; }

        public ulong IGTRunningTimer { get; private set; }
        public ulong IGTCutsceneTimer { get; private set; }
        public ulong IGTMenuTimer { get; private set; }
        public ulong IGTPausedTimer { get; private set; }

        public GameMemory(Process proc)
        {
            this.proc = proc;
            this.gameVersion = REmake2VersionDetector.GetVersion(this.proc);
            this.memoryAccess = new ProcessMemory.ProcessMemory(this.proc.Id);
            this.BaseAddress = (ulong)this.proc.MainModule.BaseAddress.ToInt64();
            this.PointerIGT = 0U;
            this.PointerPlayerHP = 0U;
            this.PointerBossHP = 0U;

            this.PlayerCurrentHealth = 0;
            this.PlayerMaxHealth = 0;
            this.BossCurrentHealth = 0;
            this.BossMaxHealth = 0;
        }

        public async Task UpdatePointers(CancellationToken cToken)
        {
            switch (this.gameVersion)
            {
                case REmake2VersionEnumeration.Stock_1p00:
                case REmake2VersionEnumeration.Stock_1p01:
                    {
                        uint igt1 = await memoryAccess.GetUIntAtAsync(this.BaseAddress + 0x070ACAE0U, cToken);
                        uint igt2 = await memoryAccess.GetUIntAtAsync(igt1 + 0x2E0U, cToken);
                        uint igt3 = await memoryAccess.GetUIntAtAsync(igt2 + 0x218U, cToken);
                        uint igt4 = await memoryAccess.GetUIntAtAsync(igt3 + 0x610U, cToken);
                        uint igt5 = await memoryAccess.GetUIntAtAsync(igt4 + 0x710U, cToken);
                        this.PointerIGT = await memoryAccess.GetUIntAtAsync(igt5 + 0x60U, cToken);

                        uint playerPtr1 = await memoryAccess.GetUIntAtAsync(this.BaseAddress + 0x070ACA88U, cToken);
                        uint playerPtr2 = await memoryAccess.GetUIntAtAsync(playerPtr1 + 0x50U, cToken);
                        this.PointerPlayerHP = await memoryAccess.GetUIntAtAsync(playerPtr2 + 0x20U, cToken);

                        uint bossPtr1 = await memoryAccess.GetUIntAtAsync(this.BaseAddress + 0x0707B758U, cToken);
                        uint bossPtr2 = await memoryAccess.GetUIntAtAsync(bossPtr1 + 0x80U, cToken);
                        uint bossPtr3 = await memoryAccess.GetUIntAtAsync(bossPtr2 + 0x88U, cToken);
                        uint bossPtr4 = await memoryAccess.GetUIntAtAsync(bossPtr3 + 0x18U, cToken);
                        this.PointerBossHP = await memoryAccess.GetUIntAtAsync(bossPtr4 + 0x1A0U, cToken);

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
                        // IGT
                        this.IGTRunningTimer = await memoryAccess.GetULongAtAsync(this.PointerIGT + 0x18U, cToken);
                        this.IGTCutsceneTimer = await memoryAccess.GetULongAtAsync(this.PointerIGT + 0x20U, cToken);
                        this.IGTMenuTimer = await memoryAccess.GetULongAtAsync(this.PointerIGT + 0x28U, cToken);
                        this.IGTPausedTimer = await memoryAccess.GetULongAtAsync(this.PointerIGT + 0x30U, cToken);

                        // Player HP
                        this.PlayerMaxHealth = await memoryAccess.GetIntAtAsync(this.PointerPlayerHP + 0x54U, cToken);
                        this.PlayerCurrentHealth = await memoryAccess.GetIntAtAsync(this.PointerPlayerHP + 0x58U, cToken);

                        // Boss HP
                        this.BossMaxHealth = await memoryAccess.GetIntAtAsync(this.PointerBossHP + 0x54U, cToken);
                        this.BossCurrentHealth = await memoryAccess.GetIntAtAsync(this.PointerBossHP + 0x58U, cToken);

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
        public async Task Refresh(CancellationToken cToken)
        {
            switch (this.gameVersion)
            {
                case REmake2VersionEnumeration.Stock_1p00:
                case REmake2VersionEnumeration.Stock_1p01:
                    {
                        // Perform slim lookups first.
                        await RefreshSlim(cToken);

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
