using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class ChecklistsPage : ContentPage
    {
        private readonly ChecklistsViewModel _viewModel;

        public ChecklistsPage(ChecklistsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadChecklistsAsync();
        }
    }
}
