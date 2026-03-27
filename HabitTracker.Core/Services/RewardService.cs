using HabitTracker.Core.Data;
using HabitTracker.Core.Models;
using System.Collections.Generic;

namespace HabitTracker.Core.Services
{
    public class RewardService
    {
        private readonly RewardRepository _rewardRepo;
        private readonly SqliteHelper _helper;

        public RewardService(string dbPath)
        {
            _rewardRepo = new RewardRepository(dbPath);
            _helper = new SqliteHelper(dbPath);
            InitializeStoreItems();
        }

        private void InitializeStoreItems()
        {
            // Seed virtual store items
            _helper.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS StoreItems (
                    ItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Cost INTEGER NOT NULL
                );
            ");

            var count = System.Convert.ToInt32(_helper.ExecuteScalar("SELECT COUNT(*) FROM StoreItems"));
            if (count == 0)
            {
                _helper.ExecuteNonQuery("INSERT INTO StoreItems (Name, Description, Cost) VALUES ('Golden Theme', 'Unlocks the prestigious Golden UI theme', 100)");
                _helper.ExecuteNonQuery("INSERT INTO StoreItems (Name, Description, Cost) VALUES ('Dark Mode', 'Unlock the sleek Dark Mode interface', 50)");
                _helper.ExecuteNonQuery("INSERT INTO StoreItems (Name, Description, Cost) VALUES ('Extra Freeze', 'Buy an extra streak freeze', 200)");
            }

            _helper.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS UserPurchases (
                    UserId INTEGER NOT NULL,
                    ItemId INTEGER NOT NULL,
                    PRIMARY KEY (UserId, ItemId)
                );
            ");
        }

        public class StoreItem
        {
            public int ItemId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int Cost { get; set; }
        }

        public List<StoreItem> GetAvailableStoreItems()
        {
            var items = new List<StoreItem>();
            using (var conn = _helper.GetConnection())
            {
                conn.Open();
                using (var cmd = new System.Data.SQLite.SQLiteCommand("SELECT * FROM StoreItems", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new StoreItem
                        {
                            ItemId = System.Convert.ToInt32(reader["ItemId"]),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            Cost = System.Convert.ToInt32(reader["Cost"])
                        });
                    }
                }
            }
            return items;
        }

        public bool PurchaseItem(User user, StoreItem item)
        {
            if (user.Coins >= item.Cost)
            {
                user.Coins -= item.Cost;
                
                // If it's a freeze, add it to user
                if (item.Name == "Extra Freeze")
                {
                    user.AvailableFreezes++;
                }

                _helper.ExecuteNonQuery("INSERT OR IGNORE INTO UserPurchases (UserId, ItemId) VALUES (@u, @i)",
                    new System.Data.SQLite.SQLiteParameter("@u", user.UserId),
                    new System.Data.SQLite.SQLiteParameter("@i", item.ItemId));
                
                return true;
            }
            return false; // Not enough coins
        }
    }
}
