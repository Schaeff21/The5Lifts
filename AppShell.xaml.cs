using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
namespace D424Weightlifting

{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
            Routing.RegisterRoute(nameof(Workout), typeof(Workout));
            Routing.RegisterRoute(nameof(Choice), typeof(Choice));
            Routing.RegisterRoute(nameof(Macros), typeof(Macros));
            Routing.RegisterRoute(nameof(RegisterNewUser), typeof(RegisterNewUser));
            Routing.RegisterRoute(nameof(LiftingHistory), typeof(LiftingHistory));
            Routing.RegisterRoute(nameof(EditLiftPage), typeof(EditLiftPage));
            Routing.RegisterRoute(nameof(EditMacros), typeof(EditMacros));
        }
    }
}
