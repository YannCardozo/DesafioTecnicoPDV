using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdvNetDesktop.Components
{
    public class SpinnerOverlay : Panel
    {
        private readonly Spinner spinner;

        public SpinnerOverlay(Control parent)
        {
            Dock = DockStyle.Fill;
            Visible = false;

            spinner = new Spinner { Size = new Size(50, 50), Anchor = AnchorStyles.None };
            Controls.Add(spinner);

            parent.Controls.Add(this);

            Resize += (_, _) =>
            {
                spinner.Left = (Width - spinner.Width) / 2;
                spinner.Top = (Height - spinner.Height) / 2;
            };
        }

        public new void Show()
        {
            Visible = true;
            BringToFront();
            spinner.Start();
        }

        public new void Hide()
        {
            spinner.Stop();
            Visible = false;
        }
    }

    
}
