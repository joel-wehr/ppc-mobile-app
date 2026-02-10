using powered_parachute.Models;
using powered_parachute.Models.Enums;

namespace powered_parachute.Services
{
    public class StatisticsService
    {
        private readonly DatabaseService _databaseService;

        public StatisticsService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<PilotStatistics> GetPilotStatisticsAsync()
        {
            var flights = await _databaseService.GetFlightsAsync();

            var stats = new PilotStatistics
            {
                TotalFlights = flights.Count,
                TotalHours = flights.Sum(f => f.HoursFlown ?? (f.DurationMinutes.HasValue ? f.DurationMinutes.Value / 60.0 : 0)),
                TotalLandings = flights.Sum(f => f.LandingCount ?? 0),
                TotalTakeoffs = flights.Sum(f => f.TakeoffCount ?? 0),
                FlightsThisMonth = flights.Count(f => f.FlightDate.Year == DateTime.Now.Year && f.FlightDate.Month == DateTime.Now.Month),
                FlightsThisYear = flights.Count(f => f.FlightDate.Year == DateTime.Now.Year)
            };

            // Currency: landings in last 90 days
            var cutoff90 = DateTime.Now.AddDays(-90);
            stats.LandingsLast90Days = flights
                .Where(f => f.FlightDate >= cutoff90)
                .Sum(f => f.LandingCount ?? 0);

            stats.CurrencyStatus = stats.LandingsLast90Days switch
            {
                >= 3 => CurrencyStatus.Current,
                >= 1 => CurrencyStatus.Warning,
                _ => CurrencyStatus.Expired
            };

            return stats;
        }

        public async Task<EquipmentStatistics?> GetEquipmentStatisticsAsync()
        {
            var frame = await _databaseService.GetActivePpcFrameAsync();
            if (frame == null) return null;

            var engine = await _databaseService.GetEngineByFrameAsync(frame.Id);
            var wing = await _databaseService.GetWingByFrameAsync(frame.Id);

            return new EquipmentStatistics
            {
                FrameName = frame.DisplayName,
                EngineHours = engine?.TotalHours,
                HoursUntilTBO = engine?.HoursUntilTBO,
                WingHours = wing?.TotalHours,
                LastWingInspection = wing?.LastInspectionDate
            };
        }
    }

    public class PilotStatistics
    {
        public int TotalFlights { get; set; }
        public double TotalHours { get; set; }
        public int TotalLandings { get; set; }
        public int TotalTakeoffs { get; set; }
        public int FlightsThisMonth { get; set; }
        public int FlightsThisYear { get; set; }
        public int LandingsLast90Days { get; set; }
        public CurrencyStatus CurrencyStatus { get; set; }
    }

    public class EquipmentStatistics
    {
        public string FrameName { get; set; } = string.Empty;
        public double? EngineHours { get; set; }
        public double? HoursUntilTBO { get; set; }
        public double? WingHours { get; set; }
        public DateTime? LastWingInspection { get; set; }
    }
}
