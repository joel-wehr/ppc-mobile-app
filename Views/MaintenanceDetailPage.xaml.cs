using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class MaintenanceDetailPage : ContentPage
    {
        private readonly MaintenanceDetailViewModel _viewModel;

        public MaintenanceDetailPage(MaintenanceDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }
    }
}
