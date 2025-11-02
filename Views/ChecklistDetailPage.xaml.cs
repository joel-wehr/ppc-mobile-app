using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class ChecklistDetailPage : ContentPage
    {
        public ChecklistDetailPage(ChecklistDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
