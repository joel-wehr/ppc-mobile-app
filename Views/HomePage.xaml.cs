using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage(HomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
