using CommunityToolkit.Mvvm.ComponentModel;
using powered_parachute.Models.Enums;
using powered_parachute.Services;

namespace powered_parachute.ViewModels
{
    public partial class StatisticsViewModel : BaseViewModel
    {
        private readonly StatisticsService _statisticsService;

        [ObservableProperty]
        private int totalFlights;

        [ObservableProperty]
        private string totalHours = "0.0";

        [ObservableProperty]
        private int totalLandings;

        [ObservableProperty]
        private int totalTakeoffs;

        [ObservableProperty]
        private int flightsThisMonth;

        [ObservableProperty]
        private int flightsThisYear;

        [ObservableProperty]
        private int landingsLast90Days;

        [ObservableProperty]
        private CurrencyStatus currencyStatus;

        [ObservableProperty]
        private string currencyText = string.Empty;

        [ObservableProperty]
        private string? engineHoursDisplay;

        [ObservableProperty]
        private string? hoursUntilTboDisplay;

        [ObservableProperty]
        private string? wingHoursDisplay;

        [ObservableProperty]
        private string? frameName;

        [ObservableProperty]
        private bool hasEquipment;

        public StatisticsViewModel(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
            Title = "Statistics";
        }

        public async Task LoadStatsAsync()
        {
            var stats = await _statisticsService.GetPilotStatisticsAsync();
            TotalFlights = stats.TotalFlights;
            TotalHours = stats.TotalHours.ToString("F1");
            TotalLandings = stats.TotalLandings;
            TotalTakeoffs = stats.TotalTakeoffs;
            FlightsThisMonth = stats.FlightsThisMonth;
            FlightsThisYear = stats.FlightsThisYear;
            LandingsLast90Days = stats.LandingsLast90Days;
            CurrencyStatus = stats.CurrencyStatus;
            CurrencyText = stats.CurrencyStatus switch
            {
                CurrencyStatus.Current => $"Current ({stats.LandingsLast90Days} landings in 90 days)",
                CurrencyStatus.Warning => $"Warning ({stats.LandingsLast90Days} landings in 90 days)",
                _ => "Not Current (0 landings in 90 days)"
            };

            var equipStats = await _statisticsService.GetEquipmentStatisticsAsync();
            HasEquipment = equipStats != null;
            if (equipStats != null)
            {
                FrameName = equipStats.FrameName;
                EngineHoursDisplay = equipStats.EngineHours?.ToString("F1") + "h";
                HoursUntilTboDisplay = equipStats.HoursUntilTBO?.ToString("F0") + "h";
                WingHoursDisplay = equipStats.WingHours?.ToString("F1") + "h";
            }
        }
    }
}
