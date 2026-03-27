using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HabitTracker.WinForms.Forms
{
    public class ToastNotification : Form
    {
        private Timer fadeTimer;
        private double opacityThreshold = 1.0;
        
        public ToastNotification(string message, Color bg)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = bg;
            this.ForeColor = Color.White;
            this.Size = new Size(250, 60);
            this.Opacity = 0.9;
            this.TopMost = true;

            var lbl = new Label 
            {
                Text = message,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            this.Controls.Add(lbl);

            fadeTimer = new Timer { Interval = 50 };
            fadeTimer.Tick += FadeTimer_Tick;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw a rounded border simulation implicitly
            base.OnPaint(e);
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            opacityThreshold -= 0.05;
            if (opacityThreshold <= 0.0)
            {
                fadeTimer.Stop();
                this.Close();
            }
            else
            {
                this.Opacity = opacityThreshold;
            }
        }

        public void ShowToast(Form parent)
        {
            if (parent != null)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(
                    parent.Location.X + (parent.Width - this.Width) / 2, 
                    parent.Location.Y + parent.Height - 150
                );
            }
            this.Show();
            Timer waitTimer = new Timer { Interval = 1500 };
            waitTimer.Tick += (s, e) => { waitTimer.Stop(); fadeTimer.Start(); };
            waitTimer.Start();
        }
    }
}
