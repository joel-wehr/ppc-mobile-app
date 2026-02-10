using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class MaintenanceLogPage : ContentPage
    {
        private readonly MaintenanceLogViewModel _viewModel;

        public MaintenanceLogPage(MaintenanceLogViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadLogsAsync();
        }
    }
}
