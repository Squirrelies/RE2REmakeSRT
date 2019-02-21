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
                Program.GetProcessPid();
            }
            finally
            {
                if (Program.gamePID == -1)
                    ((System.Timers.Timer)sender).Start();
                else
                    CloseForm();
            }
        }

        private void CloseForm()
        {
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
}
