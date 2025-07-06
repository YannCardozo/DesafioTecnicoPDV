using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdvNetDesktop.Components
{
    public class LoadingOverlay : Panel
    {
        private readonly ProgressBar bar;

        public LoadingOverlay(Control parent)
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(120, Color.Gray);
            Visible = false;

            bar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Size = new Size(120, 20),
                Anchor = AnchorStyles.None
            };
            Controls.Add(bar);

            parent.Controls.Add(this);

            // centraliza progressBar sempre que redimensionar
            Resize += (_, _) =>
            {
                bar.Left = (Width - bar.Width) / 2;
                bar.Top = (Height - bar.Height) / 2;
            };
        }
    }
}
