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
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<ChecklistInfo> checklists = new();

        [ObservableProperty]
        private bool isEditMode;

        public ChecklistsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Checklists";
        }

        public async Task LoadChecklistsAsync()
        {
            var templates = await _databaseService.GetActiveTemplatesAsync();

            Checklists = new ObservableCollection<ChecklistInfo>(
                templates.Select(t => new ChecklistInfo
                {
                    TemplateId = t.Id,
                    DisplayName = t.Name,
                    Description = t.Description ?? string.Empty,
                    IsDefault = t.IsDefault
                }));
        }

        [RelayCommand]
        async Task OpenChecklist(ChecklistInfo checklistInfo)
        {
            await Shell.Current.GoToAsync($"{nameof(ChecklistDetailPage)}?TemplateId={checklistInfo.TemplateId}");
        }

        [RelayCommand]
        async Task EditChecklist(ChecklistInfo checklistInfo)
        {
            await Shell.Current.GoToAsync($"{nameof(ChecklistEditorPage)}?TemplateId={checklistInfo.TemplateId}");
        }

        [RelayCommand]
        async Task AddChecklist()
        {
            await Shell.Current.GoToAsync($"{nameof(ChecklistEditorPage)}?TemplateId=0");
        }

        [RelayCommand]
        async Task DeleteChecklist(ChecklistInfo checklistInfo)
        {
            bool confirm = await Shell.Current.DisplayAlertAsync(
                "Delete", $"Delete \"{checklistInfo.DisplayName}\"?", "Delete", "Cancel");
            if (!confirm) return;

            var template = await _databaseService.GetTemplateAsync(checklistInfo.TemplateId);
            if (template != null)
            {
                await _databaseService.DeleteTemplateAsync(template);
                await LoadChecklistsAsync();
            }
        }

        [RelayCommand]
        void ToggleEditMode()
        {
            IsEditMode = !IsEditMode;
        }
    }

    public class ChecklistInfo
    {
        public int TemplateId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}
