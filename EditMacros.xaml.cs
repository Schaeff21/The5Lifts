using System;
using D424;
using Microsoft.Maui.Controls;

namespace D424Weightlifting
{
public partial class EditMacros : ContentPage
{
    private D424.Macros _currentMacros;
	public EditMacros()
	{
		InitializeComponent();
	}
    private async void LoadMacros_Clicked(object sender, EventArgs e)
    {
        try
        {
                var selectedDate = MacrosDatePicker.Date;
            _currentMacros = await App.Database.GetDailyMacrosDatedAsync(selectedDate, UserSession.LoggedInUserId);

            if(_currentMacros != null)
            {
                ProteinEntry.Text = _currentMacros.Protein.ToString();
                FatsEntry.Text = _currentMacros.Fats.ToString();
                CarbsEntry.Text = _currentMacros.Carbs.ToString();
                CaloriesEntry.Text = _currentMacros.Calories.ToString();
            }
            else
            {
                await DisplayAlert("Error", "No macros for selected date found.", "OK");
                ClearEntries();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load: {ex.Message}", "OK");
        }
    }
    private async void SaveMacros_Clicked(object sender, EventArgs e)
    {
        try
        {
            if(_currentMacros == null)
            {
                _currentMacros = new D424.Macros
                {
                    DateOnly = MacrosDatePicker.Date.ToString("yyyy-MM-dd")
                };
            }
            _currentMacros.Protein = int.TryParse(ProteinEntry.Text, out var protein) ? protein : 0;
            _currentMacros.Fats = int.TryParse(FatsEntry.Text, out var fats) ? fats : 0;
            _currentMacros.Carbs = int.TryParse(CarbsEntry.Text, out var carbs) ? carbs : 0;
            _currentMacros.Calories = int.TryParse(CaloriesEntry.Text, out var calories) ? calories : 0;

            await App.Database.SaveMacrosAsync(_currentMacros);
            await DisplayAlert("Success", "Macros have been updated.", "OK");
            ClearEntries();
            await Shell.Current.GoToAsync(nameof(LiftingHistory));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save: {ex.Message}", "OK");
        }
    }
    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LiftingHistory));

    }

    private void ClearEntries()
    {
        ProteinEntry.Text = string.Empty;
        FatsEntry.Text = string.Empty;  
        CarbsEntry.Text = string.Empty;
        CaloriesEntry.Text = string.Empty;
    }
}
}