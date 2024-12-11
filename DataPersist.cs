using D424;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424Weightlifting
{
    public class DataPersist
    {
        private DataPersist() { }

        private static DataPersist _instance;

        public static DataPersist Instance => _instance ??= new DataPersist();

        public int TotalProtein { get; set; }
        public int TotalCarbs { get; set; }
        public int TotalFats { get; set; }
        public int TotalCalories => (TotalProtein * 4) + (TotalCarbs * 4) + (TotalFats * 9);

        public async Task LoadDailyMacroTotalsAsync(Database database)
        {
            var macros = await database.GetDailyMacrosDatedAsync(DateTime.Today, UserSession.LoggedInUserId);
            if (macros != null)
            {
                TotalProtein = macros.Protein;
                TotalCarbs = macros.Carbs;
                TotalFats = macros.Fats;
            }
        }

        public async Task SaveDailyMacroTotalsAsync(Database database)
        {
            var macros = new D424.Macros
            {
                Protein = TotalProtein,
                Carbs = TotalCarbs,
                Fats = TotalFats,
                Calories = TotalCalories,
                Date = DateTime.Today
            };
            await database.SaveMacrosAsync(macros);
        }

        public void ResetDailyTotals()
        {
            TotalProtein = 0;
            TotalCarbs = 0;
            TotalFats = 0;
        }
        
    }
}
