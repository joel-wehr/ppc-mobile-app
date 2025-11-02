using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class FlightDetailPage : ContentPage
    {
        public FlightDetailPage(FlightDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
