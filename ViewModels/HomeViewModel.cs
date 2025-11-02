using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Views;

namespace powered_parachute.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            Title = "Pegasus Flight";
        }

        [RelayCommand]
        async Task NavigateToChecklists()
        {
            await Shell.Current.GoToAsync(nameof(ChecklistsPage));
        }

        [RelayCommand]
        async Task NavigateToFlightLog()
        {
            await Shell.Current.GoToAsync(nameof(FlightLogPage));
        }
    }
}
