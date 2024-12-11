using System;
using System.Threading;
using D424;
using Microsoft.Maui.Controls;

namespace D424Weightlifting;

public partial class Macros : ContentPage
{

    private Timer _midnightTimer;
	public Macros()
	{
		InitializeComponent();
        MidnightResetTimer();
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDailyMacrosAsync();
        UpdateDailyTotal();
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(Choice)}");
    }

    private async void AddMacros_Clicked(object sender, EventArgs e)
    {
        if(int.TryParse(ProteinEntry.Text, out int proteinValue))
        {
            DataPersist.Instance.TotalProtein += proteinValue;
        }
        if (int.TryParse(CarbsEntry.Text, out int carbsValue))
        {
            DataPersist.Instance.TotalCarbs += carbsValue;
        }
        if(int.TryParse(FatsEntry.Text, out int fatsValue))
        {
            DataPersist.Instance.TotalFats += fatsValue;
        }
        

        UpdateDailyTotal();
        await SaveDailyMacrosAsync();
    }

    private void UpdateDailyTotal()
    {
        int totalCalories = DataPersist.Instance.TotalCalories;
        TotalProteinLabel.Text = $"Protein: {DataPersist.Instance.TotalProtein}g";
        TotalCarbsLabel.Text = $" Carbs: {DataPersist.Instance.TotalCarbs}g";
        TotalFatsLabel.Text = $"Fats: {DataPersist.Instance.TotalFats}g";
        TotalCaloriesLabel.Text = $"Calories: {totalCalories}";
    }

    private async Task LoadDailyMacrosAsync()
    {
        //await DataPersist.Instance.LoadDailyMacroTotalsAsync(App.Database);
        //UpdateDailyTotal();
        try
        {
            if(!UserSession.IsUserLoggedIn)
            {
                await DisplayAlert("Error", "No user is logged in", "OK");
                return;
            }

            var macros = await App.Database.GetDailyMacrosDatedAsync(DateTime.Today, UserSession.LoggedInUserId);

            if(macros != null)
            {
                DataPersist.Instance.TotalProtein = macros.Protein;
                DataPersist.Instance.TotalFats = macros.Fats;
                DataPersist.Instance.TotalCarbs = macros.Carbs;
            }
            else
            {
                DataPersist.Instance.ResetDailyTotals();
            }
            UpdateDailyTotal();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load Macros: {ex.Message}", "OK");
        }
    }

    private async void MidnightResetTimer()
    {
        
        var timeToMidnight = GetTimeToMidnight();
        _midnightTimer = new Timer(MidnightTimerCallBack, null, timeToMidnight, Timeout.InfiniteTimeSpan);
    }
    private TimeSpan GetTimeToMidnight()
    {
        var now = DateTime.Now;
        var midnight = DateTime.Today.AddDays(1);
        return midnight-now;
    }

    private void MidnightTimerCallBack(object state)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            DataPersist.Instance.ResetDailyTotals();
            UpdateDailyTotal();
        });

        _midnightTimer.Change(TimeSpan.FromDays(1), Timeout.InfiniteTimeSpan);
    }

    private async void SaveMacros_Clicked(object sender, EventArgs e)
    {
        try
        {
            await SaveDailyMacrosAsync();
            await DisplayAlert("Success", "Macros have been saved.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save: {ex.Message}", "OK");
        }
    }

    private async Task SaveDailyMacrosAsync()
    {
        try
        {
            if (!UserSession.IsUserLoggedIn)
            {
                await DisplayAlert("Error", "No user is logged in. Cannot save macros.", "OK");
                return;
            }
        
            var macros = new D424.Macros
            {
                UserId = UserSession.LoggedInUserId,
                Protein = DataPersist.Instance.TotalProtein,
                Carbs = DataPersist.Instance.TotalCarbs,
                Fats = DataPersist.Instance.TotalFats,
                Calories = DataPersist.Instance.TotalCalories,
                Date = DateTime.Today,
                DateOnly = DateTime.Today.ToString("yyyy-MM-dd")
            };

            await App.Database.SaveMacrosAsync(macros);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save, Error caused by: {ex.Message}", "OK");
        }

    }
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await SaveDailyMacrosAsync();
        _midnightTimer?.Dispose();
    }
}