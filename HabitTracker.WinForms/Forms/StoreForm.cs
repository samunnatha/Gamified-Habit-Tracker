using System;
using System.Drawing;
using System.Windows.Forms;
using HabitTracker.Core.Models;
using HabitTracker.Core.Services;

namespace HabitTracker.WinForms.Forms
{
    public class StoreForm : Form
    {
        private User _user;
        private RewardService _rewardService;
        private UserService _userService;

        private FlowLayoutPanel flowLayout;
        private Label lblCoins;

        public StoreForm(User user, string dbPath)
        {
            _user = user;
            _rewardService = new RewardService(dbPath);
            _userService = new UserService(dbPath);
            InitializeComponent();
            LoadStore();
        }

        private void InitializeComponent()
        {
            this.Text = "Rewards Store";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            lblCoins = new Label { Top = 20, Left = 20, Width = 300, Font = new Font("Arial", 12, FontStyle.Bold), Text = $"Your Coins: 🪙 {_user.Coins}" };
            this.Controls.Add(lblCoins);

            flowLayout = new FlowLayoutPanel
            {
                Top = 60, Left = 20, Width = 440, Height = 280, AutoScroll = true, BackColor = Color.WhiteSmoke
            };

            this.Controls.Add(flowLayout);
        }

        private void LoadStore()
        {
            flowLayout.Controls.Clear();
            var items = _rewardService.GetAvailableStoreItems();

            foreach (var item in items)
            {
                var pnl = new Panel { Width = 400, Height = 80, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(5) };
                pnl.Controls.Add(new Label { Text = item.Name, Top = 10, Left = 10, Font = new Font("Arial", 10, FontStyle.Bold), AutoSize = true });
                pnl.Controls.Add(new Label { Text = item.Description, Top = 35, Left = 10, Width = 280 });
                
                var btnBuy = new Button { Text = $"Buy ({item.Cost} 🪙)", Top = 25, Left = 300, Width = 80, BackColor = Color.Gold };
                btnBuy.Click += (s, e) => {
                    if (_rewardService.PurchaseItem(_user, item))
                    {
                        _userService.RefreshProgress(_user);
                        lblCoins.Text = $"Your Coins: 🪙 {_user.Coins}";
                        MessageBox.Show($"Successfully purchased {item.Name}!", "Purchase Successful");
                    }
                    else
                    {
                        MessageBox.Show("Not enough coins!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                pnl.Controls.Add(btnBuy);
                flowLayout.Controls.Add(pnl);
            }
        }
    }
}
