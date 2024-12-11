using D424;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
namespace D424Weightlifting
{
    public partial class App : Application
    {
        public static Database Database { get; private set; }
        public App()
        {
            InitializeComponent();

            string databasePath = Path.Combine(FileSystem.AppDataDirectory, "MyDatabase.db3");
            Database = new Database(databasePath);

            MainPage = new AppShell();

        }
    }
}
