namespace HabitTracker.Core.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; } // Added for real WinForms login
        public int Level { get; set; }
        public int XP { get; set; }
        public int Coins { get; set; } // Virtual currency for rewards store
        public int AvailableFreezes { get; set; } // Streak freeze feature: 1 freeze allowed
    }
}
