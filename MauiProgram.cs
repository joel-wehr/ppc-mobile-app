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
                    fonts.AddMaterialIconFonts(); // Add Material Icons
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Register Services
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<ChecklistService>();

            // Register ViewModels
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<ChecklistsViewModel>();
            builder.Services.AddTransient<ChecklistDetailViewModel>();
            builder.Services.AddTransient<FlightLogViewModel>();
            builder.Services.AddTransient<FlightDetailViewModel>();

            // Register Views
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<ChecklistsPage>();
            builder.Services.AddTransient<ChecklistDetailPage>();
            builder.Services.AddTransient<FlightLogPage>();
            builder.Services.AddTransient<FlightDetailPage>();

            return builder.Build();
        }
    }
}
