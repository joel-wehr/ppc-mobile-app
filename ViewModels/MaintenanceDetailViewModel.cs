using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Models.Enums;
using powered_parachute.Services;
using System.Collections.ObjectModel;

namespace powered_parachute.ViewModels
{
    [QueryProperty(nameof(LogId), "LogId")]
    public partial class MaintenanceDetailViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private MaintenanceLog? _log;

        [ObservableProperty]
        private int logId;

        [ObservableProperty]
        private bool isNew = true;

        [ObservableProperty]
        private DateTime maintenanceDate = DateTime.Today;

        [ObservableProperty]
        private MaintenanceType maintenanceType;

        public List<MaintenanceType> MaintenanceTypes { get; } = Enum.GetValues<MaintenanceType>().ToList();

        [ObservableProperty]
        private MaintenanceComponent component;

        public List<MaintenanceComponent> Components { get; } = Enum.GetValues<MaintenanceComponent>().ToList();

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private string partsUsed = string.Empty;

        [ObservableProperty]
        private string cost = string.Empty;

        [ObservableProperty]
        private string engineHoursAtService = string.Empty;

        [ObservableProperty]
        private string nextServiceDueHours = string.Empty;

        [ObservableProperty]
        private DateTime? nextServiceDueDate;

        [ObservableProperty]
        private string performedBy = string.Empty;

        [ObservableProperty]
        private string notes = string.Empty;

        [ObservableProperty]
        private ObservableCollection<PpcFrame> frames = new();

        [ObservableProperty]
        private PpcFrame? selectedFrame;

        public MaintenanceDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Maintenance Record";
        }

        partial void OnLogIdChanged(int value)
        {
            if (value > 0)
            {
                IsNew = false;
                LoadLogAsync().ConfigureAwait(false);
            }
        }

        public async Task LoadAsync()
        {
            var frameList = await _databaseService.GetPpcFramesAsync();
            Frames = new ObservableCollection<PpcFrame>(frameList);
        }

        private async Task LoadLogAsync()
        {
            await LoadAsync();
            _log = await _databaseService.GetMaintenanceLogAsync(LogId);
            if (_log == null) return;

            Title = "Edit Maintenance";
            MaintenanceDate = _log.MaintenanceDate;
            MaintenanceType = _log.MaintenanceType;
            Component = _log.Component;
            Description = _log.Description ?? string.Empty;
            PartsUsed = _log.PartsUsed ?? string.Empty;
            Cost = _log.Cost?.ToString("F2") ?? string.Empty;
            EngineHoursAtService = _log.EngineHoursAtService?.ToString("F1") ?? string.Empty;
            NextServiceDueHours = _log.NextServiceDueHours?.ToString("F1") ?? string.Empty;
            NextServiceDueDate = _log.NextServiceDueDate;
            PerformedBy = _log.PerformedBy ?? string.Empty;
            Notes = _log.Notes ?? string.Empty;
            SelectedFrame = Frames.FirstOrDefault(f => f.Id == _log.PpcFrameId);
        }

        [RelayCommand]
        async Task Save()
        {
            if (SelectedFrame == null)
            {
                await Shell.Current.DisplayAlertAsync("Error", "Please select a PPC.", "OK");
                return;
            }

            _log ??= new MaintenanceLog();
            _log.PpcFrameId = SelectedFrame.Id;
            _log.MaintenanceDate = MaintenanceDate;
            _log.MaintenanceType = MaintenanceType;
            _log.Component = Component;
            _log.Description = Description;
            _log.PartsUsed = PartsUsed;
            if (double.TryParse(Cost, out var c)) _log.Cost = c;
            if (double.TryParse(EngineHoursAtService, out var eh)) _log.EngineHoursAtService = eh;
            if (double.TryParse(NextServiceDueHours, out var nsh)) _log.NextServiceDueHours = nsh;
            _log.NextServiceDueDate = NextServiceDueDate;
            _log.PerformedBy = PerformedBy;
            _log.Notes = Notes;

            await _databaseService.SaveMaintenanceLogAsync(_log);
            await Shell.Current.DisplayAlertAsync("Saved", "Maintenance record saved.", "OK");
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        async Task Delete()
        {
            if (_log == null) return;
            bool confirm = await Shell.Current.DisplayAlertAsync("Delete", "Delete this maintenance record?", "Delete", "Cancel");
            if (!confirm) return;
            await _databaseService.DeleteMaintenanceLogAsync(_log);
            await Shell.Current.GoToAsync("..");
        }
    }
}
