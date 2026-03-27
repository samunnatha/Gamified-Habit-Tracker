using HabitTracker.Core.Data;
using HabitTracker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HabitTracker.Core.Services
{
    public class HabitService
    {
        private readonly HabitRepository _habitRepo;
        private readonly HabitLogRepository _logRepo;
        private readonly UserRepository _userRepo;
        private readonly AchievementService _achService;
        private readonly RewardRepository _rewardRepo; // Keeping this since it unlocks level-based rewards

        public HabitService(string dbPath)
        {
            _habitRepo = new HabitRepository(dbPath);
            _logRepo = new HabitLogRepository(dbPath);
            _userRepo = new UserRepository(dbPath);
            _achService = new AchievementService(dbPath);
            _rewardRepo = new RewardRepository(dbPath);
        }

        public List<Habit> GetHabits(int userId) => _habitRepo.GetHabitsByUser(userId);
        public void AddHabit(Habit h) => _habitRepo.AddHabit(h);
        public void UpdateHabit(Habit h) => _habitRepo.UpdateHabit(h);
        public void DeleteHabit(int id, int userId) => _habitRepo.DeleteHabit(id, userId);

        public bool IsCompletedToday(int habitId) => _logRepo.IsHabitCompletedToday(habitId);
        
        // Smart Reminders logic (predictive based on previous logs)
        public TimeSpan GetSmartReminderTime(int habitId, TimeSpan defaultTime)
        {
            // For now, return default time. A full calculation would AVG out previous completion times.
            // Placeholder for AI/Smart prediction
            return defaultTime; 
        }

        public string SuggestHabits(List<Habit> existingHabits)
        {
            var suggestions = new[] { "Drink water", "Read 10 pages", "Meditate", "Exercise for 20m" };
            var currentNames = existingHabits.Select(h => h.HabitName.ToLower()).ToList();
            var missing = suggestions.Where(s => !currentNames.Contains(s.ToLower())).ToList();
            if (missing.Any())
            {
                var rand = new Random();
                return missing[rand.Next(missing.Count)];
            }
            return "Take a break!";
        }

        public class CompletionResult
        {
            public int XpGained { get; set; }
            public int CoinsGained { get; set; }
            public bool LeveledUp { get; set; }
            public List<Achievement> NewAchievements { get; set; }
        }

        public CompletionResult CompleteHabit(Habit habit, User user)
        {
            if (IsCompletedToday(habit.HabitId)) return null;

            // Apply Streak Freeze
            if (_logRepo.DidMissYesterday(habit.HabitId) && habit.CurrentStreak > 0)
            {
                if (user.AvailableFreezes > 0) user.AvailableFreezes--;
                else habit.CurrentStreak = 0;
            }

            // Gamification logic
            int xpGained = habit.Difficulty.ToLower() == "hard" ? 30 : (habit.Difficulty.ToLower() == "medium" ? 20 : 10);
            int coinsGained = xpGained / 2; // Earn half as many coins as XP

            int oldLevel = user.Level;
            habit.CurrentStreak++;
            user.XP += xpGained;
            user.Coins += coinsGained;
            user.Level = (int)Math.Sqrt(user.XP / 50.0) + 1;

            _logRepo.AddLog(new HabitLog { HabitId = habit.HabitId, Date = DateTime.Now, Completed = true });
            _habitRepo.UpdateHabit(habit);
            _userRepo.UpdateUserProgress(user);

            // Unlock Rewards based on level
            if (user.Level > oldLevel) _rewardRepo.UnlockEligibleRewards(user);

            // Check Achievements
            // Dummy total completions for now
            int total = 1; // You'd compute this from _logRepo for all user's habits
            var achs = _achService.CheckAchievements(user, habit, total);

            return new CompletionResult
            {
                XpGained = xpGained,
                CoinsGained = coinsGained,
                LeveledUp = user.Level > oldLevel,
                NewAchievements = achs
            };
        }
    }
}
