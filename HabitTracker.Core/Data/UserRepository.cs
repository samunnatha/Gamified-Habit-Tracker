using HabitTracker.Core.Models;
using System;
using System.Data.SQLite;
using System.Collections.Generic;

namespace HabitTracker.Core.Data
{
    public class UserRepository
    {
        private readonly SqliteHelper _helper;

        public UserRepository(string dbPath)
        {
            _helper = new SqliteHelper(dbPath);
        }

        public User Authenticate(string username, string passwordHash)
        {
            using (var conn = _helper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT * FROM Users WHERE Username = @u AND PasswordHash = @p", conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", passwordHash);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Username = reader["Username"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                Level = Convert.ToInt32(reader["Level"]),
                                XP = Convert.ToInt32(reader["XP"]),
                                Coins = Convert.ToInt32(reader["Coins"]),
                                AvailableFreezes = Convert.ToInt32(reader["AvailableFreezes"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool Register(string username, string passwordHash)
        {
            try
            {
                var cmd = "INSERT INTO Users (Username, PasswordHash, Level, XP, Coins, AvailableFreezes) VALUES (@u, @p, 1, 0, 0, 1)";
                _helper.ExecuteNonQuery(cmd, 
                    new SQLiteParameter("@u", username),
                    new SQLiteParameter("@p", passwordHash));
                return true;
            }
            catch { return false; }
        }

        public void UpdateUserProgress(User user)
        {
            var cmd = "UPDATE Users SET Level = @l, XP = @x, Coins = @c, AvailableFreezes = @f WHERE UserId = @id";
            _helper.ExecuteNonQuery(cmd,
                new SQLiteParameter("@l", user.Level),
                new SQLiteParameter("@x", user.XP),
                new SQLiteParameter("@c", user.Coins),
                new SQLiteParameter("@f", user.AvailableFreezes),
                new SQLiteParameter("@id", user.UserId));
        }
    }
}
