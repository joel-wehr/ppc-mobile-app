using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class StatisticsPage : ContentPage
    {
        private readonly StatisticsViewModel _viewModel;

        public StatisticsPage(StatisticsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadStatsAsync();
        }
    }
}
