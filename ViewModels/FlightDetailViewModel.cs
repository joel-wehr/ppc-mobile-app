using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Models.Enums;
using powered_parachute.Services;
using powered_parachute.Views;
using System.Collections.ObjectModel;

namespace powered_parachute.ViewModels
{
    [QueryProperty(nameof(FlightId), "FlightId")]
    public partial class FlightDetailViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private int flightId;

        [ObservableProperty]
        private Flight? flight;

        [ObservableProperty]
        private string dateDisplay = string.Empty;

        [ObservableProperty]
        private string durationDisplay = string.Empty;

        [ObservableProperty]
        private string timeRangeDisplay = string.Empty;

        [ObservableProperty]
        private int totalChecklists;

        [ObservableProperty]
        private string location = string.Empty;

        [ObservableProperty]
        private string flightTypeDisplay = string.Empty;

        [ObservableProperty]
        private string operationsDisplay = string.Empty;

        [ObservableProperty]
        private string fuelDisplay = string.Empty;

        [ObservableProperty]
        private string hobbsDisplay = string.Empty;

        [ObservableProperty]
        private string weatherDisplay = string.Empty;

        [ObservableProperty]
        private string notes = string.Empty;

        [ObservableProperty]
        private string lessonsLearned = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ChecklistLogSummary> checklistLogs = new();

        [ObservableProperty]
        private bool hasWeather;

        [ObservableProperty]
        private bool hasFuel;

        [ObservableProperty]
        private bool hasHobbs;

        [ObservableProperty]
        private bool hasNotes;

        [ObservableProperty]
        private bool hasLessons;

        public FlightDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Flight Details";
        }

        partial void OnFlightIdChanged(int value)
        {
            if (value > 0)
            {
                LoadFlightDetailsAsync().ConfigureAwait(false);
            }
        }

        private async Task LoadFlightDetailsAsync()
        {
            Flight = await _databaseService.GetFlightAsync(FlightId);
            if (Flight == null) return;

            DateDisplay = Flight.FlightDate.ToString("dddd, MMMM d, yyyy");

            // Duration
            if (Flight.HoursFlown.HasValue && Flight.HoursFlown.Value > 0)
            {
                DurationDisplay = Flight.HoursFlown.Value.ToString("F1") + "h";
            }
            else if (Flight.DurationMinutes.HasValue && Flight.DurationMinutes.Value > 0)
            {
                var hours = Flight.DurationMinutes.Value / 60;
                var minutes = Flight.DurationMinutes.Value % 60;
                DurationDisplay = hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
            }
            else
            {
                DurationDisplay = "Not recorded";
            }

            // Time range
            var start = Flight.DepartureTime ?? Flight.StartTime;
            var end = Flight.LandingTime ?? Flight.EndTime;
            if (start.HasValue && end.HasValue)
            {
                TimeRangeDisplay = $"{start.Value:h:mm tt} - {end.Value:h:mm tt}";
            }
            else
            {
                TimeRangeDisplay = "Not recorded";
            }

            Location = Flight.DepartureLocation ?? Flight.Location ?? string.Empty;

            // Flight type
            FlightTypeDisplay = Flight.FlightType switch
            {
                FlightType.Local => "Local",
                FlightType.CrossCountry => "Cross Country",
                FlightType.TrainingDual => "Training (Dual)",
                FlightType.TrainingSolo => "Training (Solo)",
                FlightType.Practice => "Practice",
                FlightType.CheckRide => "Check Ride",
                _ => "Local"
            };

            // Operations
            if (Flight.TakeoffCount.HasValue || Flight.LandingCount.HasValue)
            {
                OperationsDisplay = $"{Flight.TakeoffCount ?? 0} takeoffs, {Flight.LandingCount ?? 0} landings";
            }

            // Fuel
            HasFuel = Flight.FuelStartGallons.HasValue || Flight.FuelEndGallons.HasValue;
            if (HasFuel)
            {
                FuelDisplay = $"Start: {Flight.FuelStartGallons?.ToString("F1") ?? "--"} gal, End: {Flight.FuelEndGallons?.ToString("F1") ?? "--"} gal";
                if (Flight.FuelConsumed.HasValue)
                    FuelDisplay += $" (Used: {Flight.FuelConsumed.Value:F1} gal)";
            }

            // Hobbs
            HasHobbs = Flight.HobbsStart.HasValue || Flight.HobbsEnd.HasValue;
            if (HasHobbs)
            {
                HobbsDisplay = $"Start: {Flight.HobbsStart?.ToString("F1") ?? "--"}, End: {Flight.HobbsEnd?.ToString("F1") ?? "--"}";
            }

            // Weather
            var weatherParts = new List<string>();
            if (Flight.WindSpeed.HasValue)
                weatherParts.Add($"Wind: {Flight.WindSpeed}mph {Flight.WindDirection ?? ""}".Trim());
            if (Flight.Temperature.HasValue)
                weatherParts.Add($"Temp: {Flight.Temperature}F");
            if (Flight.Visibility.HasValue)
                weatherParts.Add($"Vis: {Flight.Visibility}SM");
            HasWeather = weatherParts.Any() || !string.IsNullOrEmpty(Flight.WeatherNotes) || !string.IsNullOrEmpty(Flight.WeatherConditions);
            WeatherDisplay = weatherParts.Any() ? string.Join(" | ", weatherParts) : (Flight.WeatherConditions ?? string.Empty);

            Notes = Flight.Notes ?? string.Empty;
            HasNotes = !string.IsNullOrEmpty(Notes);
            LessonsLearned = Flight.LessonsLearned ?? string.Empty;
            HasLessons = !string.IsNullOrEmpty(LessonsLearned);

            // Load checklist logs
            var logs = await _databaseService.GetChecklistLogsByFlightAsync(FlightId);
            TotalChecklists = logs.Count;
            ChecklistLogs = new ObservableCollection<ChecklistLogSummary>(
                logs.Select(l => new ChecklistLogSummary
                {
                    Name = l.TemplateName ?? $"Checklist #{l.ChecklistType}",
                    TimeDisplay = l.CompletedAt.ToString("h:mm tt"),
                    CompletionPercentage = l.TotalItems > 0 ? (int)((double)l.CheckedItems / l.TotalItems * 100) : 0
                }));
        }

        [RelayCommand]
        async Task EditFlight()
        {
            await Shell.Current.GoToAsync($"{nameof(FlightEditorPage)}?FlightId={FlightId}");
        }

        [RelayCommand]
        async Task DeleteFlight()
        {
            if (Flight == null) return;

            bool confirm = await Shell.Current.DisplayAlertAsync(
                "Delete Flight",
                "Are you sure you want to delete this flight and all associated checklist logs? This cannot be undone.",
                "Delete",
                "Cancel");

            if (!confirm) return;

            var logs = await _databaseService.GetChecklistLogsByFlightAsync(FlightId);
            foreach (var log in logs)
            {
                await _databaseService.DeleteChecklistLogAsync(log);
            }

            await _databaseService.DeleteFlightAsync(Flight);

            await Shell.Current.DisplayAlertAsync("Deleted", "Flight deleted successfully.", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    public class ChecklistLogSummary
    {
        public string Name { get; set; } = string.Empty;
        public string TimeDisplay { get; set; } = string.Empty;
        public int CompletionPercentage { get; set; }
    }
}
