# Gamified Habit Tracker (Advanced Edition)

This repository contains the complete, gamified offline-first Habit Tracking application built with C# (.NET Framework 4.8), Windows Forms, and SQLite.

## ✨ New Features from Phase 2
- **Refactored Architecture**: The application strictly separates UI from Business Logic using `HabitTracker.Core.Services`.
- **Advanced Gamification**:
  - **Achievements System**: Tracks Total Completions, Streaks, and Level milestones to unlock achievements.
  - **Rewards Store**: You earn **Coins (🪙)** passively when completing habits, which can be spent to buy cosmetic themes or Extra Streak Freezes!
- **WOW Features!**
  - **Analytics Dashboard (📊)**: A beautiful WinForms Column Chart displaying your habit completion history over the last 7 days.
  - **AI Habit Suggestions (🤖)**: Automatically scans your existing habits to suggest new routines and practices.
  - **Smart Reminders Prediction (🔔)**: The timer actively evaluates whether a habit has been skipped before firing predictive alerts.

---

## 🚀 Extremely Clear Instructions on How to Run This

Since this application uses `.NET Framework 4.8` SDK-style projects natively supported by modern IDEs, please follow these EXACT steps to avoid any build errors:

### Option 1: The Easiest Way (Using Visual Studio 2019 / 2022)
1. **Open Visual Studio.**
2. Click **File -> Open -> Project/Solution**.
3. Navigate to `c:\--Files--\Programming\Projects\Gamified Habit Tracker\` and select **GamifiedHabitTracker.sln**.
4. Right-click the **HabitTracker.WinForms** project in the Solution Explorer and select **"Set as Startup Project"**.
5. Wait ~10 seconds for Visual Studio to automatically resolve the `System.Data.SQLite` NuGet dependencies.
6. Press **F5** (or click the Green "Start" button) to compile and run the application!

### Option 2: Using the Developer Command Prompt
If you prefer the command line:
1. Open the **Developer Command Prompt for VS 2022**.
2. Navigate to the project directory:
   ```cmd
   cd "C:\--Files--\Programming\Projects\Gamified Habit Tracker"
   ```
3. Restore NuGet packages and build the solution:
   ```cmd
   msbuild GamifiedHabitTracker.sln /t:Restore;Build
   ```
4. Run the executable:
   ```cmd
   "HabitTracker.WinForms\bin\Debug\net48\HabitTracker.WinForms.exe"
   ```

### Option 3: Dotnet CLI Command Prompt
Because we used modern SDK style `.csproj` files, you can also compile via standard dotnet tools:
```powershell
cd "c:\--Files--\Programming\Projects\Gamified Habit Tracker"
dotnet build
dotnet run --project HabitTracker.WinForms
```

---

## 🛠️ First Launch Setup
When you first run the app, a setup dialog will appear:
1. Click **Browse** and select any folder (e.g., your Desktop or Documents). This is where the local SQLite `.db` file will be generated and stored natively!
2. Click **Save & Initialize**. The tables (including new `Achievements`, `UserAchievements`, and `StoreItems`) will be seeded automatically.
3. Once running, click **Register** in the login form, invent a username/password, and log in to explore the Gamified Dashboard, Store, and Analytics tools!
