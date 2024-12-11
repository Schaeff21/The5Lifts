namespace D424Weightlifting;

using D424;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

public partial class Choice : ContentPage
{
	public Choice()
	{
		InitializeComponent();
	}

    private async void BTNWorkouts_Clicked(object sender, EventArgs e)
    {
        WorkoutPicker.Items.Clear();
        WorkoutPicker.Items.Add("Bench Press");
        WorkoutPicker.Items.Add("Deadlift"); 
        WorkoutPicker.Items.Add("Overhead Press");
        WorkoutPicker.Items.Add("Pull Ups");
        WorkoutPicker.Items.Add("Squat");

        await Task.Delay(200);

        WorkoutPicker.IsVisible = true;

    }

    private async void BTNMacros_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(Macros)}");

    }

    public async void WorkoutPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        try 
        { 
        if (WorkoutPicker.SelectedItem == null)
            {
                await DisplayAlert("Error", "Please select a Workout", "OK");
                return;
            }

            string _selectedWorkout = WorkoutPicker.SelectedItem.ToString();
            if (string.IsNullOrEmpty(_selectedWorkout))
            {
                await DisplayAlert("Error", "Please select a Workout", "OK");
                return;
            }

            Console.WriteLine($"Selected workout: {_selectedWorkout}");

            await Shell.Current.GoToAsync($"{nameof(Workout)}?SelectedWorkout={_selectedWorkout}");
        }
        catch (Exception ex) 
        {
            await DisplayAlert("Error", $"Unexpected error {ex.Message}", "OK");
        }
    }

    private async void BtnTracking_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(LiftingHistory)}");
    }

    private async void LogOut_Clicked(object sender, EventArgs e)
    {
        UserSession.ClearSession();
        await Shell.Current.GoToAsync(nameof(HomePage));
    }
}