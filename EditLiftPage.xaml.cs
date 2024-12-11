using System;
using D424;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;


namespace D424Weightlifting
{
    public partial class EditLiftPage : ContentPage
	{
		public ObservableCollection<LiftSet> Sets { get; set; }
		public ObservableCollection<DailyLifts> Lifts { get; set; }

		public EditLiftPage()
		{
			InitializeComponent();
			Sets = new ObservableCollection<LiftSet>();
			Lifts = new ObservableCollection<DailyLifts>();
			BindingContext = this;
		}

        private async void LoadLiftButton_Clicked(object sender, EventArgs e)
        {
			try
			{
                var selectedDate = LiftDatePicker.Date.ToString("yyyy-MM-dd");
                var lifts = await App.Database.GetLiftsByDateWithSetsAsync(selectedDate, UserSession.LoggedInUserId);			
				Lifts.Clear();

				if(lifts != null && lifts.Any())
				{
					
					foreach(var lift in lifts)
					{
						if (!Lifts.Any(existingLift => existingLift.LiftId == lift.LiftId))
						{
							Lifts.Add(lift);
						}
					}
				}
				else
				{
					await DisplayAlert("Error", "No lifts found for selected date", "OK");
				}
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
			}
        }

        private void LiftPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
			if(LiftPicker.SelectedItem is DailyLifts selectedLift)
			{
				LoadLift(selectedLift);
			}
        }
        private void LiftsSetsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if(e.CurrentSelection.FirstOrDefault() is DailyLifts selectedLift)
			{
				LoadLift(selectedLift);
			}
        }

		private async void LoadLift (DailyLifts lift)
		{
			try
			{
				//var sets = await App.Database.GetSetsByLiftIdAsync(lift.LiftId);

				Sets.Clear();
				if(lift.SetDetails != null && lift.SetDetails.Any())
				{
					foreach(var set in lift.SetDetails)
					{
						Sets.Add(set);
					}
					
				}
				else
				{
					await DisplayAlert("Info", "No sets found for lift", "OK");
				}
				UpdateTotalVolume();
				CurrentLiftLabel.Text = $"Lift Name: {lift.LiftName}";
				
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Failed to load: {ex.Message}", "OK");
			}
		}

        private async void SaveEditLift_Clicked(object sender, EventArgs e)
		{
            if (LiftPicker.SelectedItem is not DailyLifts lift) return;

			try
			{
				lift.TotalSets = Sets.Count;
				await App.Database.UpdateLiftAsync(lift);

					foreach(var set in Sets)
					{
						set.LiftId = lift.LiftId;

						if (set.SetId > 0)
						{
							await App.Database.UpdateSetAsync(set);
						}
						else
						{
							await App.Database.InsertSetAsync(set);

                        }						
					}
				
					await DisplayAlert("Success", "Lift has been updated", "OK");
					await Shell.Current.GoToAsync(nameof(LiftingHistory));
			}
				catch (Exception ex)
				{
					await DisplayAlert("Error", $"Failed to update: {ex.Message}", "OK");
				}
		}

		private void AddASet_Clicked(object sender, EventArgs e)
		{
			var setNumber = Sets.Count + 1;
			Sets.Add(new LiftSet { SetNumber = setNumber });
			UpdateTotalVolume();
		}

		private async void BackButton_Clicked(object sender, EventArgs e)
		{
			await Shell.Current.GoToAsync(nameof(LiftingHistory));
		}
		private void UpdateTotalVolume()
		{
			var totalVolume = Sets.Sum(set => set.Reps * set.Weight);
			TotalVolumeLabel.Text = $"Total Volume: {totalVolume}";
		}
    }
}