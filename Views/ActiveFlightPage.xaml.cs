using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class ActiveFlightPage : ContentPage
    {
        private readonly ActiveFlightViewModel _viewModel;

        public ActiveFlightPage(ActiveFlightViewModel viewModel)
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
