using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Services;
using powered_parachute.Views;
using System.Collections.ObjectModel;

namespace powered_parachute.ViewModels
{
    public partial class EquipmentListViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<PpcFrame> frames = new();

        [ObservableProperty]
        private bool hasFrames;

        public EquipmentListViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "My Equipment";
        }

        public async Task LoadFramesAsync()
        {
            var frameList = await _databaseService.GetPpcFramesAsync();
            Frames = new ObservableCollection<PpcFrame>(frameList);
            HasFrames = frameList.Any();
        }

        [RelayCommand]
        async Task AddPpc()
        {
            await Shell.Current.GoToAsync(nameof(EquipmentDetailPage));
        }

        [RelayCommand]
        async Task OpenFrame(PpcFrame frame)
        {
            await Shell.Current.GoToAsync($"{nameof(EquipmentDetailPage)}?FrameId={frame.Id}");
        }
    }
}
