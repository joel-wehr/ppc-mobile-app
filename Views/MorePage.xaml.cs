using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class MorePage : ContentPage
    {
        public MorePage(MoreViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
