using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace RE2REmakeSRT
{
    public partial class AttachUI : Form
    {
        private System.Timers.Timer processPollingTimer;

        public AttachUI()
        {
            InitializeComponent();

            this.ContextMenu = Program.contextMenu;

            processPollingTimer = new System.Timers.Timer() { AutoReset = false, Interval = 250 };
            processPollingTimer.Elapsed += ProcessPollingTimer_Elapsed;
            processPollingTimer.Start();
        }

        private void ProcessPollingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Process[] gameProcesses = Process.GetProcessesByName("re2");
                Debug.WriteLine("RE2 (2019) processes found: {0}", gameProcesses.Length);
                if (gameProcesses.Length != 0)
                {
                    Program.gameProc = gameProcesses[0];

                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() =>
                        {
                            this.Close();
                        }));
                    }
                    else
                        this.Close();
                }
            }
            finally
            {
                if (Program.gameProc == null)
                    ((System.Timers.Timer)sender).Start();
            }
        }
    }
}
