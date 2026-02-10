using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class FlightEditorPage : ContentPage
    {
        public FlightEditorPage(FlightEditorViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
