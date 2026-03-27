using System;

namespace HabitTracker.Core.Models
{
    public class Achievement
    {
        public int AchievementId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RequiredValue { get; set; }
        public string Type { get; set; } // "Streak", "TotalCompletions", "Level"
    }

    public class UserAchievement
    {
        public int UserId { get; set; }
        public int AchievementId { get; set; }
        public DateTime DateUnlocked { get; set; }
    }
}
