using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Services;
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
            Title = "Flight Log";
        }

        public async Task LoadFlightsAsync()
        {
            var flightList = await _databaseService.GetFlightsWithLogsAsync();

            if (!flightList.Any())
            {
                HasFlights = false;
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
                    TimeDisplay = flight.StartTime.HasValue
                        ? flight.StartTime.Value.ToString("h:mm tt")
                        : "Not recorded",
                    ChecklistCount = logs.Count,
                    DurationDisplay = GetDurationDisplay(flight),
                    Location = flight.Location ?? string.Empty
                };

                flightSummaries.Add(summary);
            }

            Flights = new ObservableCollection<FlightSummary>(flightSummaries);
        }

        private string GetDurationDisplay(Flight flight)
        {
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
            await Shell.Current.GoToAsync($"FlightDetailPage?FlightId={flight.FlightId}");
        }

        [RelayCommand]
        async Task ClearAllLogs()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Clear All Logs",
                "Are you sure you want to delete all checklist logs? This cannot be undone.",
                "Delete",
                "Cancel");

            if (!confirm) return;

            var logs = await _databaseService.GetChecklistLogsAsync();
            foreach (var log in logs)
            {
                await _databaseService.DeleteChecklistLogAsync(log);
            }

            await LoadFlightsAsync();
        }

        public async void OnAppearing()
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

        public string ChecklistsDisplay => $"{ChecklistCount} checklist{(ChecklistCount != 1 ? "s" : "")}";
    }

    public partial class ChecklistLogGroup : ObservableObject
    {
        public DateTime Date { get; set; }
        public string DateDisplay => Date.ToString("ddd, MMM d, yyyy");
        public List<ChecklistLogItem> Items { get; set; } = new();
        public string Summary { get; set; } = string.Empty;

        public ChecklistLogGroup(DateTime date, List<ChecklistLog> logs)
        {
            Date = date;

            var orderedLogs = logs.OrderBy(l => l.CompletedAt).ToList();

            for (int i = 0; i < orderedLogs.Count; i++)
            {
                var log = orderedLogs[i];
                var item = new ChecklistLogItem
                {
                    Id = log.Id,
                    ChecklistType = log.ChecklistType,
                    ChecklistName = GetChecklistName(log.ChecklistType),
                    CompletedAt = log.CompletedAt,
                    TimeDisplay = log.CompletedAt.ToString("h:mm tt"),
                    TotalItems = log.TotalItems,
                    CheckedItems = log.CheckedItems,
                    CompletionPercentage = log.TotalItems > 0
                        ? (int)((double)log.CheckedItems / log.TotalItems * 100)
                        : 0
                };

                // Calculate time since previous checklist
                if (i > 0)
                {
                    var previousLog = orderedLogs[i - 1];
                    var duration = log.CompletedAt - previousLog.CompletedAt;
                    item.DurationSincePrevious = FormatDuration(duration);
                }

                Items.Add(item);
            }

            // Calculate total time for the day
            if (orderedLogs.Count > 1)
            {
                var totalDuration = orderedLogs.Last().CompletedAt - orderedLogs.First().CompletedAt;
                Summary = $"Total time: {FormatDuration(totalDuration)} • {orderedLogs.Count} checklists";
            }
            else
            {
                Summary = $"{orderedLogs.Count} checklist";
            }
        }

        private string GetChecklistName(ChecklistType type)
        {
            return type switch
            {
                ChecklistType.PreflightDetailed => "Preflight (Detailed)",
                ChecklistType.PreflightAbbreviated => "Preflight (Quick)",
                ChecklistType.WarmUp => "Warm Up",
                ChecklistType.PreStartAndTakeoff => "Pre-Start & Takeoff",
                ChecklistType.WingLayout => "Wing Layout",
                ChecklistType.InFlight => "In-Flight Practice",
                ChecklistType.PostFlight => "Post Flight",
                _ => type.ToString()
            };
        }

        private string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
            {
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            }
            else if (duration.TotalMinutes >= 1)
            {
                return $"{(int)duration.TotalMinutes}m";
            }
            else
            {
                return $"{duration.Seconds}s";
            }
        }
    }

    public class ChecklistLogItem
    {
        public int Id { get; set; }
        public ChecklistType ChecklistType { get; set; }
        public string ChecklistName { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
        public string TimeDisplay { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public int CheckedItems { get; set; }
        public int CompletionPercentage { get; set; }
        public string? DurationSincePrevious { get; set; }
    }
}
