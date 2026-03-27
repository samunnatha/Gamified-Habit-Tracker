using HabitTracker.Core.Data;
using HabitTracker.Core.Models;
using System.Collections.Generic;

namespace HabitTracker.Core.Services
{
    public class AchievementService
    {
        private readonly AchievementRepository _achRepo;

        public AchievementService(string dbPath)
        {
            _achRepo = new AchievementRepository(dbPath);
        }

        public List<Achievement> GetUnlockedAchievements(int userId)
        {
            var unlockedIds = _achRepo.GetUnlockedAchievementIds(userId);
            var all = _achRepo.GetAllAchievements();
            return all.FindAll(a => unlockedIds.Contains(a.AchievementId));
        }

        public List<Achievement> GetLockedAchievements(int userId)
        {
            var unlockedIds = _achRepo.GetUnlockedAchievementIds(userId);
            var all = _achRepo.GetAllAchievements();
            return all.FindAll(a => !unlockedIds.Contains(a.AchievementId));
        }

        public List<Achievement> CheckAchievements(User user, Habit habit, int totalCompletedHabits)
        {
            var recentlyUnlocked = new List<Achievement>();
            var locked = GetLockedAchievements(user.UserId);

            foreach (var ach in locked)
            {
                bool unlock = false;
                switch (ach.Type)
                {
                    case "TotalCompletions":
                        unlock = totalCompletedHabits >= ach.RequiredValue;
                        break;
                    case "Streak":
                        unlock = habit.CurrentStreak >= ach.RequiredValue;
                        break;
                    case "Level":
                        unlock = user.Level >= ach.RequiredValue;
                        break;
                }

                if (unlock)
                {
                    _achRepo.UnlockAchievement(user.UserId, ach.AchievementId);
                    recentlyUnlocked.Add(ach);
                }
            }
            return recentlyUnlocked;
        }
    }
}
