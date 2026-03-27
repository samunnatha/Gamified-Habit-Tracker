using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HabitTracker.Core.Models;
using HabitTracker.Core.Services;

namespace HabitTracker.WinForms.Forms
{
    public class MainForm : Form
    {
        private readonly string _dbPath;
        private User _user;
        
        // Refactored to use Services instead of Repositories
        private readonly HabitService _habitService;
        private readonly UserService _userService;

        private DataGridView dgvHabits;
        private ProgressBar pbXP;
        private Label lblLevelXP;
        private Label lblCoinsFreezes;
        private Timer reminderTimer;

        public MainForm(string dbPath, User loggedInUser)
        {
            _dbPath = dbPath;
            _user = loggedInUser;
            
            _habitService = new HabitService(dbPath);
            _userService = new UserService(dbPath);

            InitializeComponent();
            LoadData();
            ShowDailySummary();

            reminderTimer = new Timer { Interval = 60000 }; // check every minute
            reminderTimer.Tick += ReminderTimer_Tick;
            reminderTimer.Start();
        }

        private void InitializeComponent()
        {
            this.Text = $"Gamified Habit Tracker - Welcome {_user.Username}!";
            this.Size = new Size(850, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.WhiteSmoke };
            
            lblLevelXP = new Label { Top = 10, Left = 20, Width = 300, Font = new Font("Arial", 12, FontStyle.Bold) };
            pbXP = new ProgressBar { Top = 40, Left = 20, Width = 400, Height = 20, Maximum=100 };
            lblCoinsFreezes = new Label { Top = 20, Left = 450, Width = 200, Font = new Font("Arial", 10, FontStyle.Italic) };

            var btnBackup = new Button { Text = "Backup DB", Top = 20, Left = 700, Width = 100 };
            btnBackup.Click += BtnBackup_Click;

            panelTop.Controls.Add(lblLevelXP);
            panelTop.Controls.Add(pbXP);
            panelTop.Controls.Add(lblCoinsFreezes);
            panelTop.Controls.Add(btnBackup);

            var panelButtons = new Panel { Dock = DockStyle.Right, Width = 150, Padding = new Padding(10) };
            
            var btnComplete = new Button { Text = "Complete!", Dock = DockStyle.Top, Height = 60, BackColor = Color.LightGreen, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnComplete.Click += BtnComplete_Click;
            
            var btnAdd = new Button { Text = "Add Habit", Dock = DockStyle.Top, Height = 40, Margin = new Padding(0,10,0,10) };
            btnAdd.Click += BtnAdd_Click;
            var btnEdit = new Button { Text = "Edit Habit", Dock = DockStyle.Top, Height = 40, Margin = new Padding(0,0,0,10) };
            btnEdit.Click += BtnEdit_Click;
            var btnDelete = new Button { Text = "Delete Habit", Dock = DockStyle.Top, Height = 40, Margin = new Padding(0,0,0,10) };
            btnDelete.Click += BtnDelete_Click;
            var btnStore = new Button { Text = "Rewards Store 🛒", Dock = DockStyle.Top, Height = 50, Margin = new Padding(0,20,0,0), BackColor = Color.Gold};
            btnStore.Click += BtnStore_Click;
            var btnAnalytics = new Button { Text = "Analytics 📊", Dock = DockStyle.Top, Height = 50, Margin = new Padding(0,10,0,0), BackColor = Color.LightSkyBlue};
            btnAnalytics.Click += BtnAnalytics_Click;

            panelButtons.Controls.Add(btnAnalytics);
            panelButtons.Controls.Add(btnStore);
            panelButtons.Controls.Add(new Label{Height=20, Dock=DockStyle.Top});
            panelButtons.Controls.Add(btnDelete);
            panelButtons.Controls.Add(btnEdit);
            panelButtons.Controls.Add(btnAdd);
            panelButtons.Controls.Add(btnComplete);

            dgvHabits = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            
            this.Controls.Add(dgvHabits);
            this.Controls.Add(panelButtons);
            this.Controls.Add(panelTop);
        }

        private void LoadData()
        {
            _user = _userService.Authenticate(_user.Username, _user.PasswordHash);
            int requiredXp = (int)Math.Pow(_user.Level, 2) * 50; 
            pbXP.Maximum = requiredXp;
            pbXP.Value = Math.Min(_user.XP, requiredXp);
            
            lblLevelXP.Text = $"Level: {_user.Level} | XP: {_user.XP} / {requiredXp}";
            lblCoinsFreezes.Text = $"Coins: 🪙 {_user.Coins}   |   Freezes: ❄️ {_user.AvailableFreezes}";

            var habits = _habitService.GetHabits(_user.UserId);
            dgvHabits.DataSource = null; 
            dgvHabits.DataSource = habits.Select(h => new { 
                h.HabitId, 
                h.HabitName, 
                h.Difficulty, 
                Streak = h.CurrentStreak + "🔥", 
                Reminder = h.ReminderTime.ToString(@"hh\:mm"),
                Completed = _habitService.IsCompletedToday(h.HabitId) ? "Yes" : "No"
            }).ToList();

            foreach (DataGridViewRow r in dgvHabits.Rows)
            {
                if (r.Cells["Completed"].Value.ToString() == "Yes")
                    r.DefaultCellStyle.BackColor = Color.LightCyan;
            }
        }

        private void ShowDailySummary()
        {
            int total = _habitService.GetHabits(_user.UserId).Count;
            if (total > 0)
            {
                int completed = _habitService.GetHabits(_user.UserId).Count(h => _habitService.IsCompletedToday(h.HabitId));
                MessageBox.Show($"Daily Summary:\nYou have completed {completed}/{total} habits today!", "Daily Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private Habit GetSelected()
        {
            if (dgvHabits.SelectedRows.Count > 0)
            {
                int id = (int)dgvHabits.SelectedRows[0].Cells["HabitId"].Value;
                return _habitService.GetHabits(_user.UserId).FirstOrDefault(h => h.HabitId == id);
            }
            return null;
        }

        private void BtnAdd_Click(object s, EventArgs e)
        {
            using (var f = new AddEditHabitForm()) { if (f.ShowDialog() == DialogResult.OK) { f.HabitResult.UserId = _user.UserId; _habitService.AddHabit(f.HabitResult); LoadData(); } }
        }

        private void BtnEdit_Click(object s, EventArgs e)
        {
            var h = GetSelected();
            if (h != null) { using (var f = new AddEditHabitForm(h)) { if (f.ShowDialog() == DialogResult.OK) { _habitService.UpdateHabit(f.HabitResult); LoadData(); } } }
        }

        private void BtnDelete_Click(object s, EventArgs e)
        {
            var h = GetSelected();
            if (h != null && MessageBox.Show($"Delete '{h.HabitName}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) { _habitService.DeleteHabit(h.HabitId, _user.UserId); LoadData(); }
        }

        private void BtnComplete_Click(object s, EventArgs e)
        {
            var h = GetSelected();
            if (h != null)
            {
                var result = _habitService.CompleteHabit(h, _user);
                if (result != null)
                {
                    string msg = $"+{result.XpGained} XP!\n+{result.CoinsGained} 🪙 Coins!";
                    if (result.LeveledUp) msg += $"\n\nLEVEL UP! You are now Level {_user.Level}!";
                    if (result.NewAchievements != null && result.NewAchievements.Any()) {
                        msg += "\n\n🏆 Achievements Unlocked:\n" + string.Join("\n", result.NewAchievements.Select(a => $"- {a.Name}: {a.Description}"));
                    }
                    MessageBox.Show(msg, "Habit Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
            }
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
                    MessageBox.Show($"🔔 Smart Reminder: Time to complete '{h.HabitName}'!", "Reminder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    reminderTimer.Start();
                }
            }
        }

        private void BtnBackup_Click(object s, EventArgs e)
        {
            using (var sfd = new SaveFileDialog() { Filter = "SQLite|*.db", FileName = "backup.db" })
                if (sfd.ShowDialog() == DialogResult.OK) try { File.Copy(_dbPath, sfd.FileName, true); MessageBox.Show("Backup created!"); } catch (Exception ex) { MessageBox.Show("Error: "+ex.Message); }
        }
    }
}
