using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Services;
using powered_parachute.Views;
using System.Collections.ObjectModel;

namespace powered_parachute.ViewModels
{
    public partial class ActiveFlightViewModel : BaseViewModel
    {
        private readonly FlightSessionService _flightSessionService;
        private readonly DatabaseService _databaseService;
        private IDispatcherTimer? _timer;

        [ObservableProperty]
        private bool isSessionActive;

        [ObservableProperty]
        private string elapsedTimeDisplay = "00:00:00";

        [ObservableProperty]
        private string departureLocation = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ChecklistStatus> checklistStatuses = new();

        [ObservableProperty]
        private int selectedPpcFrameId;

        [ObservableProperty]
        private ObservableCollection<PpcFrame> ppcFrames = new();

        public ActiveFlightViewModel(FlightSessionService flightSessionService, DatabaseService databaseService)
        {
            _flightSessionService = flightSessionService;
            _databaseService = databaseService;
            Title = "Fly";
        }

        public async Task LoadAsync()
        {
            IsSessionActive = _flightSessionService.IsSessionActive;

            // Load PPC frames for picker
            var frames = await _databaseService.GetPpcFramesAsync();
            PpcFrames = new ObservableCollection<PpcFrame>(frames);

            // Load default checklist sequence
            var templates = await _databaseService.GetActiveTemplatesAsync();
            var statuses = templates.Select(t => new ChecklistStatus
            {
                TemplateId = t.Id,
                Name = t.Name,
                Status = _flightSessionService.CompletedChecklistLogIds.Count > 0 ? "Not Started" : "Not Started"
            }).ToList();
            ChecklistStatuses = new ObservableCollection<ChecklistStatus>(statuses);

            if (IsSessionActive)
            {
                StartTimer();
            }
        }

        [RelayCommand]
        async Task StartSession()
        {
            await _flightSessionService.StartSessionAsync(SelectedPpcFrameId > 0 ? SelectedPpcFrameId : null);

            if (!string.IsNullOrEmpty(DepartureLocation) && _flightSessionService.ActiveFlight != null)
            {
                _flightSessionService.ActiveFlight.DepartureLocation = DepartureLocation;
                _flightSessionService.ActiveFlight.Location = DepartureLocation;
                await _databaseService.SaveFlightAsync(_flightSessionService.ActiveFlight);
            }

            IsSessionActive = true;
            StartTimer();
        }

        [RelayCommand]
        async Task EndSession()
        {
            bool confirm = await Shell.Current.DisplayAlertAsync(
                "End Flight",
                "Are you sure you want to end this flight session?",
                "End Flight",
                "Cancel");

            if (!confirm) return;

            StopTimer();
            await _flightSessionService.EndSessionAsync();
            IsSessionActive = false;

            // Navigate to flight editor to fill in details
            if (_flightSessionService.ActiveFlight != null)
            {
                await Shell.Current.GoToAsync($"{nameof(FlightEditorPage)}?FlightId={_flightSessionService.ActiveFlight.Id}");
                _flightSessionService.Reset();
            }
        }

        [RelayCommand]
        async Task OpenChecklist(ChecklistStatus status)
        {
            await Shell.Current.GoToAsync($"{nameof(ChecklistDetailPage)}?TemplateId={status.TemplateId}");
        }

        private void StartTimer()
        {
            _timer = Application.Current?.Dispatcher.CreateTimer();
            if (_timer != null)
            {
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += (s, e) =>
                {
                    var elapsed = _flightSessionService.GetElapsedTime();
                    ElapsedTimeDisplay = elapsed.ToString(@"hh\:mm\:ss");
                };
                _timer.Start();
            }
        }

        private void StopTimer()
        {
            _timer?.Stop();
            _timer = null;
        }
    }

    public class ChecklistStatus
    {
        public int TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Not Started";

        public Color StatusColor => Status switch
        {
            "Completed" => Color.FromArgb("#28A745"),
            "In Progress" => Color.FromArgb("#FFD700"),
            _ => Color.FromArgb("#ACACAC")
        };
    }
}
