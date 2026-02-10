using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Views;

namespace powered_parachute.ViewModels
{
    public partial class MoreViewModel : BaseViewModel
    {
        public MoreViewModel()
        {
            Title = "More";
        }

        [RelayCommand]
        async Task NavigateToPilotProfile()
        {
            await Shell.Current.GoToAsync(nameof(PilotProfilePage));
        }

        [RelayCommand]
        async Task NavigateToEquipment()
        {
            await Shell.Current.GoToAsync(nameof(EquipmentListPage));
        }

        [RelayCommand]
        async Task NavigateToMaintenance()
        {
            await Shell.Current.GoToAsync(nameof(MaintenanceLogPage));
        }

        [RelayCommand]
        async Task NavigateToStatistics()
        {
            await Shell.Current.GoToAsync(nameof(StatisticsPage));
        }

        [RelayCommand]
        async Task NavigateToSettings()
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
    }
}
