using GameOverlay.Drawing;
using GameOverlay.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RE2REmakeSRT
{
    public class OverlayDrawer : DXOverlay, IDisposable
    {
        private SharpDX.Direct2D1.Bitmap inventoryError;
        private SharpDX.Direct2D1.Bitmap inventoryImage;
        private SharpDX.Direct2D1.Bitmap inventoryImagePatch1;

        private static Font consolasBold = null;
        private static SolidBrush darkRedBrush = null;
        private static SolidBrush redBrush = null;
        private static SolidBrush whiteBrush = null;
        private static SolidBrush greyBrush = null;
        private static SolidBrush blackBrush = null;
        private static SolidBrush greenBrush = null;
        private static SolidBrush yellowBrush = null;

        private static SolidBrush backBrush = null;
        private static SolidBrush foreBrush = null;

        public OverlayDrawer(IntPtr windowHook, int invSlotWidth, int invSlotHeight, double invSlotScaling = 0.5d, double desiredDrawRate = 60d) : base(windowHook, desiredDrawRate)
        {
            base.Initialize((OverlayWindow w, Graphics g) =>
            {
                consolasBold = g.CreateFont("Consolas", 8f, true);

                darkRedBrush = g.CreateSolidBrush(139, 0, 0);
                redBrush = g.CreateSolidBrush(255, 0, 0);
                yellowBrush = g.CreateSolidBrush(218, 165, 32); // Goldenrod
                greenBrush = g.CreateSolidBrush(124, 252, 0); // LawnGreen
                whiteBrush = g.CreateSolidBrush(255, 255, 255);
                greyBrush = g.CreateSolidBrush(150, 150, 150);
                blackBrush = g.CreateSolidBrush(0, 0, 0);

                backBrush = g.CreateSolidBrush(60, 60, 60);
                foreBrush = g.CreateSolidBrush(100, 0, 0);

                // Loads the inventory images into memory and scales them if required.
                GenerateImages(g, invSlotWidth, invSlotHeight, invSlotScaling);
            });
        }

        public Task Run(CancellationToken cToken) => base.Run(DirectXOverlay_Paint, cToken);

        private void DirectXOverlay_Paint(OverlayWindow w, Graphics g)
        {
            StatisticsDraw(w, g, 5, 50);
            HealthDraw(w, g, 5, 25);
            InventoryDraw(w, g, 200, 25);
        }

        private void DrawProgressBarDirectX(OverlayWindow w, Graphics g, SolidBrush bgBrush, SolidBrush foreBrush, float x, float y, float width, float height, float value, float maximum = 100)
        {
            // Draw BG.
            g.DrawRectangle(bgBrush, x, y, x + width, y + height, 3f);

            // Draw FG.
            Rectangle foreRect = new Rectangle(
                x,
                y,
                x + ((width * value / maximum)),
                y + (height)
                );
            g.FillRectangle(foreBrush, foreRect);
        }

        private void HealthDraw(OverlayWindow w, Graphics g, int xOffset, int yOffset)
        {
            float fontSize = 26f;

            // Draw health.
            if (Program.gameMemory.PlayerCurrentHealth > 1200 || Program.gameMemory.PlayerCurrentHealth < 0) // Dead?
                g.DrawText(consolasBold, fontSize, redBrush, xOffset, yOffset, "DEAD");
            else if (Program.gameMemory.PlayerCurrentHealth >= 801) // Fine (Green)
                g.DrawText(consolasBold, fontSize, greenBrush, xOffset, yOffset, Program.gameMemory.PlayerCurrentHealth.ToString());
            else if (Program.gameMemory.PlayerCurrentHealth <= 800 && Program.gameMemory.PlayerCurrentHealth >= 361) // Caution (Yellow)
                g.DrawText(consolasBold, fontSize, yellowBrush, xOffset, yOffset, Program.gameMemory.PlayerCurrentHealth.ToString());
            else if (Program.gameMemory.PlayerCurrentHealth <= 360) // Danger (Red)
                g.DrawText(consolasBold, fontSize, redBrush, xOffset, yOffset, Program.gameMemory.PlayerCurrentHealth.ToString());
        }

        private void StatisticsDraw(OverlayWindow w, Graphics g, int xOffset, int yOffset)
        {
            // Additional information and stats.
            // Adjustments for displaying text properly.
            int heightGap = 15;
            int i = -1;

            // IGT Display.
            g.DrawText(consolasBold, 20f, whiteBrush, xOffset + 0, yOffset + (heightGap * ++i), string.Format("{0}", Program.gameMemory.IGTFormattedString));
            yOffset += 5;

            if (Program.programSpecialOptions.Flags.HasFlag(ProgramFlags.Debug))
            {
                ++i;
                g.DrawText(consolasBold, 16f, greyBrush, xOffset + 0, yOffset + (heightGap * ++i), "Raw IGT");
                g.DrawText(consolasBold, 16f, greyBrush, xOffset + 0, yOffset + (heightGap * ++i), "A:" + Program.gameMemory.IGTRunningTimer.ToString("00000000000000000000"));
                g.DrawText(consolasBold, 16f, greyBrush, xOffset + 0, yOffset + (heightGap * ++i), "C:" + Program.gameMemory.IGTCutsceneTimer.ToString("00000000000000000000"));
                g.DrawText(consolasBold, 16f, greyBrush, xOffset + 0, yOffset + (heightGap * ++i), "M:" + Program.gameMemory.IGTMenuTimer.ToString("00000000000000000000"));
                g.DrawText(consolasBold, 16f, greyBrush, xOffset + 0, yOffset + (heightGap * ++i), "P:" + Program.gameMemory.IGTPausedTimer.ToString("00000000000000000000"));
                ++i;
            }

            g.DrawText(consolasBold, 16f, greyBrush, xOffset + 0, yOffset + (heightGap * ++i), string.Format("DA Rank: {0}", Program.gameMemory.Rank));
            g.DrawText(consolasBold, 16f, greyBrush, xOffset + 0, yOffset + (heightGap * ++i), string.Format("DA Score: {0}", Program.gameMemory.RankScore));

            g.DrawText(consolasBold, 16f, redBrush, xOffset + 0, yOffset + (heightGap * ++i), "Enemy HP");
            yOffset += 6;
            foreach (EnemyHP enemyHP in Program.gameMemory.EnemyHealth.Where(a => a.IsAlive).OrderBy(a => a.Percentage).ThenByDescending(a => a.CurrentHP))
            {
                int x = xOffset + 0;
                int y = yOffset + (heightGap * ++i);

                DrawProgressBarDirectX(w, g, backBrush, foreBrush, x, y, 158, heightGap, enemyHP.Percentage * 100f, 100f);
                g.DrawText(consolasBold, 12f, redBrush, x + 5, y, string.Format("{0} {1:P1}", enemyHP.CurrentHP, enemyHP.Percentage));
            }
        }

        private void InventoryDraw(OverlayWindow w, Graphics g, int xOffset, int yOffset)
        {
            foreach (InventoryEntry inv in Program.gameMemory.PlayerInventory)
            {
                if (inv == default || inv.SlotPosition < 0 || inv.SlotPosition > 19 || inv.IsEmptySlot)
                    continue;

                int slotColumn = inv.SlotPosition % 4;
                int slotRow = inv.SlotPosition / 4;
                int imageX = xOffset + (slotColumn * Program.INV_SLOT_WIDTH);
                int imageY = yOffset + (slotRow * Program.INV_SLOT_HEIGHT);
                int textX = imageX + (int)(Program.INV_SLOT_WIDTH * 0.7);
                int textY = imageY + (int)(Program.INV_SLOT_HEIGHT * 0.7);
                bool evenSlotColumn = slotColumn % 2 == 0;
                SolidBrush textBrush = whiteBrush;

                if (inv.Quantity == 0)
                    textBrush = darkRedBrush;

                System.Drawing.Rectangle r;
                SharpDX.Direct2D1.Bitmap b;
                Weapon weapon;
                if (inv.IsItem && Program.ItemToImageTranslation.ContainsKey(inv.ItemID))
                {
                    r = Program.ItemToImageTranslation[inv.ItemID];
                    if (inv.ItemID == ItemEnumeration.OldKey)
                        b = inventoryImagePatch1;
                    else
                        b = inventoryImage;
                }
                else if (inv.IsWeapon && Program.WeaponToImageTranslation.ContainsKey(weapon = new Weapon() { WeaponID = inv.WeaponID, Attachments = inv.Attachments }))
                {
                    r = Program.WeaponToImageTranslation[weapon];
                    b = inventoryImage;
                }
                else
                {
                    r = new System.Drawing.Rectangle(0, 0, Program.INV_SLOT_WIDTH, Program.INV_SLOT_HEIGHT);
                    b = inventoryError;
                }

                // Double-slot item.
                if (b.Size.Width == Program.INV_SLOT_WIDTH * 2)
                {
                    // Shift the quantity text over into the 2nd slot's area.
                    textX += Program.INV_SLOT_WIDTH;
                }

                SharpDX.Mathematics.Interop.RawRectangleF drrf = new SharpDX.Mathematics.Interop.RawRectangleF(imageX, imageY, imageX + r.Width, imageY + r.Height);
                using (SharpDX.Direct2D1.Bitmap croppedBitmap = new SharpDX.Direct2D1.Bitmap(g.GetRenderTarget(), new SharpDX.Size2(r.Width, r.Height), new SharpDX.Direct2D1.BitmapProperties()
                {
                    PixelFormat = new SharpDX.Direct2D1.PixelFormat()
                    {
                        AlphaMode = SharpDX.Direct2D1.AlphaMode.Premultiplied,
                        Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm
                    }
                }))
                {
                    croppedBitmap.CopyFromBitmap(b, new SharpDX.Mathematics.Interop.RawPoint(0, 0), RectangleToRawRectangle(r));
                    g.GetRenderTarget().DrawBitmap(croppedBitmap, drrf, 1f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
                }
                g.DrawText(consolasBold, 18f, textBrush, textX, textY, (inv.Quantity != -1) ? inv.Quantity.ToString() : "∞");
            }
        }



        public SharpDX.Mathematics.Interop.RawRectangle RectangleToRawRectangle(System.Drawing.Rectangle r) => new SharpDX.Mathematics.Interop.RawRectangle(r.Left, r.Top, r.Right, r.Bottom);

        public void GenerateImages(Graphics g, int invSlotWidth, int invSlotHeight, double invSlotScaling = 0.5d)
        {
            try
            {
                // Create error inventory image.
                System.Drawing.Bitmap tempInventoryError = null;
                try
                {
                    // Create a blank bitmap to draw on.
                    tempInventoryError = new System.Drawing.Bitmap(invSlotWidth, invSlotHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    using (System.Drawing.Graphics grp = System.Drawing.Graphics.FromImage(tempInventoryError))
                    {
                        // Draw the bitmap.
                        grp.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 0, 0, 0)), 0, 0, tempInventoryError.Width, tempInventoryError.Height);
                        grp.DrawLine(new System.Drawing.Pen(System.Drawing.Color.FromArgb(150, 255, 0, 0), 3), 0, 0, tempInventoryError.Width, tempInventoryError.Height);
                        grp.DrawLine(new System.Drawing.Pen(System.Drawing.Color.FromArgb(150, 255, 0, 0), 3), tempInventoryError.Width, 0, 0, tempInventoryError.Height);
                    }

                    // Convert the bitmap from GDI Format32bppPArgb to DXGI R8G8B8A8_UNorm Premultiplied.
                    inventoryError = GDIBitmapToSharpDXBitmap(tempInventoryError, g.GetRenderTarget());
                }
                finally
                {
                    // Dispose of the GDI bitmaps.
                    tempInventoryError?.Dispose();
                }

                // Scale and convert the inventory images.
                System.Drawing.Bitmap ui0100_iam_texout = null;
                System.Drawing.Bitmap _40d_texout = null;
                try
                {
                    // Create the bitmap from the byte array.
                    ui0100_iam_texout = Properties.Resources.ui0100_iam_texout;
                    _40d_texout = Properties.Resources._40d_texout;

                    // Scale the bitmap.
                    if (Program.programSpecialOptions.ScalingFactor != 1d)
                    {
                        ui0100_iam_texout = new System.Drawing.Bitmap(ui0100_iam_texout, (int)Math.Round(ui0100_iam_texout.Width * Program.programSpecialOptions.ScalingFactor, MidpointRounding.AwayFromZero), (int)Math.Round(ui0100_iam_texout.Height * Program.programSpecialOptions.ScalingFactor, MidpointRounding.AwayFromZero));
                        _40d_texout = new System.Drawing.Bitmap(_40d_texout, (int)Math.Round(_40d_texout.Width * Program.programSpecialOptions.ScalingFactor, MidpointRounding.AwayFromZero), (int)Math.Round(_40d_texout.Height * Program.programSpecialOptions.ScalingFactor, MidpointRounding.AwayFromZero));
                    }

                    // Transform the bitmap into a pre-multiplied alpha.
                    ui0100_iam_texout = ui0100_iam_texout.Clone(new System.Drawing.Rectangle(0, 0, ui0100_iam_texout.Width, ui0100_iam_texout.Height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    _40d_texout = _40d_texout.Clone(new System.Drawing.Rectangle(0, 0, _40d_texout.Width, _40d_texout.Height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                    // Convert the bitmap from GDI Format32bppPArgb to DXGI R8G8B8A8_UNorm Premultiplied.
                    inventoryImage = GDIBitmapToSharpDXBitmap(ui0100_iam_texout, g.GetRenderTarget());
                    inventoryImagePatch1 = GDIBitmapToSharpDXBitmap(_40d_texout, g.GetRenderTarget());
                }
                finally
                {
                    // Dispose of the GDI bitmaps.
                    ui0100_iam_texout?.Dispose();
                    _40d_texout?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Program.FailFast(Program.GetExceptionMessage(ex), ex);
            }
        }

        private SharpDX.Direct2D1.Bitmap GDIBitmapToSharpDXBitmap(System.Drawing.Bitmap bitmap, SharpDX.Direct2D1.RenderTarget device)
        {
            System.Drawing.Rectangle sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            SharpDX.Direct2D1.BitmapProperties bitmapProperties = new SharpDX.Direct2D1.BitmapProperties(new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied));
            SharpDX.Size2 size = new SharpDX.Size2(bitmap.Width, bitmap.Height);

            // Transform pixels from GDI's wild ass BGRA to DXGI-compatible RGBA.
            int stride = bitmap.Width * sizeof(int);
            using (SharpDX.DataStream pixelStream = new SharpDX.DataStream(bitmap.Height * stride, true, true))
            {
                // Lock the source bitmap.
                System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(sourceArea, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                // Convert each pixel.
                for (int y = 0; y < bitmap.Height; ++y)
                {
                    int offset = bitmapData.Stride * y;
                    for (int x = 0; x < bitmap.Width; ++x)
                    {
                        byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);

                        int rgba = R | (G << 8) | (B << 16) | (A << 24);
                        pixelStream.Write(rgba);
                    }

                }

                // Unlock source bitmap now.
                bitmap.UnlockBits(bitmapData);

                // Reset stream position for reading.
                pixelStream.Position = 0;

                // Create the SharpDX bitmap from the DataStream.
                return new SharpDX.Direct2D1.Bitmap(device, size, pixelStream, stride, bitmapProperties);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual new void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    base.Dispose();
                    inventoryError?.Dispose();
                    inventoryImage?.Dispose();
                    inventoryImagePatch1?.Dispose();
                    consolasBold?.Dispose();
                    darkRedBrush?.Dispose();
                    redBrush?.Dispose();
                    whiteBrush?.Dispose();
                    greyBrush?.Dispose();
                    blackBrush?.Dispose();
                    greenBrush?.Dispose();
                    yellowBrush?.Dispose();
                    backBrush?.Dispose();
                    foreBrush?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~OverlayDrawer()
        // {
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
