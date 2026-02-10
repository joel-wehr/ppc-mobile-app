using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Models.Enums;
using powered_parachute.Services;
using powered_parachute.Views;
using System.Collections.ObjectModel;

namespace powered_parachute.ViewModels
{
    public partial class MaintenanceLogViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<MaintenanceLog> logs = new();

        [ObservableProperty]
        private bool hasLogs;

        [ObservableProperty]
        private ObservableCollection<PpcFrame> frames = new();

        [ObservableProperty]
        private PpcFrame? selectedFrame;

        public MaintenanceLogViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Maintenance";
        }

        public async Task LoadLogsAsync()
        {
            var frameList = await _databaseService.GetPpcFramesAsync();
            Frames = new ObservableCollection<PpcFrame>(frameList);

            var logList = SelectedFrame != null
                ? await _databaseService.GetMaintenanceLogsAsync(SelectedFrame.Id)
                : await _databaseService.GetMaintenanceLogsAsync();

            Logs = new ObservableCollection<MaintenanceLog>(logList);
            HasLogs = logList.Any();
        }

        [RelayCommand]
        async Task AddLog()
        {
            await Shell.Current.GoToAsync(nameof(MaintenanceDetailPage));
        }

        [RelayCommand]
        async Task OpenLog(MaintenanceLog log)
        {
            await Shell.Current.GoToAsync($"{nameof(MaintenanceDetailPage)}?LogId={log.Id}");
        }

        [RelayCommand]
        async Task FilterByFrame()
        {
            await LoadLogsAsync();
        }
    }
}
