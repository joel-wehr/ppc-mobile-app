using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class ChecklistsPage : ContentPage
    {
        public ChecklistsPage(ChecklistsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
