using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdvNetDesktop.Components
{
    public class Spinner : Control
    {
        private readonly System.Windows.Forms.Timer timer = new() { Interval = 45 };
        private int angle;

        public Spinner()
        {
            DoubleBuffered = true;
            timer.Tick += (_, _) => { angle = (angle + 10) % 360; Invalidate(); };
            Size = new Size(40, 40);
        }

        public void Start() => timer.Start();
        public void Stop() => timer.Stop();

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int s = Math.Min(Width, Height) - 6;
            Rectangle rect = new(3, 3, s, s);

            using var pen = new Pen(Color.White, 4)
            { StartCap = LineCap.Round, EndCap = LineCap.Round };

            e.Graphics.TranslateTransform(Width / 2f, Height / 2f);
            e.Graphics.RotateTransform(angle);
            e.Graphics.TranslateTransform(-Width / 2f, -Height / 2f);

            e.Graphics.DrawArc(pen, rect, 0, 270);
        }
    }
}
