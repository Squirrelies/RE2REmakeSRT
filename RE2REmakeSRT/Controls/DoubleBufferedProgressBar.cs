using System.Drawing;
using System.Windows.Forms;

namespace RE2REmakeSRT.Controls
{
    public class DoubleBufferedProgressBar : Control
    {
        public DoubleBufferedProgressBar()
        {
            // Set control styles.
            this.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.Selectable, false);

            // Set some defaults.
            this.minimum = 0;
            this.maximum = 100;
            this.value = 0;

            // Set default bar colors.
            this.ForeColor = Color.Firebrick;
            this.BackColor = Color.DimGray;
        }

        // Storage variables.
        private decimal value;
        private decimal minimum;
        private decimal maximum;

        // Public properties.
        public decimal Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
                Invalidate();
            }
        }

        public decimal Minimum
        {
            get
            {
                return this.minimum;
            }
            set
            {
                this.minimum = value;
                Invalidate();
            }
        }

        public decimal Maximum
        {
            get
            {
                return this.maximum;
            }
            set
            {
                this.maximum = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            RectangleF rc = new RectangleF(
                1,
                1,
                (float)(this.Width * (Value - Minimum) / Maximum) - 2f,
                this.Height - 2f
                );
            using (SolidBrush br = new SolidBrush(this.ForeColor))
            {
                e.Graphics.FillRectangle(br, rc);
            }
            base.OnPaint(e);
        }
    }
}
