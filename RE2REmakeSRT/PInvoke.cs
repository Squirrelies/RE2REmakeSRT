using System;
using System.Runtime.InteropServices;

namespace RE2REmakeSRT
{
    public static class PInvoke
    {
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();

        public static void DragControl(IntPtr controlHandle)
        {
            ReleaseCapture();
            SendMessage(controlHandle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
    }
}
