using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RE2REmakeSRT
{
    public partial class MainUI : Form
    {
        // How often to perform a full-UI redraw. 200 Milliseconds.
        public const long FULL_UI_DRAW_TICKS = TimeSpan.TicksPerMillisecond * 200L;

        private System.Timers.Timer memoryPollingTimer;
        private long lastFullUIDraw;
        private SmoothingMode smoothingMode = SmoothingMode.HighSpeed;
        private PixelOffsetMode pixelOffsetMode = PixelOffsetMode.HighSpeed;
        private CompositingQuality compositingQuality = CompositingQuality.HighSpeed;
        private CompositingMode compositingMode = CompositingMode.SourceOver;
        private InterpolationMode interpolationMode = InterpolationMode.Low;

        public MainUI()
        {
            InitializeComponent();
            lastFullUIDraw = DateTime.UtcNow.Ticks;
            memoryPollingTimer = new System.Timers.Timer() { AutoReset = false, Interval = 15 };
            memoryPollingTimer.Elapsed += MemoryPollingTimer_Elapsed;
            memoryPollingTimer.Start();
        }

        private void MemoryPollingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                // Suspend UI layout logic to perform redrawing.
                MainUI uiForm = (MainUI)Program.mainContext.MainForm;

                // Only draw occasionally, not as often as the stats panel.
                if (DateTime.UtcNow.Ticks - lastFullUIDraw >= FULL_UI_DRAW_TICKS)
                {
                    // Update the last drawn time.
                    lastFullUIDraw = DateTime.UtcNow.Ticks;

                    // Get the full amount of updated information from memory.
                    Task.WaitAll(Program.gameMem.Refresh(CancellationToken.None));

                    // Output some info to debug listeners.
                    //Debug.WriteLine("IGT: {0} {1}'s Health: {2} Poisoned: {3} {4} ({5} / {6})", Program.gameMem.InGameTimerString, Program.gameMem.CurrentCharacter, Program.gameMem.CurrentHealth, Program.gameMem.IsPoisoned, Program.gameMem.ItemSlots[Program.gameMem.EquippedItemSlotIndex].Item, Program.gameMem.EquippedCurrentAmmo, Program.gameMem.EquippedMaxAmmo);

                    // Only draw these periodically to reduce CPU usage.
                    uiForm.playerHealthStatus.Invalidate();
                    //uiForm.playerInfoPanel.Invalidate();
                    //uiForm.inventoryPanel.Invalidate();
                }
                else
                {
                    // Get a slimmed-down amount of updated information from memory.
                    Task.WaitAll(Program.gameMem.RefreshSlim(CancellationToken.None));
                }

                // Always draw this as these are simple text draws and contains the IGT/frame count.
                uiForm.statisticsPanel.Invalidate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[{0}] {1}\r\n{2}", ex.GetType().ToString(), ex.Message, ex.StackTrace);
            }
            finally
            {
                // Trigger the timer to start once again.
                ((System.Timers.Timer)sender).Start();
            }
        }

        private void playerHealthStatus_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = smoothingMode;
            e.Graphics.CompositingQuality = compositingQuality;
            e.Graphics.CompositingMode = compositingMode;
            e.Graphics.InterpolationMode = interpolationMode;
            e.Graphics.PixelOffsetMode = pixelOffsetMode;

            // Draw health.
            if (Program.gameMem.PlayerCurrentHealth > 1200 || Program.gameMem.PlayerCurrentHealth < 0) // Dead?
            {
                e.Graphics.DrawText(15, 37, "DEAD", Brushes.Red);
                playerHealthStatus.ThreadSafeSetHealthImage(Properties.Resources.EMPTY, "EMPTY");
            }
            else if (Program.gameMem.PlayerCurrentHealth >= 801) // Fine (Green)
            {
                e.Graphics.DrawText(15, 37, Program.gameMem.PlayerCurrentHealth.ToString(), Brushes.LawnGreen);
                playerHealthStatus.ThreadSafeSetHealthImage(Properties.Resources.FINE, "FINE");
            }
            else if (Program.gameMem.PlayerCurrentHealth <= 800 && Program.gameMem.PlayerCurrentHealth >= 361) // Caution (Yellow)
            {
                e.Graphics.DrawText(15, 37, Program.gameMem.PlayerCurrentHealth.ToString(), Brushes.Goldenrod);
                playerHealthStatus.ThreadSafeSetHealthImage(Properties.Resources.CAUTION_YELLOW, "CAUTION_YELLOW");
            }
            else if (Program.gameMem.PlayerCurrentHealth <= 360) // Danger (Red)
            {
                e.Graphics.DrawText(15, 37, Program.gameMem.PlayerCurrentHealth.ToString(), Brushes.Red);
                playerHealthStatus.ThreadSafeSetHealthImage(Properties.Resources.DANGER, "DANGER");
            }

            //if (Program.gameMem.CurrentCharacter == CharacterEnumeration.Chris)
            //{
            //    if (Program.gameMem.CurrentHealth > 1400 || Program.gameMem.CurrentHealth < 0) // Dead?
            //    {
            //        e.Graphics.DrawText(15, 37, "DEAD", Brushes.Red);
            //        playerHealthStatus.ThreadSafeSetHealthImage(Properties.Resources.EMPTY, "EMPTY");
            //    }
            //    else if (Program.gameMem.CurrentHealth >= 1050) // Fine (Green)
            //    {
            //        e.Graphics.DrawText(15, 37, Program.gameMem.CurrentHealth.ToString(), Brushes.LawnGreen);
            //        playerHealthStatus.ThreadSafeSetHealthImage((!Program.gameMem.IsPoisoned) ? Properties.Resources.FINE : Properties.Resources.POISON, (!Program.gameMem.IsPoisoned) ? "FINE" : "POISON");
            //    }
            //    else if (Program.gameMem.CurrentHealth <= 1049 && Program.gameMem.CurrentHealth >= 700) // Caution (Yellow)
            //    {
            //        e.Graphics.DrawText(15, 37, Program.gameMem.CurrentHealth.ToString(), Brushes.Goldenrod);
            //        playerHealthStatus.ThreadSafeSetHealthImage((!Program.gameMem.IsPoisoned) ? Properties.Resources.CAUTION_YELLOW : Properties.Resources.POISON, (!Program.gameMem.IsPoisoned) ? "CAUTION_YELLOW" : "POISON");
            //    }
            //    else if (Program.gameMem.CurrentHealth <= 699 && Program.gameMem.CurrentHealth >= 350) // Caution (Orange)
            //    {
            //        e.Graphics.DrawText(15, 37, Program.gameMem.CurrentHealth.ToString(), Brushes.Orange);
            //        playerHealthStatus.ThreadSafeSetHealthImage((!Program.gameMem.IsPoisoned) ? Properties.Resources.CAUTION_ORANGE : Properties.Resources.POISON, (!Program.gameMem.IsPoisoned) ? "CAUTION_ORANGE" : "POISON");
            //    }
            //    else if (Program.gameMem.CurrentHealth <= 349) // Danger (Red)
            //    {
            //        e.Graphics.DrawText(15, 37, Program.gameMem.CurrentHealth.ToString(), Brushes.Red);
            //        playerHealthStatus.ThreadSafeSetHealthImage((!Program.gameMem.IsPoisoned) ? Properties.Resources.DANGER : Properties.Resources.POISON, (!Program.gameMem.IsPoisoned) ? "DANGER" : "POISON");
            //    }
            //}
            //else if (Program.gameMem.CurrentCharacter == CharacterEnumeration.Jill)
            //{
            //    if (Program.gameMem.CurrentHealth > 960 || Program.gameMem.CurrentHealth < 0) // Dead?
            //    {
            //        e.Graphics.DrawText(15, 37, "DEAD", Brushes.Red);
            //        playerHealthStatus.ThreadSafeSetHealthImage(Properties.Resources.EMPTY, "EMPTY");
            //    }
            //    else if (Program.gameMem.CurrentHealth >= 720) // Fine (Green)
            //    {
            //        e.Graphics.DrawText(15, 37, Program.gameMem.CurrentHealth.ToString(), Brushes.LawnGreen);
            //        playerHealthStatus.ThreadSafeSetHealthImage((!Program.gameMem.IsPoisoned) ? Properties.Resources.FINE : Properties.Resources.POISON, (!Program.gameMem.IsPoisoned) ? "FINE" : "POISON");
            //    }
            //    else if (Program.gameMem.CurrentHealth <= 719 && Program.gameMem.CurrentHealth >= 480) // Caution (Yellow)
            //    {
            //        e.Graphics.DrawText(15, 37, Program.gameMem.CurrentHealth.ToString(), Brushes.Goldenrod);
            //        playerHealthStatus.ThreadSafeSetHealthImage((!Program.gameMem.IsPoisoned) ? Properties.Resources.CAUTION_YELLOW : Properties.Resources.POISON, (!Program.gameMem.IsPoisoned) ? "CAUTION_YELLOW" : "POISON");
            //    }
            //    else if (Program.gameMem.CurrentHealth <= 479 && Program.gameMem.CurrentHealth >= 240) // Caution (Orange)
            //    {
            //        e.Graphics.DrawText(15, 37, Program.gameMem.CurrentHealth.ToString(), Brushes.Orange);
            //        playerHealthStatus.ThreadSafeSetHealthImage((!Program.gameMem.IsPoisoned) ? Properties.Resources.CAUTION_ORANGE : Properties.Resources.POISON, (!Program.gameMem.IsPoisoned) ? "CAUTION_ORANGE" : "POISON");
            //    }
            //    else if (Program.gameMem.CurrentHealth <= 239) // Danger (Red)
            //    {
            //        e.Graphics.DrawText(15, 37, Program.gameMem.CurrentHealth.ToString(), Brushes.Red);
            //        playerHealthStatus.ThreadSafeSetHealthImage((!Program.gameMem.IsPoisoned) ? Properties.Resources.DANGER : Properties.Resources.POISON, (!Program.gameMem.IsPoisoned) ? "DANGER" : "POISON");
            //    }
            //}
            //else if (Program.gameMem.CurrentCharacter == CharacterEnumeration.Rebecca)
            //{
            //    if (Program.gameMem.CurrentHealth > 850 || Program.gameMem.CurrentHealth < 0) // Dead?
            //    {
            //        e.Graphics.DrawText(15, 37, "DEAD", Brushes.Red);
            //        playerHealthStatus.ThreadSafeSetHealthImage(Properties.Resources.EMPTY, "EMPTY");
            //    }
            //    else if (Program.gameMem.CurrentHealth >= 638) // Fine (Green)
            //    {
            //        e.Graphics.DrawText(15, 37, Program.gameMem.CurrentHealth.ToString(), Brushes.LawnGreen);
            //        playerHealthStatus.ThreadSafeSetHealthImage((!Program.gameMem.IsPoisoned) ? Properties.Resources.FINE : Properties.Resources.POISON, (!Program.gameMem.IsPoisoned) ? "FINE" : "POISON");
            //    }
            //    else if (Program.gameMem.CurrentHealth <= 637 && Program.gameMem.CurrentHealth >= 425) // Caution (Yellow)
            //    {
            //        e.Graphics.DrawText(15, 37, Program.gameMem.CurrentHealth.ToString(), Brushes.Goldenrod);
            //        playerHealthStatus.ThreadSafeSetHealthImage((!Program.gameMem.IsPoisoned) ? Properties.Resources.CAUTION_YELLOW : Properties.Resources.POISON, (!Program.gameMem.IsPoisoned) ? "CAUTION_YELLOW" : "POISON");
            //    }
            //    else if (Program.gameMem.CurrentHealth <= 424 && Program.gameMem.CurrentHealth >= 213) // Caution (Orange)
            //    {
            //        e.Graphics.DrawText(15, 37, Program.gameMem.CurrentHealth.ToString(), Brushes.Orange);
            //        playerHealthStatus.ThreadSafeSetHealthImage((!Program.gameMem.IsPoisoned) ? Properties.Resources.CAUTION_ORANGE : Properties.Resources.POISON, (!Program.gameMem.IsPoisoned) ? "CAUTION_ORANGE" : "POISON");
            //    }
            //    else if (Program.gameMem.CurrentHealth <= 212) // Danger (Red)
            //    {
            //        e.Graphics.DrawText(15, 37, Program.gameMem.CurrentHealth.ToString(), Brushes.Red);
            //        playerHealthStatus.ThreadSafeSetHealthImage((!Program.gameMem.IsPoisoned) ? Properties.Resources.DANGER : Properties.Resources.POISON, (!Program.gameMem.IsPoisoned) ? "DANGER" : "POISON");
            //    }
            //}
            //else // AKA anything not handled.
            //{

            //}
        }

        private void playerInfoPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = smoothingMode;
            e.Graphics.CompositingQuality = compositingQuality;
            e.Graphics.CompositingMode = compositingMode;
            e.Graphics.InterpolationMode = interpolationMode;
            e.Graphics.PixelOffsetMode = pixelOffsetMode;

            //Brush slotColor = Brushes.LawnGreen;
            //if (Program.gameMem.EquippedItemSlotIndex != -1)
            //    e.Graphics.DrawInvItem(ItemPositionEnumeration.EquippedItemSlot, GameMemory.ItemToImageTranslation[Program.gameMem.ItemSlots[Program.gameMem.EquippedItemSlotIndex].Item], Program.gameMem.ItemSlots[Program.gameMem.EquippedItemSlotIndex].GetQuantityText(out slotColor), slotColor);
            //else
            //    e.Graphics.DrawInvItem(ItemPositionEnumeration.EquippedItemSlot, GameMemory.ItemToImageTranslation[ItemEnumeration.None], string.Empty, slotColor);

            //slotColor = Brushes.LightGray;
            //if (Program.gameMem.CurrentCharacter == CharacterEnumeration.Chris || Program.gameMem.CurrentCharacter == CharacterEnumeration.Rebecca)
            //{
            //    e.Graphics.DrawInvItem(ItemPositionEnumeration.DefItemSlot1, GameMemory.ItemToImageTranslation[ItemEnumeration.Dagger], Program.gameMem.DefenseItemQuantity1.ToString(), slotColor, Program.gameMem.EquippedDefenseItem == DefenseItemEnumeration.Dagger);
            //    e.Graphics.DrawInvItem(ItemPositionEnumeration.DefItemSlot2, GameMemory.ItemToImageTranslation[ItemEnumeration.FlashGrenade], Program.gameMem.DefenseItemQuantity2.ToString(), slotColor, Program.gameMem.EquippedDefenseItem == DefenseItemEnumeration.FlashGrenade);
            //}
            //else if (Program.gameMem.CurrentCharacter == CharacterEnumeration.Jill)
            //{
            //    e.Graphics.DrawInvItem(ItemPositionEnumeration.DefItemSlot1, GameMemory.ItemToImageTranslation[ItemEnumeration.Dagger], Program.gameMem.DefenseItemQuantity1.ToString(), slotColor, Program.gameMem.EquippedDefenseItem == DefenseItemEnumeration.Dagger);
            //    e.Graphics.DrawInvItem(ItemPositionEnumeration.DefItemSlot2, GameMemory.ItemToImageTranslation[ItemEnumeration.BatteryPack], Program.gameMem.DefenseItemQuantity2.ToString(), slotColor, Program.gameMem.EquippedDefenseItem == DefenseItemEnumeration.BatteryPack);
            //}
        }

        private void inventoryPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = smoothingMode;
            e.Graphics.CompositingQuality = compositingQuality;
            e.Graphics.CompositingMode = compositingMode;
            e.Graphics.InterpolationMode = interpolationMode;
            e.Graphics.PixelOffsetMode = pixelOffsetMode;

            Brush slotColor;

            //// Draw inventory and quantity.
            //e.Graphics.DrawInvItem(ItemPositionEnumeration.InvItemSlot1, GameMemory.ItemToImageTranslation[Program.gameMem.ItemSlots[0].Item], Program.gameMem.ItemSlots[0].GetQuantityText(out slotColor), slotColor);
            //e.Graphics.DrawInvItem(ItemPositionEnumeration.InvItemSlot2, GameMemory.ItemToImageTranslation[Program.gameMem.ItemSlots[1].Item], Program.gameMem.ItemSlots[1].GetQuantityText(out slotColor), slotColor);
            //e.Graphics.DrawInvItem(ItemPositionEnumeration.InvItemSlot3, GameMemory.ItemToImageTranslation[Program.gameMem.ItemSlots[2].Item], Program.gameMem.ItemSlots[2].GetQuantityText(out slotColor), slotColor);
            //e.Graphics.DrawInvItem(ItemPositionEnumeration.InvItemSlot4, GameMemory.ItemToImageTranslation[Program.gameMem.ItemSlots[3].Item], Program.gameMem.ItemSlots[3].GetQuantityText(out slotColor), slotColor);
            //e.Graphics.DrawInvItem(ItemPositionEnumeration.InvItemSlot5, GameMemory.ItemToImageTranslation[Program.gameMem.ItemSlots[4].Item], Program.gameMem.ItemSlots[4].GetQuantityText(out slotColor), slotColor);
            //e.Graphics.DrawInvItem(ItemPositionEnumeration.InvItemSlot6, GameMemory.ItemToImageTranslation[Program.gameMem.ItemSlots[5].Item], Program.gameMem.ItemSlots[5].GetQuantityText(out slotColor), slotColor);

            //if (Program.gameMem.CurrentCharacter == CharacterEnumeration.Chris || Program.gameMem.CurrentCharacter == CharacterEnumeration.Rebecca)
            //{
            //    // Chris/Rebecca have 6 slots.
            //    e.Graphics.DrawInvItem(ItemPositionEnumeration.InvItemSlot7, Program.blockedOffSlot, string.Empty, slotColor);
            //    e.Graphics.DrawInvItem(ItemPositionEnumeration.InvItemSlot8, Program.blockedOffSlot, string.Empty, slotColor);
            //}
            //else if (Program.gameMem.CurrentCharacter == CharacterEnumeration.Jill)
            //{
            //    // Jill has an extra 2 slots.
            //    e.Graphics.DrawInvItem(ItemPositionEnumeration.InvItemSlot7, GameMemory.ItemToImageTranslation[Program.gameMem.ItemSlots[6].Item], Program.gameMem.ItemSlots[6].GetQuantityText(out slotColor), slotColor);
            //    e.Graphics.DrawInvItem(ItemPositionEnumeration.InvItemSlot8, GameMemory.ItemToImageTranslation[Program.gameMem.ItemSlots[7].Item], Program.gameMem.ItemSlots[7].GetQuantityText(out slotColor), slotColor);
            //}
        }

        private void statisticsPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = smoothingMode;
            e.Graphics.CompositingQuality = compositingQuality;
            e.Graphics.CompositingMode = compositingMode;
            e.Graphics.InterpolationMode = interpolationMode;
            e.Graphics.PixelOffsetMode = pixelOffsetMode;

            int heightOffset = 15;

            // Increment for displaying text properly.
            int i = 1;

            e.Graphics.DrawText(0, 0, string.Format("{0}", Program.gameMem.InGameTimerString), Brushes.White, new Font("Consolas", 16, FontStyle.Bold));

            if (Program.gameMem.BossCurrentHealth != 0 && Program.gameMem.BossMaxHealth != 0)
                e.Graphics.DrawText(0, (heightOffset * ++i), string.Format("Boss: {0} ({1:P0})", Program.gameMem.BossCurrentHealth, (decimal)Program.gameMem.BossCurrentHealth / (decimal)Program.gameMem.BossMaxHealth), Brushes.Red, new Font("Consolas", 9, FontStyle.Bold));


            //e.Graphics.DrawText(0, 26, "Raw IGT", Brushes.Gray, new Font("Consolas", 9, FontStyle.Bold));
            //e.Graphics.DrawText(52, 22, Program.gameMem.RawInGameTimer.ToString(), Brushes.Gray, new Font("Consolas", 12, FontStyle.Bold));
            //e.Graphics.DrawText(0, 10 + (heightOffset * ++i), string.Format("Saves: {0}", Program.gameMem.TotalSaves), Brushes.Gray, new Font("Consolas", 9, FontStyle.Bold));
            //e.Graphics.DrawText(0, 10 + (heightOffset * ++i), string.Format("Shots Fired: {0}", Program.gameMem.TotalShots), Brushes.Gray, new Font("Consolas", 9, FontStyle.Bold));
            //e.Graphics.DrawText(0, 10 + (heightOffset * ++i), string.Format("Ammo: {0} / {1}", Program.gameMem.EquippedCurrentAmmo, Program.gameMem.EquippedMaxAmmo), Brushes.Gray, new Font("Consolas", 9, FontStyle.Bold));
        }
    }
}
