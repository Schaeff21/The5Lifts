namespace D424Weightlifting;

public partial class RegisterNewUser : ContentPage
{
	public RegisterNewUser()
	{
		InitializeComponent();
	}

    private async void BTNRegisterUser_Clicked(object sender, EventArgs e)
    {
        try
        {
            var userName = UserNameEntry.Text;
            var password = PasswordEntry.Text;
            var email = EmailEntry.Text;
            var height = int.Parse(HeightEntry.Text);
            var weight = int.Parse(WeightEntry.Text);

            var result = await App.Database.NewUserRegisterAsync(userName, password,
                email, height, weight);
            await DisplayAlert("Registration", result, "OK");
            
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to register: {ex.Message}", "OK");
        }
    }

    private async void Return_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}