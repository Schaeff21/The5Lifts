using D424;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using System.Linq.Expressions;
using System.Windows.Input;

namespace D424Weightlifting
{

    public partial class LiftingHistory : ContentPage
    {
        public List<string> LiftNames { get; set; }
        public List<DateTime> PastSevenDays { get; set; }
        public Dictionary<string, Dictionary<DateTime, string>> LiftingData {  get; set; }
        
        public LiftingHistory()
        {
            InitializeComponent();
            BindingContext = this;
      
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if(UserSession.IsUserLoggedIn)
            {
                await LoadLiftingHistory(UserSession.LoggedInUserId);
            }
            else
            {
                await DisplayAlert("Error", "No user is logged in.", "OK");
            }
        }

        private async Task LoadLiftingHistory(int userId)
        {
            try
            {
                PastSevenDays = Enumerable.Range(0, 7)
                    .Select(offset => DateTime.Today.AddDays(-offset))
                    .OrderByDescending(d => d)
                    .ToList();

                var liftHistoryData = await App.Database.GetLiftHistory7DaysAsync(userId);
                var macrosHistoryData = new Dictionary<string, D424.Macros>();

                foreach (var date in PastSevenDays)
                {
                    var macros = await App.Database.GetDailyMacrosDatedAsync(date, userId);
                    macrosHistoryData[date.ToString("yyyy-MM-dd")] = macros;
                }

                if (liftHistoryData == null || !liftHistoryData.Any())
                {
                    await DisplayAlert("Error", "No lifting history available.", "OK");
                    SetupLiftingHistoryGrid(new List<string>(), PastSevenDays, new List<DailyLifts>(), macrosHistoryData);
                    return;
                }

                foreach (var lift in liftHistoryData)
                {
                    lift.SetDetails = await App.Database.GetSetsByLiftIdAsync(lift.LiftId);
                }

                var liftNames = liftHistoryData.Select(l => l.LiftName).Distinct().ToList();
                SetupLiftingHistoryGrid(liftNames, PastSevenDays, liftHistoryData, macrosHistoryData);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
        }

        private async void SetupLiftingHistoryGrid(List<string> liftNames, List<DateTime> pastWeekDates, List<DailyLifts> liftHistoryData, Dictionary<string, D424.Macros> macrosHistoryData)
        {
            LiftingHistoryGrid.ColumnDefinitions.Clear();
            LiftingHistoryGrid.RowDefinitions.Clear();
            LiftingHistoryGrid.Children.Clear();

            LiftingHistoryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            foreach (var _ in pastWeekDates)
            {
                LiftingHistoryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            LiftingHistoryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header row
            foreach (var _ in liftNames)
            {
                LiftingHistoryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            LiftingHistoryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            LiftingHistoryGrid.Add(new Label
            {
                Text = "Lifts",
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }, 0, 0);

            for (int col = 0; col < pastWeekDates.Count; col++)
            {
                LiftingHistoryGrid.Add(new Label
                {
                    Text = pastWeekDates[col].ToString("MM/dd"),
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }, col + 1, 0);
            }

            for (int row = 0; row < liftNames.Count; row++)
            {
                var liftName = liftNames[row];

                LiftingHistoryGrid.Add(new Label
                {
                    Text = liftName,
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center
                }, 0, row + 1);

                for (int col = 0; col < pastWeekDates.Count; col++)
                {
                    var date = pastWeekDates[col];
                    var totalVolume = liftHistoryData
                        .Where(l => l.LiftName == liftName && l.DateOnly == date.ToString("yyyy-MM-dd"))
                        .SelectMany(l => l.SetDetails)
                        .Sum(s => s.Reps * s.Weight);

                    LiftingHistoryGrid.Add(new Label
                    {
                        Text = totalVolume > 0 ? totalVolume.ToString() : "-",
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }, col + 1, row + 1);
                }

                for(int col = 0; col < pastWeekDates.Count; col++)
                {
                    var date = pastWeekDates[col];
                    var macros = await App.Database.GetDailyMacrosDatedAsync(date, UserSession.LoggedInUserId);
                    var macrosText = macros != null ? $"P: {macros.Protein}g\nC: {macros.Carbs}g\nF: {macros.Fats}g\nCal: {macros.Calories}"
            : "P: 0g\nC: 0g\nF: 0g\nCal: 0";

                    LiftingHistoryGrid.Add(new Label
                    {
                        Text = macrosText,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }, col + 1, liftNames.Count + 1);
                    };
                }
            }
        

        private async Task LoadLiftingHistoryBySearchDate(DateTime date)
        {
            try
            {
                if (!UserSession.IsUserLoggedIn)
                {
                    await DisplayAlert("Error", "No user is logged in.", "OK");
                    return;
                }
                var userId = UserSession.LoggedInUserId;

                var liftHistoryData = await App.Database.GetLiftsByDateAsync(date.ToString("yyyy-MM-dd"), userId);
              
                if (liftHistoryData == null || !liftHistoryData.Any())
                {
                    await DisplayAlert("Warning", "No lifting history is available", "OK");
                    LiftNames = new List<string>();
                    LiftingData = new Dictionary<string, Dictionary<DateTime, string>>();
                    return;
                }

                LiftNames = liftHistoryData.Select(l => l.LiftName).Distinct().ToList();
                LiftingData = new Dictionary<string, Dictionary<DateTime, string>>();

                foreach (var liftName in LiftNames)
                {
                    var dateVolumes = new Dictionary<DateTime, string>();

                    foreach (var pastDate in PastSevenDays)
                    {
                        var lift = liftHistoryData.FirstOrDefault(l => l.LiftName == liftName && l.Date.Date == pastDate.Date);
                        var volume = lift?.SetDetails.Sum(set => set.Reps * set.Weight) ?? 0;
                        dateVolumes[pastDate] = volume > 0 ? volume.ToString() : "-";
                    }

                    LiftingData[liftName] = dateVolumes;
                }

                OnPropertyChanged(nameof(LiftNames));
                OnPropertyChanged(nameof(LiftingData));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load: {ex.Message}", "OK");
            }
        }

        private async Task DeleteLift(DailyLifts liftToDelete)
        {
            bool confirmDelete = await DisplayAlert("Warning", "Are you sure you wish to delete this information?", "Yes", "No");
            if (confirmDelete)
            {
                try
                {
                    await App.Database.DeleteLiftWithSetsAsync(liftToDelete.LiftId, UserSession.LoggedInUserId);
                    await DisplayAlert("Success", "Lifting information has been deleted.", "OK");
                    await LoadLiftingHistory(UserSession.LoggedInUserId);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to delete data: {ex.Message}", "OK");
                }
            }
        }
               
        private async void ReturnButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(Choice)}");
        }

        private async void EditLift_Clicked(object sender, EventArgs e)
        {
            try
                {
                await Shell.Current.GoToAsync(nameof(EditLiftPage));
                }
            catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to navigate to EditLiftPage: {ex.Message}", "OK");
                }
        }
        private async void Delete_Clicked(object sender, EventArgs e)
        {
            var selectedDate = DatePicker.Date;

            if (selectedDate == null)
            {
                await DisplayAlert("Error", "Please select a workout date.", "OK");
                return;
            }

            try
            {
                var liftsToDelete = await App.Database.GetLiftsByDateWithSetsAsync(selectedDate.ToString("yyyy-MM-dd"), UserSession.LoggedInUserId);

                if (liftsToDelete == null || !liftsToDelete.Any())
                {
                    await DisplayAlert("Info", "No workouts found for the selected date.", "OK");
                    return;
                }

                bool confirmDelete = await DisplayAlert("Confirm",
                    $"Are you sure you want to delete all workouts for {selectedDate:yyyy-MM-dd}?", "Yes", "No");

                if (confirmDelete)
                {
                    foreach (var lift in liftsToDelete)
                    {
                        await App.Database.DeleteLiftWithSetsAsync(lift.LiftId, UserSession.LoggedInUserId);
                    }

                    await DisplayAlert("Success", $"Workouts deleted for {selectedDate:yyyy-MM-dd}.", "OK");
                    await LoadLiftingHistory(UserSession.LoggedInUserId);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete workouts: {ex.Message}", "OK");
            }
        }
        private async void SearchButton_Clicked(object? sender, EventArgs e)
        {
            var selectedDate = DatePicker.Date;
            var lifthistoryData = await App.Database.GetLiftsByDateWithSetsAsync(selectedDate.ToString("yyyy-MM-dd"), UserSession.LoggedInUserId);
            var macrosData = await App.Database.GetDailyMacrosDatedAsync(selectedDate, UserSession.LoggedInUserId);

            string message = $"Lifts for {selectedDate: MM/dd/yyyy}: \n";

            if(lifthistoryData != null && lifthistoryData.Any())
            {
                foreach(var lift in lifthistoryData)
                {
                    var totalVolume = lift.SetDetails?.Sum(set => set.Reps * set.Weight) ?? 0;
                    message += $" - {lift.LiftName}: {totalVolume} lbs\n";
                }
            }
            else
            {
                message += "No lifts recorded. \n";
            }
            message += "\n Macros: \n";

            if(macrosData != null)
            {
                message += $"Protein: {macrosData.Protein}g\n";
                message += $"Fats: {macrosData.Fats}g\n";
                message += $"Carbs: {macrosData.Carbs}g\n";
                message += $"Calories: {macrosData.Calories}g\n";
            }


            await DisplayAlert("Lifting and Macros Data: ", message, "OK");
        }

        private async void EditMacros_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(EditMacros));
        }
    }    
}
