using HabitTracker.Core.Data;
using System;
using System.Collections.Generic;

namespace HabitTracker.Core.Services
{
    public class AnalyticsService
    {
        private readonly SqliteHelper _helper;

        public AnalyticsService(string dbPath)
        {
            _helper = new SqliteHelper(dbPath);
        }

        // Returns date vs completion count for the last 7 days
        public Dictionary<DateTime, int> GetLast7DaysCompletions(int userId)
        {
            var data = new Dictionary<DateTime, int>();
            var startDate = DateTime.Today.AddDays(-6);

            for (int i = 0; i < 7; i++)
            {
                data[startDate.AddDays(i)] = 0;
            }

            using (var conn = _helper.GetConnection())
            {
                conn.Open();
                var cmdText = @"
                    SELECT hl.Date, COUNT(hl.LogId) as Cnt
                    FROM HabitLogs hl
                    INNER JOIN Habits h ON hl.HabitId = h.HabitId
                    WHERE h.UserId = @u AND hl.Completed = 1 AND hl.Date >= @ds
                    GROUP BY hl.Date";
                
                using (var cmd = new System.Data.SQLite.SQLiteCommand(cmdText, conn))
                {
                    cmd.Parameters.AddWithValue("@u", userId);
                    cmd.Parameters.AddWithValue("@ds", startDate.ToString("yyyy-MM-dd"));
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (DateTime.TryParse(reader["Date"].ToString(), out DateTime date))
                            {
                                if (data.ContainsKey(date.Date))
                                {
                                    data[date.Date] = Convert.ToInt32(reader["Cnt"]);
                                }
                            }
                        }
                    }
                }
            }
            return data;
        }
    }
}
