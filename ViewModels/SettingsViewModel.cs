using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Services;

namespace powered_parachute.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private string appVersion = "1.0.0";

        public SettingsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Settings";
        }

        [RelayCommand]
        async Task ResetChecklists()
        {
            bool confirm = await Shell.Current.DisplayAlertAsync(
                "Reset Checklists",
                "Reset all checklists to defaults? Custom checklists will be deleted.",
                "Reset",
                "Cancel");
            if (!confirm) return;

            await _databaseService.ResetChecklistsToDefaultsAsync();
            await Shell.Current.DisplayAlertAsync("Done", "Checklists reset to defaults.", "OK");
        }

        [RelayCommand]
        async Task ClearAllData()
        {
            bool confirm = await Shell.Current.DisplayAlertAsync(
                "Clear All Data",
                "Delete ALL flight logs, checklist logs, and maintenance records? This cannot be undone. Equipment and pilot profile will be kept.",
                "Delete All",
                "Cancel");
            if (!confirm) return;

            await _databaseService.ClearAllDataAsync();
            await Shell.Current.DisplayAlertAsync("Done", "All flight data cleared.", "OK");
        }
    }
}
