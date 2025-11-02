using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Services;
using powered_parachute.Views;
using System.Collections.ObjectModel;

namespace powered_parachute.ViewModels
{
    public partial class ChecklistsViewModel : BaseViewModel
    {
        private readonly ChecklistService _checklistService;

        [ObservableProperty]
        private ObservableCollection<ChecklistInfo> checklists = new();

        public ChecklistsViewModel(ChecklistService checklistService)
        {
            _checklistService = checklistService;
            Title = "Checklists";
            LoadChecklists();
        }

        private void LoadChecklists()
        {
            var checklistTypes = _checklistService.GetChecklistTypes();

            foreach (var (type, displayName, description) in checklistTypes)
            {
                Checklists.Add(new ChecklistInfo
                {
                    Type = type,
                    DisplayName = displayName,
                    Description = description
                });
            }
        }

        [RelayCommand]
        async Task OpenChecklist(ChecklistInfo checklistInfo)
        {
            var param = new Dictionary<string, object>
            {
                { "ChecklistType", checklistInfo.Type.ToString() }
            };

            await Shell.Current.GoToAsync(nameof(ChecklistDetailPage), param);
        }
    }

    public class ChecklistInfo
    {
        public ChecklistType Type { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
