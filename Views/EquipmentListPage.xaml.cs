using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class EquipmentListPage : ContentPage
    {
        private readonly EquipmentListViewModel _viewModel;

        public EquipmentListPage(EquipmentListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadFramesAsync();
        }
    }
}
