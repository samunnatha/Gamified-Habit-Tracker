using HabitTracker.Core.Models;
using HabitTracker.Core.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace HabitTracker.WinForms.Forms
{
    public class AnalyticsForm : Form
    {
        private User _user;
        private AnalyticsService _analyticsService;
        private HabitService _habitService;

        private Chart _chart;
        private Label _lblAI;

        public AnalyticsForm(User user, string dbPath)
        {
            _user = user;
            _analyticsService = new AnalyticsService(dbPath);
            _habitService = new HabitService(dbPath);
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Analytics & Insights - WOW Feature";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // Chart Control
            _chart = new Chart();
            _chart.Top = 20;
            _chart.Left = 20;
            _chart.Width = 740;
            _chart.Height = 400;

            var chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "Date";
            chartArea.AxisY.Title = "Completions";
            chartArea.AxisY.Interval = 1;
            _chart.ChartAreas.Add(chartArea);

            var series = new Series("Habits");
            series.ChartType = SeriesChartType.Column;
            series.Color = Color.SteelBlue;
            series.IsValueShownAsLabel = true;
            _chart.Series.Add(series);

            this.Controls.Add(_chart);

            // AI Insight Panel
            var pnlAI = new Panel { Top = 440, Left = 20, Width = 740, Height = 100, BackColor = Color.LightYellow, BorderStyle = BorderStyle.FixedSingle };
            var lblTitle = new Label { Top = 10, Left = 10, Text = "🤖 AI Habit Suggestions & Insights", Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true };
            _lblAI = new Label { Top = 40, Left = 10, Width = 720, Font = new Font("Arial", 10, FontStyle.Italic) };
            
            pnlAI.Controls.Add(lblTitle);
            pnlAI.Controls.Add(_lblAI);

            this.Controls.Add(pnlAI);
        }

        private void LoadData()
        {
            var data = _analyticsService.GetLast7DaysCompletions(_user.UserId);
            
            _chart.Series["Habits"].Points.Clear();
            foreach (var kvp in data.OrderBy(k => k.Key))
            {
                _chart.Series["Habits"].Points.AddXY(kvp.Key.ToString("MM/dd"), kvp.Value);
            }

            var existingHabits = _habitService.GetHabits(_user.UserId);
            string suggestion = _habitService.SuggestHabits(existingHabits);

            _lblAI.Text = $"Based on your tracked habits, we noticed an opportunity to improve. \n\n✨ Suggestion: You should try adding '{suggestion}' to your daily routine!";
        }
    }
}
