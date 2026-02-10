using Microsoft.Extensions.Logging;
using UraniumUI;
using CommunityToolkit.Maui;
using powered_parachute.Services;
using powered_parachute.ViewModels;
using powered_parachute.Views;

namespace powered_parachute
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddMaterialIconFonts();
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Register Services
            builder.Services.AddSingleton<DefaultChecklistDataService>();
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<FlightSessionService>();
            builder.Services.AddTransient<StatisticsService>();
            builder.Services.AddSingleton<ApiSyncService>();

            // Register ViewModels
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<ActiveFlightViewModel>();
            builder.Services.AddTransient<ChecklistsViewModel>();
            builder.Services.AddTransient<ChecklistDetailViewModel>();
            builder.Services.AddTransient<ChecklistEditorViewModel>();
            builder.Services.AddTransient<FlightLogViewModel>();
            builder.Services.AddTransient<FlightDetailViewModel>();
            builder.Services.AddTransient<FlightEditorViewModel>();
            builder.Services.AddTransient<MoreViewModel>();
            builder.Services.AddTransient<PilotProfileViewModel>();
            builder.Services.AddTransient<EquipmentListViewModel>();
            builder.Services.AddTransient<EquipmentDetailViewModel>();
            builder.Services.AddTransient<MaintenanceLogViewModel>();
            builder.Services.AddTransient<MaintenanceDetailViewModel>();
            builder.Services.AddTransient<StatisticsViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();

            // Register Views
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<ActiveFlightPage>();
            builder.Services.AddTransient<ChecklistsPage>();
            builder.Services.AddTransient<ChecklistDetailPage>();
            builder.Services.AddTransient<ChecklistEditorPage>();
            builder.Services.AddTransient<FlightLogPage>();
            builder.Services.AddTransient<FlightDetailPage>();
            builder.Services.AddTransient<FlightEditorPage>();
            builder.Services.AddTransient<MorePage>();
            builder.Services.AddTransient<PilotProfilePage>();
            builder.Services.AddTransient<EquipmentListPage>();
            builder.Services.AddTransient<EquipmentDetailPage>();
            builder.Services.AddTransient<MaintenanceLogPage>();
            builder.Services.AddTransient<MaintenanceDetailPage>();
            builder.Services.AddTransient<StatisticsPage>();
            builder.Services.AddTransient<SettingsPage>();

            return builder.Build();
        }
    }
}
