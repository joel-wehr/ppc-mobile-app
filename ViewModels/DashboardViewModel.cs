using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Models.Enums;
using powered_parachute.Services;
using powered_parachute.Views;
using System.Collections.ObjectModel;

namespace powered_parachute.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly StatisticsService _statisticsService;
        private readonly FlightSessionService _flightSessionService;

        [ObservableProperty]
        private string pilotName = string.Empty;

        [ObservableProperty]
        private string activePpcName = string.Empty;

        [ObservableProperty]
        private string totalHoursDisplay = "0.0h";

        [ObservableProperty]
        private int flightsThisMonth;

        [ObservableProperty]
        private int landingsLast90Days;

        [ObservableProperty]
        private CurrencyStatus currencyStatus;

        [ObservableProperty]
        private string currencyStatusText = "No Landings";

        [ObservableProperty]
        private ObservableCollection<FlightSummary> recentFlights = new();

        [ObservableProperty]
        private bool hasProfile;

        [ObservableProperty]
        private bool hasPpc;

        [ObservableProperty]
        private bool hasFlights;

        [ObservableProperty]
        private string? engineAlert;

        [ObservableProperty]
        private string? wingAlert;

        public DashboardViewModel(
            DatabaseService databaseService,
            StatisticsService statisticsService,
            FlightSessionService flightSessionService)
        {
            _databaseService = databaseService;
            _statisticsService = statisticsService;
            _flightSessionService = flightSessionService;
            Title = "Dashboard";
        }

        public async Task LoadDashboardAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Load pilot profile
                var profile = await _databaseService.GetPilotProfileAsync();
                HasProfile = profile != null;
                PilotName = profile?.FullName ?? "Pilot";

                // Load active PPC
                var ppc = await _databaseService.GetActivePpcFrameAsync();
                HasPpc = ppc != null;
                ActivePpcName = ppc?.DisplayName ?? "No PPC Set Up";

                // Load stats
                var stats = await _statisticsService.GetPilotStatisticsAsync();
                TotalHoursDisplay = stats.TotalHours.ToString("F1") + "h";
                FlightsThisMonth = stats.FlightsThisMonth;
                LandingsLast90Days = stats.LandingsLast90Days;
                CurrencyStatus = stats.CurrencyStatus;
                CurrencyStatusText = stats.CurrencyStatus switch
                {
                    CurrencyStatus.Current => "Current",
                    CurrencyStatus.Warning => "Warning",
                    CurrencyStatus.Expired => "Not Current",
                    _ => "Unknown"
                };

                // Load recent flights
                var recent = await _databaseService.GetRecentFlightsAsync(3);
                HasFlights = recent.Any();
                var summaries = recent.Select(f => new FlightSummary
                {
                    FlightId = f.Id,
                    FlightDate = f.FlightDate,
                    DateDisplay = f.FlightDate.ToString("MMM d"),
                    Location = f.DepartureLocation ?? f.Location ?? string.Empty,
                    DurationDisplay = f.DurationMinutes.HasValue
                        ? (f.DurationMinutes.Value >= 60
                            ? $"{f.DurationMinutes.Value / 60}h {f.DurationMinutes.Value % 60}m"
                            : $"{f.DurationMinutes.Value}m")
                        : string.Empty
                });
                RecentFlights = new ObservableCollection<FlightSummary>(summaries);

                // Equipment alerts
                if (ppc != null)
                {
                    var equipStats = await _statisticsService.GetEquipmentStatisticsAsync();
                    if (equipStats?.HoursUntilTBO.HasValue == true && equipStats.HoursUntilTBO < 50)
                    {
                        EngineAlert = $"Engine TBO in {equipStats.HoursUntilTBO:F0}h";
                    }
                    else
                    {
                        EngineAlert = null;
                    }

                    if (equipStats?.LastWingInspection.HasValue == true)
                    {
                        var daysSince = (DateTime.Now - equipStats.LastWingInspection.Value).TotalDays;
                        if (daysSince > 365)
                        {
                            WingAlert = "Wing inspection overdue";
                        }
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task StartFlight()
        {
            await Shell.Current.GoToAsync("//Fly");
        }

        [RelayCommand]
        async Task NavigateToSetupProfile()
        {
            await Shell.Current.GoToAsync(nameof(PilotProfilePage));
        }

        [RelayCommand]
        async Task NavigateToAddPpc()
        {
            await Shell.Current.GoToAsync(nameof(EquipmentDetailPage));
        }

        [RelayCommand]
        async Task NavigateToFlight(FlightSummary flight)
        {
            await Shell.Current.GoToAsync($"{nameof(FlightDetailPage)}?FlightId={flight.FlightId}");
        }
    }
}
