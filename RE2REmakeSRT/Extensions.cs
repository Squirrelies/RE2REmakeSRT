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
            //if (image == null)
            //    image = Program.emptySlot;

            //if (b == null)
            //    b = Brushes.LawnGreen;

            //byte bytePosition = (byte)position;
            //if (position == ItemPositionEnumeration.DefItemSlot1) // 30
            //{
            //    float xImg = ICON_OFFSET_X;
            //    float yImg = ICON_OFFSET_Y + (INVENTORY_ITEM_HEIGHT * (0 / 2));
            //    g.DrawImage(image, xImg, yImg, INVENTORY_ITEM_WIDTH, INVENTORY_ITEM_HEIGHT);

            //    if (equipped)
            //        g.DrawRectangle(new Pen(Color.LawnGreen, 3), xImg + 1, yImg + 1, INVENTORY_ITEM_WIDTH - 3, INVENTORY_ITEM_HEIGHT - 3);

            //    float xStr = ICON_OFFSET_X + 5;
            //    float yStr = ICON_OFFSET_Y + ((INVENTORY_ITEM_HEIGHT * (0 / 2)) + 64);
            //    g.DrawString(text, new Font("Consolas", 14, FontStyle.Bold), b, xStr, yStr);
            //}
            //else if (position == ItemPositionEnumeration.DefItemSlot2) // 30
            //{
            //    float xImg = 96 + 30 + ICON_OFFSET_X;
            //    float yImg = ICON_OFFSET_Y + (INVENTORY_ITEM_HEIGHT * (0 / 2));
            //    g.DrawImage(image, xImg, yImg, INVENTORY_ITEM_WIDTH, INVENTORY_ITEM_HEIGHT);

            //    if (equipped)
            //        g.DrawRectangle(new Pen(Color.LawnGreen, 3), xImg + 1, yImg + 1, INVENTORY_ITEM_WIDTH - 3, INVENTORY_ITEM_HEIGHT - 3);

            //    float xStr = 96 + 30 + ICON_OFFSET_X + 5;
            //    float yStr = ICON_OFFSET_Y + ((INVENTORY_ITEM_HEIGHT * (0 / 2)) + 64);
            //    g.DrawString(text, new Font("Consolas", 14, FontStyle.Bold), b, xStr, yStr);
            //}
            //else if (position == ItemPositionEnumeration.EquippedItemSlot) // 30
            //{
            //    float xImg = 192 + 60 + ICON_OFFSET_X;
            //    float yImg = ICON_OFFSET_Y + (INVENTORY_ITEM_HEIGHT * (0 / 2));
            //    g.DrawImage(image, xImg, yImg, INVENTORY_ITEM_WIDTH, INVENTORY_ITEM_HEIGHT);

            //    float xStr = 192 + 60 + ICON_OFFSET_X + 5;
            //    float yStr = ICON_OFFSET_Y + ((INVENTORY_ITEM_HEIGHT * (0 / 2)) + 64);
            //    g.DrawString(text, new Font("Consolas", 14, FontStyle.Bold), b, xStr, yStr);
            //}
            //else if (bytePosition % 2 == 1) // Odd
            //{
            //    float xImg = ICON_OFFSET_X + INVENTORY_ITEM_WIDTH;
            //    float yImg = ICON_OFFSET_Y + (INVENTORY_ITEM_HEIGHT * (bytePosition / 2));
            //    g.DrawImage(image, xImg, yImg, INVENTORY_ITEM_WIDTH, INVENTORY_ITEM_HEIGHT);

            //    float xStr = ICON_OFFSET_X + INVENTORY_ITEM_WIDTH + 5;
            //    float yStr = ICON_OFFSET_Y + ((INVENTORY_ITEM_HEIGHT * (bytePosition / 2)) + 64);
            //    g.DrawString(text, new Font("Consolas", 14, FontStyle.Bold), b, xStr, yStr);
            //}
            //else // Even
            //{
            //    float xImg = ICON_OFFSET_X;
            //    float yImg = ICON_OFFSET_Y + (INVENTORY_ITEM_HEIGHT * (bytePosition / 2));
            //    g.DrawImage(image, xImg, yImg, INVENTORY_ITEM_WIDTH, INVENTORY_ITEM_HEIGHT);

            //    float xStr = ICON_OFFSET_X + 5;
            //    float yStr = ICON_OFFSET_Y + ((INVENTORY_ITEM_HEIGHT * (bytePosition / 2)) + 64);
            //    g.DrawString(text, new Font("Consolas", 14, FontStyle.Bold), b, xStr, yStr);
            //}
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
    }
}
