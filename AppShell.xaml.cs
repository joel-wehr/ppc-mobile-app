using powered_parachute.Views;

namespace powered_parachute
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            Routing.RegisterRoute(nameof(ChecklistsPage), typeof(ChecklistsPage));
            Routing.RegisterRoute(nameof(ChecklistDetailPage), typeof(ChecklistDetailPage));
            Routing.RegisterRoute(nameof(FlightLogPage), typeof(FlightLogPage));
            Routing.RegisterRoute(nameof(FlightDetailPage), typeof(FlightDetailPage));
        }
    }
}
