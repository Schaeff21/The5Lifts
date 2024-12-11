using D424;
using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace D424Weightlifting
{
    [QueryProperty(nameof(SelectedWorkout), "SelectedWorkout")]


    public partial class Workout : ContentPage
    {
        public string SelectedWorkout { get; set; }
        public ObservableCollection<WorkoutSet> Sets { get; set; } = new ObservableCollection<WorkoutSet>();

        public Workout()
        {
		    InitializeComponent();            
            AddSet();
            SetsCollection.BindingContext = this;
            Sets.CollectionChanged += Sets_CollectionChanged;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!string.IsNullOrEmpty(SelectedWorkout))
            {
                Title = SelectedWorkout;
                WorkoutNameLabel.Text = $"{SelectedWorkout}";
            }
        }


        private async void BackButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(Choice)}");
        }
        private void AddASet_Clicked(object sender, EventArgs e)
        {
            AddSet();
        }

        private void AddSet()
        {
            Sets.Add(new WorkoutSet
            {
                SetNumber = Sets.Count + 1,
                Reps = null,
                Weight = null
                
            });
            CalculateTotalVolume();
        }
        private void Sets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CalculateTotalVolume();
        }

        public int TotalVolume;
        private void CalculateTotalVolume()
        {
            TotalVolume = Sets.Sum(set => (set.Reps ?? 0) * (set.Weight ?? 0));
            TotalVolumeLabel.Text = $"Total volume: {TotalVolume} lbs";
        }
        private async void SaveWorkout_Clicked(object sender, EventArgs e)
        {
            try
            {
                var newWorkout = new DailyLifts
                {
                    UserId = UserSession.LoggedInUserId,
                    LiftName = SelectedWorkout,
                    TotalSets = Sets.Count,
                    Date = DateTime.Now,
                    DateOnly = DateTime.Now.ToString("yyyy-MM-dd")
                };
                await App.Database.InsertAsync(newWorkout);

                foreach (var set in Sets)
                {
                    var liftSet = new LiftSet
                    {
                        LiftId = newWorkout.LiftId,
                        SetNumber = set.SetNumber,
                        Reps = set.Reps ?? 0,
                        Weight = set.Weight ?? 0
                    };
                    await App.Database.InsertSetAsync(liftSet);

                }
                await DisplayAlert("Success", "Saved successfully", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Workout was not saved: {ex.Message}", "OK");
            }
        }


        public class WorkoutSet
        {
            public int SetNumber { get; set; }
            public int? Reps { get; set; }
            public int? Weight { get; set; }
        }
    }
}