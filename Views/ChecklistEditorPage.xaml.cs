using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class ChecklistEditorPage : ContentPage
    {
        public ChecklistEditorPage(ChecklistEditorViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
