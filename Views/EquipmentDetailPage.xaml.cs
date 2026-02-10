using powered_parachute.ViewModels;

namespace powered_parachute.Views
{
    public partial class EquipmentDetailPage : ContentPage
    {
        public EquipmentDetailPage(EquipmentDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
