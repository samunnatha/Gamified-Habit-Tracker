using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using HabitTracker.Core.Models;
using HabitTracker.Core.Services;

namespace HabitTracker.WinForms.Forms
{
    public class MainForm : Form
    {
        private readonly string _dbPath;
        private User _user;
        
        private readonly HabitService _habitService;
        private readonly UserService _userService;
        private readonly RewardService _rewardService;
        private readonly BackupService _backupService;
        private readonly AnalyticsService _analyticsService;

        private FlowLayoutPanel flpHabits;
        private Panel customXPBar;
        private Label lblLevelXP;
        private Label lblCoinsFreezes;
        private Label lblMotivational;
        
        // Mini Dashboard UI
        private RoundedPanel pnlDashboard;
        private Chart miniChart;
        private Label lblTodayStats;

        private Timer reminderTimer;
        private Timer xpAnimTimer;
        private int targetXp;
        private int currentRenderXp;
        private int requiredXp;

        private Color _bg;
        private Color _fg;
        private Color _accent;

        public MainForm(string dbPath, User loggedInUser)
        {
            _dbPath = dbPath;
            _user = loggedInUser;
            _habitService = new HabitService(dbPath);
            _userService = new UserService(dbPath);
            _rewardService = new RewardService(dbPath);
            _backupService = new BackupService(dbPath);
            _analyticsService = new AnalyticsService(dbPath);

            // Default checks
            _user = _userService.Authenticate(_user.Username, _user.PasswordHash);
            targetXp = _user.XP;
            currentRenderXp = targetXp;

            // Deep Dark Theme (State of the Art setup)
            _bg = Color.FromArgb(20, 20, 22);
            _fg = Color.WhiteSmoke;
            _accent = Color.FromArgb(70, 130, 180); // SteelBlue
            
            InitializeComponent();
            LoadData();
            LoadMiniDashboard();
            
            reminderTimer = new Timer { Interval = 60000 }; 
            reminderTimer.Tick += ReminderTimer_Tick;
            reminderTimer.Start();

            xpAnimTimer = new Timer { Interval = 10 };
            xpAnimTimer.Tick += XpAnimTimer_Tick;
        }

        private void InitializeComponent()
        {
            this.Text = $"Gamified Habit Tracker - Welcome {_user.Username}!";
            this.Size = new Size(1150, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = _bg;
            this.ForeColor = _fg;

            // TOP WIDE HEADER CONFIG (Sleek Gradient-like aesthetics)
            var panelTop = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(28, 28, 30), Padding = new Padding(20) };
            
            lblLevelXP = new Label { Top = 20, Left = 20, Width = 350, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = _fg };
            
            // Custom drawn XP Bar!
            customXPBar = new Panel { Top = 55, Left = 20, Width = 400, Height = 15, BackColor = Color.FromArgb(40,40,40) };
            customXPBar.Paint += CustomXPBar_Paint;

            lblCoinsFreezes = new Label { Top = 20, Left = 450, Width = 250, Font = new Font("Segoe UI", 12, FontStyle.Italic), ForeColor = Color.Gold };
            lblMotivational = new Label { Top = 50, Left = 450, Width = 350, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = _accent };

            var btnJSON = new ModernButton() { 
                Text = "Export Backup 💾", Top = 25, Left = 900, Width = 150, Height=40, NormalColor=Color.FromArgb(50,50,55), HoverColor=Color.FromArgb(70,70,80), ForeColor=Color.White 
            };
            btnJSON.Click += (s, e) => {
                using (var sfd = new SaveFileDialog { Filter = "JSON File|*.json", FileName = "HabitBackup.json" })
                    if (sfd.ShowDialog() == DialogResult.OK) { _backupService.ExportToJson(_user.UserId, sfd.FileName); new ToastNotification("Exported!", Color.SteelBlue).ShowToast(this); }
            };

            var btnJSONImport = new ModernButton() { 
                Text = "Restore 🔄", Top = 25, Left = 800, Width = 90, Height=40, NormalColor=Color.FromArgb(50,50,55), HoverColor=Color.FromArgb(70,70,80), ForeColor=Color.White 
            };
            btnJSONImport.Click += (s, e) => {
                using (var ofd = new OpenFileDialog { Filter = "JSON File|*.json" })
                    if (ofd.ShowDialog() == DialogResult.OK) { _backupService.ImportFromJson(_user.UserId, ofd.FileName); LoadData(); LoadMiniDashboard(); new ToastNotification("Restored!", Color.MediumPurple).ShowToast(this); }
            };

            panelTop.Controls.Add(lblLevelXP);
            panelTop.Controls.Add(customXPBar);
            panelTop.Controls.Add(lblCoinsFreezes);
            panelTop.Controls.Add(lblMotivational);
            panelTop.Controls.Add(btnJSON);
            panelTop.Controls.Add(btnJSONImport);

            // SIDEBAR NAV CONFIG
            var panelSidebar = new Panel { Dock = DockStyle.Left, Width = 200, Padding = new Padding(15), BackColor = Color.FromArgb(24,24,26) };

            var lblMenu = new Label { Text="MANAGE", Dock=DockStyle.Top, Font=new Font("Segoe UI", 10, FontStyle.Bold), ForeColor=Color.Gray, Height=30 };
            
            var btnAdd = new ModernButton { Text = "+ Add Habit", Dock=DockStyle.Top, Height=45, Margin=new Padding(0,0,0,15), NormalColor=Color.MediumSeaGreen, HoverColor=Color.SeaGreen, ForeColor=Color.White };
            btnAdd.Click += BtnAdd_Click;
            
            var btnEdit = new ModernButton { Text = "✏️ Edit Habit", Dock=DockStyle.Top, Height=45, Margin=new Padding(0,0,0,10), NormalColor=Color.FromArgb(50,50,55), HoverColor=Color.FromArgb(70,70,80), ForeColor=Color.White };
            btnEdit.Click += BtnEdit_Click;

            var btnStore = new ModernButton { Text = "Rewards Store 🛒", Dock=DockStyle.Top, Height=50, Margin=new Padding(0,30,0,10), NormalColor=Color.Gold, HoverColor=Color.DarkGoldenrod, ForeColor=Color.Black };
            btnStore.Click += BtnStore_Click;

            var btnAnalytics = new ModernButton { Text = "Full Analytics 📊", Dock=DockStyle.Top, Height=50, NormalColor=_accent, HoverColor=Color.DeepSkyBlue, ForeColor=Color.White };
            btnAnalytics.Click += BtnAnalytics_Click;

            panelSidebar.Controls.Add(btnAnalytics);
            panelSidebar.Controls.Add(btnStore);
            panelSidebar.Controls.Add(btnEdit);
            panelSidebar.Controls.Add(btnAdd);
            panelSidebar.Controls.Add(lblMenu);

            // MINI DASHBOARD (Fills the black empty space at the bottom)
            pnlDashboard = new RoundedPanel { Dock = DockStyle.Bottom, Height = 220, BackColor = Color.Transparent, Padding = new Padding(20), BorderRadius = 20 };
            
            lblTodayStats = new Label 
            { 
                Top = 30, Left = 30, Width = 350, Height = 170,
                Font = new Font("Segoe UI", 13), ForeColor = _fg, BackColor = Color.Transparent
            };

            miniChart = new Chart { Top = 15, Left = 400, Width = 500, Height = 190, BackColor = Color.Transparent };
            var chartArea = new ChartArea("Main") { BackColor = Color.Transparent };
            chartArea.AxisX.LabelStyle.ForeColor = Color.LightGray;
            chartArea.AxisY.LabelStyle.ForeColor = Color.LightGray;
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(50,50,50);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(50,50,50);
            miniChart.ChartAreas.Add(chartArea);
            
            var series = new Series("Completions") { ChartType = SeriesChartType.Line, Color = _accent, BorderWidth = 4, MarkerStyle = MarkerStyle.Circle, MarkerSize = 8, MarkerColor = Color.White };
            miniChart.Series.Add(series);

            pnlDashboard.Controls.Add(lblTodayStats);
            pnlDashboard.Controls.Add(miniChart);

            // INTERACTIVE HABIT CARDS CONTAINER (Replacing DGV)
            flpHabits = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = _bg,
                FlowDirection = FlowDirection.LeftToRight
            };

            this.Controls.Add(flpHabits);
            this.Controls.Add(pnlDashboard);
            this.Controls.Add(panelSidebar);
            this.Controls.Add(panelTop);
        }

        private void CustomXPBar_Paint(object sender, PaintEventArgs e)
        {
            var pnl = sender as Panel;
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            if (requiredXp == 0) return;

            float fillRatio = (float)currentRenderXp / requiredXp;
            if (fillRatio > 1f) fillRatio = 1f;

            var bgBrush = new SolidBrush(Color.FromArgb(40, 40, 45));
            g.FillRectangle(bgBrush, 0, 0, pnl.Width, pnl.Height);

            int fillWidth = (int)(pnl.Width * fillRatio);
            if (fillWidth > 0)
            {
                using (var lgb = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, fillWidth, pnl.Height), Color.MediumPurple, Color.MediumSeaGreen, 0f))
                {
                    g.FillRectangle(lgb, 0, 0, fillWidth, pnl.Height);
                }
            }
        }

        private void LoadData()
        {
            _user = _userService.Authenticate(_user.Username, _user.PasswordHash);
            requiredXp = (int)Math.Pow(_user.Level, 2) * 50; 
            
            targetXp = Math.Min(_user.XP, requiredXp);
            if (currentRenderXp != targetXp) xpAnimTimer.Start();

            lblLevelXP.Text = $"⭐ Level: {_user.Level}   |   XP: {_user.XP} / {requiredXp}";
            lblCoinsFreezes.Text = $"🪙 Coins: {_user.Coins}   |   ❄️ Freezes: {_user.AvailableFreezes}";

            var habits = _habitService.GetHabits(_user.UserId);
            int comp = habits.Count(h => _habitService.IsCompletedToday(h.HabitId));
            
            lblMotivational.Text = comp == habits.Count && habits.Count > 0 
                ? "🔥 Incredible! You crushed all your routines!" 
                : $"📈 Progress: {comp}/{habits.Count} habits completed today.";

            // Render highly-interactive Habit Cards instead of a Grid!
            flpHabits.Controls.Clear();
            foreach (var h in habits.OrderByDescending(h => h.CurrentStreak))
            {
                bool isDone = _habitService.IsCompletedToday(h.HabitId);
                
                var card = new RoundedPanel
                {
                    Width = 380, Height = 130, Margin = new Padding(10), BorderRadius = 15,
                    GradientTop = isDone ? Color.FromArgb(30, 60, 40) : Color.FromArgb(38, 38, 42),
                    GradientBottom = isDone ? Color.FromArgb(20, 40, 30) : Color.FromArgb(30, 30, 34),
                    Tag = h.HabitId 
                };

                var lblName = new Label { Text = h.HabitName, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = isDone ? Color.MediumSeaGreen : _fg, Top = 15, Left = 20, AutoSize = true, BackColor = Color.Transparent };
                var lblDiff = new Label { Text = "Diff: " + h.Difficulty, Font = new Font("Segoe UI", 10), ForeColor = Color.LightGray, Top = 50, Left = 20, AutoSize = true, BackColor = Color.Transparent };
                var lblStreak = new Label { Text = $"🔥 Streak: {h.CurrentStreak}", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Orange, Top = 80, Left = 20, AutoSize = true, BackColor = Color.Transparent };
                var lblRemind = new Label { Text = $"🔔 {h.ReminderTime:hh\\:mm}", Font = new Font("Segoe UI", 10), ForeColor = Color.Khaki, Top = 83, Left = 150, AutoSize = true, BackColor = Color.Transparent };

                var btnComplete = new ModernButton
                {
                    Text = isDone ? "✔️ DONE" : "Complete",
                    Width = 100, Height = 40, Top = 40, Left = 260, BorderRadius = 20,
                    NormalColor = isDone ? Color.FromArgb(40,40,40) : Color.MediumSeaGreen,
                    HoverColor = isDone ? Color.FromArgb(50,50,50) : Color.SeaGreen,
                    ForeColor = isDone ? Color.LightGreen : Color.White
                };
                btnComplete.Click += (s, e) => {
                    if (!isDone) 
                    {
                        var result = _habitService.CompleteHabit(h, _user);
                        if (result != null)
                        {
                            new ToastNotification($"+{result.XpGained} XP! 🎉", Color.MediumSeaGreen).ShowToast(this);
                            if (result.LeveledUp) new ToastNotification($"LEVEL UP! {_user.Level} 🏆", Color.Gold).ShowToast(this);
                            if (result.CoinsGained > 0) new ToastNotification($"+{result.CoinsGained} 🪙", Color.Orange).ShowToast(this);
                            LoadData(); LoadMiniDashboard();
                        }
                    } 
                    else { new ToastNotification("Already Done! Good job!", Color.Gray).ShowToast(this); }
                };

                card.Controls.Add(lblName);
                card.Controls.Add(lblDiff);
                card.Controls.Add(lblStreak);
                card.Controls.Add(lblRemind);
                card.Controls.Add(btnComplete);

                // Hover effects on the card!
                card.MouseEnter += (s, e) => { if(!isDone) { card.GradientTop = Color.FromArgb(45, 45, 50); card.Invalidate(); } };
                card.MouseLeave += (s, e) => { if(!isDone) { card.GradientTop = Color.FromArgb(38, 38, 42); card.Invalidate(); } };

                flpHabits.Controls.Add(card);
            }
        }

        private void LoadMiniDashboard()
        {
            var habits = _habitService.GetHabits(_user.UserId);
            int todayDone = habits.Count(h => _habitService.IsCompletedToday(h.HabitId));
            int bestStreak = habits.Any() ? habits.Max(h => h.CurrentStreak) : 0;

            lblTodayStats.Text = $"📊 Today's Pulse:\n\n" +
                                 $"✅ Completed: {todayDone}\n" +
                                 $"❌ Remaining: {habits.Count - todayDone}\n\n" +
                                 $"🏆 Best Active Streak: {bestStreak} Days\n\n" +
                                 $"💡 \"Keep showing up. You're building an unstoppable rhythm.\"";

            // Load mini chart
            var data = _analyticsService.GetLast7DaysCompletions(_user.UserId);
            miniChart.Series["Completions"].Points.Clear();
            foreach (var kvp in data.OrderBy(k => k.Key))
            {
                miniChart.Series["Completions"].Points.AddXY(kvp.Key.ToString("ddd"), kvp.Value);
            }
        }

        private Habit GetSelectedCardHabit()
        {
            // Prompt the User to click the Edit action instead. 
            MessageBox.Show("Please use the 'Complete' button on the cards, or click Edit directly if you want to edit.", "Action Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null; // The Edit/Delete buttons require a rework, we'll open a raw list to select.
        }

        private void XpAnimTimer_Tick(object sender, EventArgs e)
        {
            if (currentRenderXp < targetXp) currentRenderXp += 3;
            else if (currentRenderXp > targetXp) currentRenderXp -= 3;
            
            if (Math.Abs(currentRenderXp - targetXp) <= 3) 
            {
                currentRenderXp = targetXp;
                xpAnimTimer.Stop();
            }
            customXPBar.Invalidate();
        }

        private void BtnAdd_Click(object s, EventArgs e) { using (var f = new AddEditHabitForm()) { if (f.ShowDialog() == DialogResult.OK) { f.HabitResult.UserId = _user.UserId; _habitService.AddHabit(f.HabitResult); LoadData(); LoadMiniDashboard(); } } }
        
        private string ShowNamePrompt(string title)
        {
            Form prompt = new Form() { Width = 400, Height = 180, FormBorderStyle = FormBorderStyle.FixedDialog, Text = title, StartPosition = FormStartPosition.CenterScreen, BackColor = Color.FromArgb(40,40,40), ForeColor = Color.White };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = "Enter Habit Name:", AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340 };
            Button confirmation = new Button() { Text = "Submit", Left = 260, Width = 100, Top = 90, DialogResult = DialogResult.OK, BackColor = Color.MediumSeaGreen, FlatStyle = FlatStyle.Flat };
            confirmation.FlatAppearance.BorderSize = 0;
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private void BtnEdit_Click(object s, EventArgs e) 
        { 
            string input = ShowNamePrompt("Edit Routine");
            if (string.IsNullOrEmpty(input)) return;
            var h = _habitService.GetHabits(_user.UserId).FirstOrDefault(x => x.HabitName.ToLower() == input.ToLower());
            if (h != null) { using (var f = new AddEditHabitForm(h)) { if (f.ShowDialog() == DialogResult.OK) { _habitService.UpdateHabit(f.HabitResult); LoadData(); LoadMiniDashboard(); } } }
        }

        private void BtnDelete_Click(object s, EventArgs e) 
        { 
            string input = ShowNamePrompt("Delete Routine");
            if (string.IsNullOrEmpty(input)) return;
            var h = _habitService.GetHabits(_user.UserId).FirstOrDefault(x => x.HabitName.ToLower() == input.ToLower());
            if (h != null && MessageBox.Show($"Delete '{h.HabitName}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) { _habitService.DeleteHabit(h.HabitId, _user.UserId); LoadData(); LoadMiniDashboard(); } 
        }
        private void BtnStore_Click(object s, EventArgs e) { using (var f = new StoreForm(_user, _dbPath)) { f.ShowDialog(); LoadData(); } }
        private void BtnAnalytics_Click(object s, EventArgs e) { using (var f = new AnalyticsForm(_user, _dbPath)) { f.ShowDialog(); } }
        
        private void ReminderTimer_Tick(object s, EventArgs e)
        {
            var now = DateTime.Now.TimeOfDay;
            var habits = _habitService.GetHabits(_user.UserId);
            foreach (var h in habits)
            {
                if (Math.Abs((h.ReminderTime - now).TotalMinutes) < 1.0 && !_habitService.IsCompletedToday(h.HabitId))
                {
                    reminderTimer.Stop();
                    new ToastNotification($"🔔 Reminder: {h.HabitName}", Color.DarkViolet).ShowToast(this);
                    reminderTimer.Start();
                }
            }
        }
    }
}
