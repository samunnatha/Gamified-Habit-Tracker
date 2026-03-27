using HabitTracker.Core.Data;
using HabitTracker.Core.Models;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace HabitTracker.Core.Services
{
    public class BackupService
    {
        private readonly HabitRepository _habitRepo;
        private readonly UserRepository _userRepo;
        
        public BackupService(string dbPath)
        {
            _habitRepo = new HabitRepository(dbPath);
            _userRepo = new UserRepository(dbPath);
        }

        public void ExportToJson(int userId, string exportPath)
        {
            var habits = _habitRepo.GetHabitsByUser(userId);
            
            // Simplified export for demonstration (in a full app, logs and achievements also export)
            var dataToExport = new Dictionary<string, object>
            {
                { "UserId", userId },
                { "Habits", habits }
            };

            var json = JsonConvert.SerializeObject(dataToExport, Formatting.Indented);
            File.WriteAllText(exportPath, json);
        }

        public void ImportFromJson(int userId, string importPath)
        {
            if (!File.Exists(importPath)) return;

            var json = File.ReadAllText(importPath);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (data.ContainsKey("Habits"))
            {
                var habitsDicts = data["Habits"] as System.Collections.ArrayList;
                if (habitsDicts != null)
                {
                    // Clear existing habits to mock a full restore
                    var existing = _habitRepo.GetHabitsByUser(userId);
                    foreach(var eh in existing) _habitRepo.DeleteHabit(eh.HabitId, userId);

                    foreach (var hd in habitsDicts)
                    {
                        var hdDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(hd.ToString());
                        var h = new Habit
                        {
                            UserId = userId,
                            HabitName = hdDict["HabitName"].ToString(),
                            Difficulty = hdDict["Difficulty"].ToString(),
                            CurrentStreak = System.Convert.ToInt32(hdDict["CurrentStreak"]),
                            ReminderTime = System.TimeSpan.Parse(hdDict["ReminderTime"].ToString()),
                            CreatedDate = System.Convert.ToDateTime(hdDict["CreatedDate"].ToString())
                        };
                        _habitRepo.AddHabit(h);
                    }
                }
            }
        }
    }
}
