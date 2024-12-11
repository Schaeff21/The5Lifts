using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using D424;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;


namespace D424
{
    public interface ITrackable
    {
        void Track();
    }

    public class TrackingUser
    {
        public int UserId { get; set; }
        public DateTime Date { get; set; }
    }
    public static class UserSession
    {
        public static int LoggedInUserId { get; set; }
        public static string LoggedInUserName { get; set; }
        public static bool IsUserLoggedIn { get; set; }
        public static void ClearSession()
        {
            LoggedInUserId = 0;
            LoggedInUserName = string.Empty;
        }
    }

        public class User
        {
            [PrimaryKey, AutoIncrement]
            public int UserId { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
            public int Height { get; set; }
            public int Weight { get; set; }


            public void SetPassword(string password)
            {
                Password = HashPassword(password);
            }
            public bool ValidatePassword(string password)
            {
                return Password == HashPassword(password);
            }
            private string HashPassword(string password)
            {
                using (var sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    return Convert.ToBase64String(bytes);
                }
            }
            public string GetPassword()
            {
                return Password;
            }
        }


        public class DailyLifts : TrackingUser, ITrackable
        {
            [PrimaryKey, AutoIncrement]
            public int LiftId { get; set; }
            public int UserId { get; set; }
            public string LiftName { get; set; }
            public int TotalSets { get; set; }
            public string DateOnly { get; set; }
            public string LiftSummary => $"{LiftName} - {DateOnly}";

            [Ignore]
            public List<LiftSet> SetDetails { get; set; }

            public void Track()
            {
                Console.WriteLine($"Tracking lift: {LiftName}, Total sets: {TotalSets}, Date: {Date}");

            }
        }
        public class LiftSet : ITrackable
        {
            [PrimaryKey, AutoIncrement]
            public int SetId { get; set; }
            public int LiftId { get; set; }
            public int SetNumber { get; set; }
            public int Reps { get; set; }
            public int Weight { get; set; }

            public void Track()
            {
                Console.WriteLine($"Tracking LiftSet: SetId = {SetId}, LiftId = {LiftId}, SetNumber = {SetNumber}" +
                    $"Reps = {Reps}, and Weight = {Weight}");
            }
        }

        public class DailyCardio : TrackingUser, ITrackable
        {
            [PrimaryKey, AutoIncrement]
            public int CardioId { get; set; }
            public string Type { get; set; }
            public int Duration { get; set; }

            public void Track()
            {
                Console.WriteLine($"Tracking Cardio: {Type}, Duration: {Duration}, Date: {Date}");
            }
        }
        public class Macros : TrackingUser, ITrackable
        {
            [PrimaryKey, AutoIncrement]
            public int MacroTrackId { get; set; }
            public int UserId { get; set; }
            public int Protein { get; set; }
            public int Carbs { get; set; }
            public int Fats { get; set; }
            public int Calories { get; set; }
            public string DateOnly { get; set; }
            [Ignore]
            public User User { get; set; }
            public void Track()
            {
                Console.WriteLine($"Tracking macros: Protein: {Protein}g, Carbs: {Carbs}g, Fats: {Fats}g, Calories: {Calories}, Date: {Date}");
            }
        }

        public class Database
        {
            private readonly SQLiteAsyncConnection _database;

            public Database(string dbPath)
            {
                _database = new SQLiteAsyncConnection(dbPath);
                _database.CreateTableAsync<User>().Wait();
                _database.CreateTableAsync<DailyLifts>().Wait();
                _database.CreateTableAsync<Macros>().Wait();
                _database.CreateTableAsync<LiftSet>().Wait();
            }
            public async Task<int> InsertAsync(ITrackable trackable)
            {
                switch (trackable)
                {
                    case DailyLifts lifts:
                        return await _database.InsertAsync(lifts);
                    case Macros macros:
                        return await _database.InsertAsync(macros);
                    case DailyCardio cardio:
                        return await _database.InsertAsync(cardio);
                    case LiftSet liftset:
                        return await _database.InsertAsync(liftset);
                    default:
                        return 0;
                }
            }
            public async Task<List<ITrackable>> GetAllTrackableAsync()
            {
                var lifts = await _database.Table<DailyLifts>().ToListAsync();
                var macros = await _database.Table<Macros>().ToListAsync();
                var cardio = await _database.Table<DailyCardio>().ToListAsync();
                var liftSets = await _database.Table<LiftSet>().ToListAsync();

                var allTrackables = new List<ITrackable>();
                allTrackables.AddRange(lifts.Cast<ITrackable>());
                allTrackables.AddRange(macros.Cast<ITrackable>());
                allTrackables.AddRange(cardio.Cast<ITrackable>());
                allTrackables.AddRange(liftSets.Cast<ITrackable>());

                return allTrackables;
            }
            public async Task<string> NewUserRegisterAsync(string username, string password, string email, int height, int weight)
            {
                var existingUser = await _database.Table<User>().Where(u => u.UserName == username).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    return "Username already exists, please create a new one";
                }

                var newUser = new User
                {
                    UserName = username,
                    Email = email,
                    Height = height,
                    Weight = weight
                };
                newUser.SetPassword(password);
                await _database.InsertAsync(newUser);

                return "User successfully added";
            }

            public async Task<List<DailyLifts>> GetLiftHistory7DaysAsync(int userId)
            {
                try
                {
                    DateTime pastWeekLifts = DateTime.Today.AddDays(-6);
                    var liftHistory = await _database.Table<DailyLifts>()
                        .Where(lift => lift.UserId == userId && lift.Date >= pastWeekLifts)
                        .OrderBy(lift => lift.Date)
                        .ToListAsync();

                    return liftHistory ?? new List<DailyLifts>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in getting lifts: {ex.Message}");
                    return new List<DailyLifts>();
                }
            }

            //Get user for log in
            public async Task<User> GetUserAsync(string userName, string password)
            {
                var user = await _database.Table<User>().Where(u => u.UserName == userName).FirstOrDefaultAsync();
                if (user != null && user.ValidatePassword(password))
                {
                    return user;
                }
                return null;
            }
            public async Task<int> SaveMacrosAsync(Macros macros)
            {
                macros.DateOnly = macros.Date.ToString("yyyy-MM-dd");
                var existingMacros = await _database.Table<Macros>().FirstOrDefaultAsync
                    (m => m.UserId == macros.UserId && m.DateOnly == macros.DateOnly);

                if (existingMacros != null)
                {
                    existingMacros.Protein = macros.Protein;
                    existingMacros.Carbs = macros.Carbs;
                    existingMacros.Fats = macros.Fats;
                    existingMacros.Calories = macros.Calories;
                    return await _database.UpdateAsync(existingMacros);
                }
                else
                {
                    return await _database.InsertAsync(macros);
                }
            }
            public async Task<Macros> GetDailyMacrosDatedAsync(DateTime date, int userId)
            {
                string dateOnly = date.ToString("yyyy-MM-dd");

                return await _database.Table<Macros>()
                .FirstOrDefaultAsync(m => m.UserId == userId && m.DateOnly == dateOnly);

            }

            public async Task<List<DailyLifts>> GetLiftsByDateAsync(string date, int userId)
            {
                return await _database.Table<DailyLifts>()
                       .Where(lift => lift.UserId == userId && lift.DateOnly == date)
                       .ToListAsync();
            }

            public async Task<int> DeleteLiftAsync(DailyLifts lift)
            {
                try
                {
                    var setsToDelete = await _database.Table<LiftSet>().Where
                            (s => s.LiftId == lift.LiftId).ToListAsync();
                    foreach (var set in setsToDelete)
                    {
                        await _database.DeleteAsync(set);
                    }
                    return await _database.DeleteAsync(lift);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting: {ex.Message}");
                    throw;
                }

            }
            public async Task<int> UpdateLiftAsync(DailyLifts lift)
            {
                return await _database.UpdateAsync(lift);
            }
            //TESTING TO VIEW USERS
            public async Task<List<User>> GetAllUsersAsync()
            {
                return await _database.Table<User>().ToListAsync();
            }

            public async Task ClearDatabaseAsync()
            {
                await _database.DeleteAllAsync<User>();
                await _database.DeleteAllAsync<DailyLifts>();
                await _database.DeleteAllAsync<Macros>();

                Console.WriteLine("Database cleared.");
            }




            //Sample data for testing purposes
            public async Task SampleUserAsync()
            {
                var sampleUser = new User
                {
                    UserName = "user",
                    Email = "sample@wgu.edu",
                    Height = 70,
                    Weight = 195
                };
                sampleUser.SetPassword("password");
                await _database.InsertAsync(sampleUser);
            }
            public async Task<int> InsertAsync(DailyLifts dailyLifts)
            {
                dailyLifts.DateOnly = dailyLifts.Date.ToString("yyyy-MM-dd");
                return await _database.InsertAsync(dailyLifts);
            }
            public async Task<List<DailyLifts>> GetLiftsByNameAsync(string liftName)
            {
                return await _database.Table<DailyLifts>().Where
                    (lift => lift.LiftName == liftName).OrderBy
                    (lift => lift.Date).ToListAsync();
            }

            public async Task<List<DailyLifts>> GetLiftHistoryAsync()
            {
                try
                {
                    var liftHistory = await _database.Table<DailyLifts>()
                        .OrderBy(lift => lift.Date).ToListAsync();
                    return liftHistory;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting lift history: {ex.Message}");
                    return new List<DailyLifts>();
                }
            }
            public async Task<DailyLifts> GetLiftByIdAsync(int liftId)
            {
                try
                {
                    return await _database.Table<DailyLifts>().Where(l => l.LiftId == liftId).FirstOrDefaultAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting lift ID: {ex.Message}");
                    return null;
                }
            }

            public async Task<int> InsertAsync(Macros macros)
            {
                macros.DateOnly = macros.Date.ToString("yyyy-MM-dd");
                return await _database.InsertAsync(macros);
            }

            public async Task<Macros> GetDailyMacrosAsync()
            {
                return await _database.Table<Macros>().FirstOrDefaultAsync(m => m.Date == DateTime.Today);
            }

            public async Task<int> InsertSetAsync(LiftSet set)
            {
                return await _database.InsertAsync(set);
            }
            public async Task<List<LiftSet>> GetSetsByLiftIdAsync(int liftId)
            {
                try
                {
                    return await _database.Table<LiftSet>().Where(s => s.LiftId == liftId).ToListAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting results: {ex.Message}");
                    return new List<LiftSet>();
                }
            }
            public async Task<int> InsertLiftWithSetsAsync(DailyLifts lift, List<LiftSet> sets, int userId)
            {
                try
                {
                    lift.UserId = userId;
                    int result = await _database.InsertAsync(lift);

                    foreach (var set in sets)
                    {
                        set.LiftId = lift.LiftId;
                        await _database.InsertAsync(set);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error, failed to insert: {ex.Message}");
                    throw;
                }
            }

            public async Task<DailyLifts> GetLiftWithSetsAsync(int liftId)
            {
                try
                {
                    var lift = await _database.Table<DailyLifts>().Where(l => l.LiftId == liftId).FirstOrDefaultAsync();

                    lift.SetDetails = await _database.Table<LiftSet>().Where(s => s.LiftId == liftId).ToListAsync();
                    return lift;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error failed retreiving lift: {ex.Message}");
                    return null;
                }
            }

            public async Task<int> UpdateSetAsync(LiftSet set)
            {
                return await _database.UpdateAsync(set);
            }

            public async Task<int> DeleteLiftWithSetsAsync(int liftId, int userId)
            {
                try
                {
                    var lift = await _database.Table<DailyLifts>()
                    .FirstOrDefaultAsync(l => l.LiftId == liftId && l.UserId == userId);
                    if (lift == null)
                        return 0;

                    var setsToDelete = await _database.Table<LiftSet>().Where(s => s.LiftId == liftId).ToListAsync();
                    foreach (var set in setsToDelete)
                    {
                        await _database.DeleteAsync(set);
                    }
                    return await _database.DeleteAsync<DailyLifts>(liftId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting sets: {ex.Message}");
                    throw;
                }
            }

            public async Task<List<DailyLifts>> GetLiftsByDateWithSetsAsync(string date, int userId)
            {
                try
                {

                    var lifts = await _database.Table<DailyLifts>()
                                .Where(lift => lift.UserId == userId && lift.DateOnly == date)
                                .ToListAsync();
                    foreach (var lift in lifts)
                    {
                        lift.SetDetails = await _database.Table<LiftSet>().Where(s => s.LiftId == lift.LiftId).ToListAsync();
                    }

                    return lifts;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving lifts and sets: {ex.Message}");
                    return new List<DailyLifts>();
                }
            }
        }
}




