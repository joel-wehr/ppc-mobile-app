using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Services;
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
        private ObservableCollection<ChecklistLogGroup> checklistGroups = new();

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
        private string weather = string.Empty;

        [ObservableProperty]
        private string notes = string.Empty;

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

            if (Flight == null)
                return;

            // Set display values
            DateDisplay = Flight.FlightDate.ToString("dddd, MMMM d, yyyy");

            if (Flight.DurationMinutes.HasValue && Flight.DurationMinutes.Value > 0)
            {
                var hours = Flight.DurationMinutes.Value / 60;
                var minutes = Flight.DurationMinutes.Value % 60;
                DurationDisplay = hours > 0
                    ? $"{hours}h {minutes}m"
                    : $"{minutes}m";
            }
            else
            {
                DurationDisplay = "Not recorded";
            }

            if (Flight.StartTime.HasValue && Flight.EndTime.HasValue)
            {
                TimeRangeDisplay = $"{Flight.StartTime.Value:h:mm tt} - {Flight.EndTime.Value:h:mm tt}";
            }
            else
            {
                TimeRangeDisplay = "Not recorded";
            }

            Location = Flight.Location ?? string.Empty;
            Weather = Flight.WeatherConditions ?? string.Empty;
            Notes = Flight.Notes ?? string.Empty;

            // Load checklist logs for this flight
            var logs = await _databaseService.GetChecklistLogsByFlightAsync(FlightId);
            TotalChecklists = logs.Count;

            // Group by checklist type
            var groups = logs
                .GroupBy(l => l.ChecklistType)
                .Select(g => new ChecklistLogGroup(g.First().CompletedAt.Date, g.ToList()))
                .ToList();

            ChecklistGroups = new ObservableCollection<ChecklistLogGroup>(groups);
        }

        [RelayCommand]
        async Task SaveFlight()
        {
            if (Flight == null)
                return;

            Flight.Location = Location;
            Flight.WeatherConditions = Weather;
            Flight.Notes = Notes;
            Flight.ModifiedAt = DateTime.UtcNow;

            await _databaseService.SaveFlightAsync(Flight);

            await Shell.Current.DisplayAlert(
                "Saved",
                "Flight details updated",
                "OK");
        }

        [RelayCommand]
        async Task DeleteFlight()
        {
            if (Flight == null)
                return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Delete Flight",
                "Are you sure you want to delete this flight and all associated checklist logs? This cannot be undone.",
                "Delete",
                "Cancel");

            if (!confirm)
                return;

            // Delete all associated checklist logs
            var logs = await _databaseService.GetChecklistLogsByFlightAsync(FlightId);
            foreach (var log in logs)
            {
                await _databaseService.DeleteChecklistLogAsync(log);
            }

            // Delete the flight
            await _databaseService.DeleteFlightAsync(Flight);

            await Shell.Current.DisplayAlert(
                "Deleted",
                "Flight deleted successfully",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
    }
}
