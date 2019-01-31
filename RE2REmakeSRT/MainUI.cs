using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RE2REmakeSRT
{
    public partial class MainUI : Form
    {
        // How often to perform more expensive operations.
        // 2500 milliseconds for updating pointers.
        // 333 milliseconds for a full scan.
        // 16 milliseconds for a slim scan.
        public const long PTR_UPDATE_TICKS = TimeSpan.TicksPerMillisecond * 2500L;
        public const long FULL_UI_DRAW_TICKS = TimeSpan.TicksPerMillisecond * 333L;
        public const double SLIM_UI_DRAW_MS = 16d;

        private System.Timers.Timer memoryPollingTimer;
        private long lastPtrUpdate;
        private long lastFullUIDraw;
        
        private SmoothingMode smoothingMode = SmoothingMode.HighSpeed;
        private PixelOffsetMode pixelOffsetMode = PixelOffsetMode.HighSpeed;
        private CompositingQuality compositingQuality = CompositingQuality.HighSpeed;
        private CompositingMode compositingMode = CompositingMode.SourceOver;
        private InterpolationMode interpolationMode = InterpolationMode.Low;

        public MainUI()
        {
            InitializeComponent();

            lastPtrUpdate = DateTime.UtcNow.Ticks;
            lastFullUIDraw = DateTime.UtcNow.Ticks;
            
            memoryPollingTimer = new System.Timers.Timer() { AutoReset = false, Interval = SLIM_UI_DRAW_MS };
            memoryPollingTimer.Elapsed += MemoryPollingTimer_Elapsed;
            memoryPollingTimer.Start();
        }

        private void MemoryPollingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                // Suspend UI layout logic to perform redrawing.
                MainUI uiForm = (MainUI)Program.mainContext.MainForm;

                // Only perform a pointer update occasionally.
                if (DateTime.UtcNow.Ticks - lastPtrUpdate >= PTR_UPDATE_TICKS)
                {
                    // Update the last drawn time.
                    lastPtrUpdate = DateTime.UtcNow.Ticks;

                    // Update the pointers.
                    Program.gameMem.UpdatePointers();
                }

                // Only draw occasionally, not as often as the stats panel.
                if (DateTime.UtcNow.Ticks - lastFullUIDraw >= FULL_UI_DRAW_TICKS)
                {
                    // Update the last drawn time.
                    lastFullUIDraw = DateTime.UtcNow.Ticks;

                    // Get the full amount of updated information from memory.
                    Program.gameMem.Refresh();

                    // Only draw these periodically to reduce CPU usage.
                    uiForm.playerHealthStatus.Invalidate();
                }
                else
                {
                    // Get a slimmed-down amount of updated information from memory.
                    Program.gameMem.RefreshSlim();
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
        }

        private void playerInfoPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = smoothingMode;
            e.Graphics.CompositingQuality = compositingQuality;
            e.Graphics.CompositingMode = compositingMode;
            e.Graphics.InterpolationMode = interpolationMode;
            e.Graphics.PixelOffsetMode = pixelOffsetMode;
        }

        private void inventoryPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = smoothingMode;
            e.Graphics.CompositingQuality = compositingQuality;
            e.Graphics.CompositingMode = compositingMode;
            e.Graphics.InterpolationMode = interpolationMode;
            e.Graphics.PixelOffsetMode = pixelOffsetMode;
        }

        private void statisticsPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = smoothingMode;
            e.Graphics.CompositingQuality = compositingQuality;
            e.Graphics.CompositingMode = compositingMode;
            e.Graphics.InterpolationMode = interpolationMode;
            e.Graphics.PixelOffsetMode = pixelOffsetMode;

            // IGT Display.
            e.Graphics.DrawText(0, 0, string.Format("{0}", Program.gameMem.IGTString), Brushes.White, new Font("Consolas", 16, FontStyle.Bold));
            e.Graphics.DrawText(2, 25, "Raw IGT", Brushes.Gray, new Font("Consolas", 9, FontStyle.Bold));
            e.Graphics.DrawText(0, 37, Program.gameMem.IGTRaw.ToString(), Brushes.Gray, new Font("Consolas", 12, FontStyle.Bold));

            // Additional information and stats.
            // Adjustments for displaying text properly.
            int heightGap = 15;
            int heightOffset = 25;
            int i = 1;

            e.Graphics.DrawText(0, heightOffset + (heightGap * ++i), string.Format("Rank: {0}", Program.gameMem.Rank), Brushes.Gray, new Font("Consolas", 9, FontStyle.Bold));
            e.Graphics.DrawText(0, heightOffset + (heightGap * ++i), string.Format("Score: {0}", Program.gameMem.RankScore), Brushes.Gray, new Font("Consolas", 9, FontStyle.Bold));

            if (Program.gameMem.BossCurrentHealth != 0 && Program.gameMem.BossMaxHealth != 0)
            {
                e.Graphics.DrawText(0, heightOffset + (heightGap * ++i), "Boss", Brushes.Red, new Font("Consolas", 10, FontStyle.Bold));
                e.Graphics.DrawText(0, heightOffset + (heightGap * ++i), string.Format("{0} {1:P1}", Program.gameMem.BossCurrentHealth, (decimal)Program.gameMem.BossCurrentHealth / (decimal)Program.gameMem.BossMaxHealth), Brushes.Red, new Font("Consolas", 10, FontStyle.Bold));
            }
        }
    }
}
