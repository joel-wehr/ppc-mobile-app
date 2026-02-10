using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Models.Enums;
using powered_parachute.Services;
using System.Collections.ObjectModel;

namespace powered_parachute.ViewModels
{
    [QueryProperty(nameof(TemplateId), "TemplateId")]
    public partial class ChecklistEditorViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private int templateId;

        [ObservableProperty]
        private string templateName = string.Empty;

        [ObservableProperty]
        private string templateDescription = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ChecklistTemplateItem> items = new();

        [ObservableProperty]
        private bool isNewTemplate;

        public ChecklistEditorViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Edit Checklist";
        }

        partial void OnTemplateIdChanged(int value)
        {
            if (value > 0)
            {
                LoadTemplateAsync().ConfigureAwait(false);
            }
            else
            {
                IsNewTemplate = true;
                Title = "New Checklist";
            }
        }

        private async Task LoadTemplateAsync()
        {
            var template = await _databaseService.GetTemplateAsync(TemplateId);
            if (template == null) return;

            TemplateName = template.Name;
            TemplateDescription = template.Description ?? string.Empty;
            Title = $"Edit: {template.Name}";

            var templateItems = await _databaseService.GetTemplateItemsAsync(TemplateId);
            Items = new ObservableCollection<ChecklistTemplateItem>(templateItems);
        }

        [RelayCommand]
        async Task SaveTemplate()
        {
            if (string.IsNullOrWhiteSpace(TemplateName))
            {
                await Shell.Current.DisplayAlertAsync("Error", "Please enter a checklist name.", "OK");
                return;
            }

            ChecklistTemplate template;
            if (IsNewTemplate)
            {
                var allTemplates = await _databaseService.GetAllTemplatesAsync();
                template = new ChecklistTemplate
                {
                    Name = TemplateName,
                    Description = TemplateDescription,
                    DisplayOrder = allTemplates.Count,
                    IsDefault = false,
                    IsActive = true
                };
            }
            else
            {
                template = await _databaseService.GetTemplateAsync(TemplateId);
                template.Name = TemplateName;
                template.Description = TemplateDescription;
            }

            await _databaseService.SaveTemplateAsync(template);

            // Save items
            if (!IsNewTemplate)
            {
                await _databaseService.DeleteAllTemplateItemsAsync(template.Id);
            }

            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                item.TemplateId = template.Id;
                item.DisplayOrder = i;
                item.Id = 0; // Force insert
                await _databaseService.SaveTemplateItemAsync(item);
            }

            await Shell.Current.DisplayAlertAsync("Saved", "Checklist saved successfully.", "OK");
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        void AddItem()
        {
            Items.Add(new ChecklistTemplateItem
            {
                TemplateId = TemplateId,
                Section = Items.LastOrDefault()?.Section ?? "General",
                Description = string.Empty,
                DisplayOrder = Items.Count,
                ItemType = ChecklistItemType.Checkbox
            });
        }

        [RelayCommand]
        void RemoveItem(ChecklistTemplateItem item)
        {
            Items.Remove(item);
        }

        [RelayCommand]
        async Task ResetToDefaults()
        {
            bool confirm = await Shell.Current.DisplayAlertAsync(
                "Reset", "Reset all checklists to defaults? Custom checklists will be deleted.", "Reset", "Cancel");
            if (!confirm) return;

            await _databaseService.ResetChecklistsToDefaultsAsync();
            await Shell.Current.DisplayAlertAsync("Done", "Checklists reset to defaults.", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}
