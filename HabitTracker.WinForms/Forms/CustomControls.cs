using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HabitTracker.WinForms.Forms
{
    // Reusable Custom Modern Button
    public class ModernButton : Button
    {
        public Color HoverColor { get; set; }
        public Color NormalColor { get; set; }
        public int BorderRadius { get; set; } = 8;

        public ModernButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = NormalColor;
            this.Cursor = Cursors.Hand;
            this.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.MouseEnter += (s, e) => { this.BackColor = HoverColor; };
            this.MouseLeave += (s, e) => { 
                if (this.Parent != null && this.Bounds.Contains(this.Parent.PointToClient(Cursor.Position))) return;
                this.BackColor = NormalColor; 
            };
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            GraphicsPath path = new GraphicsPath();
            int r = BorderRadius;
            path.AddArc(0, 0, r, r, 180, 90);
            path.AddArc(this.Width - r, 0, r, r, 270, 90);
            path.AddArc(this.Width - r, this.Height - r, r, r, 0, 90);
            path.AddArc(0, this.Height - r, r, r, 90, 90);
            path.CloseFigure();
            this.Region = new Region(path);

            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (this.FlatAppearance.BorderSize > 0)
            {
                using (Pen pen = new Pen(this.FlatAppearance.BorderColor, this.FlatAppearance.BorderSize))
                {
                    pevent.Graphics.DrawPath(pen, path);
                }
            }
        }
    }

    // Custom Rounded Panel for Cards
    public class RoundedPanel : Panel
    {
        public int BorderRadius { get; set; } = 15;
        public Color GradientTop { get; set; } = Color.FromArgb(45, 45, 48);
        public Color GradientBottom { get; set; } = Color.FromArgb(30, 30, 32);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsPath path = new GraphicsPath();
            int r = BorderRadius;
            path.AddArc(0, 0, r, r, 180, 90);
            path.AddArc(this.Width - r, 0, r, r, 270, 90);
            path.AddArc(this.Width - r, this.Height - r, r, r, 0, 90);
            path.AddArc(0, this.Height - r, r, r, 90, 90);
            path.CloseFigure();

            this.Region = new Region(path);

            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, GradientTop, GradientBottom, 90F))
            {
                g.FillPath(brush, path);
            }
        }
    }
}
