using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class FlightLogPage : ContentPage
    {
        private readonly FlightLogViewModel _viewModel;

        public FlightLogPage(FlightLogViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}
