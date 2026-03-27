using HabitTracker.Core.Data;
using HabitTracker.Core.Models;

namespace HabitTracker.Core.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepo;

        public UserService(string dbPath)
        {
            _userRepo = new UserRepository(dbPath);
        }

        public User Authenticate(string username, string passwordHash)
        {
            return _userRepo.Authenticate(username, passwordHash);
        }

        public bool Register(string username, string passwordHash)
        {
            return _userRepo.Register(username, passwordHash);
        }

        public void RefreshProgress(User user)
        {
            _userRepo.UpdateUserProgress(user);
        }
    }
}
