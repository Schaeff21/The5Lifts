using System;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using D424;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace D424Weightlifting
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private async void LogInBtn_Clicked(object sender, EventArgs e)
        {
            var userName = LogIn.Text;
            var password = LogInPassword.Text;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Login failed, please verify username and password", "OK");
                return;
            }        

            try
            {
                var user = await App.Database.GetUserAsync(userName, password);
                if (user != null)
                {
                    UserSession.LoggedInUserId = user.UserId;
                    UserSession.LoggedInUserName = user.UserName;
                    UserSession.IsUserLoggedIn = true;

                    await DisplayAlert("Success", $"Logged in: {user.UserName}", "OK");
                    await Shell.Current.GoToAsync(nameof(Choice));
                }
                else
                {
                    await DisplayAlert("Error", "Login failed, please verify username and password", "OK");
                    return;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert ("Error", $"Failed to log in: {ex.Message}", "OK");
            }

        }

        private async void NewUser_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new RegisterNewUser());
        }


        //FOR TESTING PURPOSES COMMENT OUT!!!!!
        private async void ViewUsers_Clicked(object sender, EventArgs e)
        {
            var users = await App.Database.GetAllUsersAsync();
            var userInfo = string.Join("\n", users.Select(u => $"ID: {u.UserId}, Username: {u.UserName}, Pass: {u.Password}"));

            await DisplayAlert("Users", userInfo, "OK");
        }
        private async void ClearDatabase_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirm", "Are you sure you want to clear all data?", "Yes", "No");
            if (confirm)
            {
                await App.Database.ClearDatabaseAsync();
                await DisplayAlert("Success", "Database cleared successfully.", "OK");
            }
        }

    }

}
