using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Models.Enums;
using powered_parachute.Services;
using powered_parachute.Views;
using System.Collections.ObjectModel;

namespace powered_parachute.ViewModels
{
    public partial class FlightLogViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<FlightSummary> flights = new();

        [ObservableProperty]
        private bool hasFlights;

        public FlightLogViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Logbook";
        }

        public async Task LoadFlightsAsync()
        {
            var flightList = await _databaseService.GetFlightsWithLogsAsync();

            // Also include flights that were manually created (not from checklists)
            var allFlights = await _databaseService.GetFlightsAsync();
            foreach (var f in allFlights)
            {
                if (!flightList.Any(fl => fl.Id == f.Id))
                {
                    flightList.Add(f);
                }
            }

            flightList = flightList.OrderByDescending(f => f.FlightDate).ToList();

            if (!flightList.Any())
            {
                HasFlights = false;
                Flights = new ObservableCollection<FlightSummary>();
                return;
            }

            HasFlights = true;

            var flightSummaries = new List<FlightSummary>();

            foreach (var flight in flightList)
            {
                var logs = await _databaseService.GetChecklistLogsByFlightAsync(flight.Id);

                var summary = new FlightSummary
                {
                    FlightId = flight.Id,
                    FlightDate = flight.FlightDate,
                    DateDisplay = flight.FlightDate.ToString("ddd, MMM d, yyyy"),
                    TimeDisplay = (flight.DepartureTime ?? flight.StartTime).HasValue
                        ? (flight.DepartureTime ?? flight.StartTime)!.Value.ToString("h:mm tt")
                        : "Not recorded",
                    ChecklistCount = logs.Count,
                    DurationDisplay = GetDurationDisplay(flight),
                    Location = flight.DepartureLocation ?? flight.Location ?? string.Empty,
                    FlightType = flight.FlightType,
                    TakeoffCount = flight.TakeoffCount ?? 0,
                    LandingCount = flight.LandingCount ?? 0
                };

                flightSummaries.Add(summary);
            }

            Flights = new ObservableCollection<FlightSummary>(flightSummaries);
        }

        private string GetDurationDisplay(Flight flight)
        {
            if (flight.HoursFlown.HasValue && flight.HoursFlown.Value > 0)
            {
                return flight.HoursFlown.Value.ToString("F1") + "h";
            }

            if (flight.DurationMinutes.HasValue && flight.DurationMinutes.Value > 0)
            {
                var hours = flight.DurationMinutes.Value / 60;
                var minutes = flight.DurationMinutes.Value % 60;
                return hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
            }

            return string.Empty;
        }

        [RelayCommand]
        async Task NavigateToFlight(FlightSummary flight)
        {
            await Shell.Current.GoToAsync($"{nameof(FlightDetailPage)}?FlightId={flight.FlightId}");
        }

        [RelayCommand]
        async Task AddManualFlight()
        {
            await Shell.Current.GoToAsync($"{nameof(FlightEditorPage)}?FlightId=0");
        }

        public async Task OnAppearingAsync()
        {
            await LoadFlightsAsync();
        }
    }

    public class FlightSummary
    {
        public int FlightId { get; set; }
        public DateTime FlightDate { get; set; }
        public string DateDisplay { get; set; } = string.Empty;
        public string TimeDisplay { get; set; } = string.Empty;
        public int ChecklistCount { get; set; }
        public string DurationDisplay { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public FlightType FlightType { get; set; }
        public int TakeoffCount { get; set; }
        public int LandingCount { get; set; }

        public string ChecklistsDisplay => $"{ChecklistCount} checklist{(ChecklistCount != 1 ? "s" : "")}";
        public string OperationsDisplay => TakeoffCount > 0 || LandingCount > 0
            ? $"{TakeoffCount}T / {LandingCount}L"
            : string.Empty;
    }
}
