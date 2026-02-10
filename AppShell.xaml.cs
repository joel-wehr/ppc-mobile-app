using powered_parachute.Views;

namespace powered_parachute
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register push navigation routes
            Routing.RegisterRoute(nameof(ChecklistDetailPage), typeof(ChecklistDetailPage));
            Routing.RegisterRoute(nameof(ChecklistEditorPage), typeof(ChecklistEditorPage));
            Routing.RegisterRoute(nameof(FlightDetailPage), typeof(FlightDetailPage));
            Routing.RegisterRoute(nameof(FlightEditorPage), typeof(FlightEditorPage));
            Routing.RegisterRoute(nameof(PilotProfilePage), typeof(PilotProfilePage));
            Routing.RegisterRoute(nameof(EquipmentListPage), typeof(EquipmentListPage));
            Routing.RegisterRoute(nameof(EquipmentDetailPage), typeof(EquipmentDetailPage));
            Routing.RegisterRoute(nameof(MaintenanceLogPage), typeof(MaintenanceLogPage));
            Routing.RegisterRoute(nameof(MaintenanceDetailPage), typeof(MaintenanceDetailPage));
            Routing.RegisterRoute(nameof(StatisticsPage), typeof(StatisticsPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        }
    }
}
