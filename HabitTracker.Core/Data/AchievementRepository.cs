using HabitTracker.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace HabitTracker.Core.Data
{
    public class AchievementRepository
    {
        private readonly SqliteHelper _helper;

        public AchievementRepository(string dbPath)
        {
            _helper = new SqliteHelper(dbPath);
        }

        public List<Achievement> GetAllAchievements()
        {
            var list = new List<Achievement>();
            using (var conn = _helper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT * FROM Achievements", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Achievement
                        {
                            AchievementId = Convert.ToInt32(reader["AchievementId"]),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            RequiredValue = Convert.ToInt32(reader["RequiredValue"]),
                            Type = reader["Type"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        public List<int> GetUnlockedAchievementIds(int userId)
        {
            var list = new List<int>();
            using (var conn = _helper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT AchievementId FROM UserAchievements WHERE UserId = @u", conn))
                {
                    cmd.Parameters.AddWithValue("@u", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(Convert.ToInt32(reader["AchievementId"]));
                        }
                    }
                }
            }
            return list;
        }

        public void UnlockAchievement(int userId, int achievementId)
        {
            var cmd = "INSERT INTO UserAchievements (UserId, AchievementId, DateUnlocked) VALUES (@u, @a, @d)";
            _helper.ExecuteNonQuery(cmd,
                new SQLiteParameter("@u", userId),
                new SQLiteParameter("@a", achievementId),
                new SQLiteParameter("@d", DateTime.Now.ToString("o")));
        }
    }
}
