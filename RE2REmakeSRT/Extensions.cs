using System;
using System.Drawing;
using System.Windows.Forms;

namespace RE2REmakeSRT
{
    public static class Extensions
    {
        private const int INVENTORY_ITEM_WIDTH = 96;
        private const int INVENTORY_ITEM_HEIGHT = 96;

        private const int ICON_OFFSET_X = 0;
        private const int ICON_OFFSET_Y = 0;

        public static void DrawInvItem(this Graphics g, ItemPositionEnumeration position, Image image = null, string text = "", Brush b = null, bool equipped = false)
        {
            
        }

        public static void DrawText(this Graphics g, float x, float y, string text = "", Brush b = null, Font f = null)
        {
            if (b == null)
                b = Brushes.LawnGreen;

            if (f == null)
                f = new Font("Consolas", 14, FontStyle.Bold);

            g.DrawString(text, f, b, x, y);
        }

        public static void ThreadSafeSetHealthImage(this PictureBox picBox, Image image, string imageKey)
        {
            if (picBox.InvokeRequired)
            {
                picBox.Invoke(new Action(() =>
                {
                    if (picBox.Tag == null || picBox.Tag.ToString() != imageKey)
                    {
                        picBox.Tag = imageKey;
                        picBox.Image = image;
                    }
                }));
            }
            else
            {
                if (picBox.Tag == null || picBox.Tag.ToString() != imageKey)
                {
                    picBox.Tag = imageKey;
                    picBox.Image = image;
                }
            }
        }

        public static bool ByteArrayEquals(this byte[] first, byte[] second)
        {
            // Check to see if the have the same reference.
            if (first == second)
                return true;

            // Check to make sure neither are null.
            if (first == null || second == null)
                return false;

            // Ensure the array lengths match.
            if (first.Length != second.Length)
                return false;

            // Check each element side by side for equality.
            for (int i = 0; i < first.Length; i++)
                if (first[i] != second[i])
                    return false;

            // We made it past the for loop, we're equals!
            return true;
        }
    }
}
