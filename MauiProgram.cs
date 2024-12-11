using Microsoft.Extensions.Logging;
using D424;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace D424Weightlifting
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()               
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            string databasePath = Path.Combine(FileSystem.AppDataDirectory, "MyDatabase.db3");
            builder.Services.AddSingleton<Database>(s => new Database(databasePath));


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
