using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Models.Enums;
using powered_parachute.Services;
using System.Collections.ObjectModel;

namespace powered_parachute.ViewModels
{
    [QueryProperty(nameof(TemplateId), "TemplateId")]
    public partial class ChecklistDetailViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly FlightSessionService _flightSessionService;

        private int _templateId;
        private ChecklistTemplate? _template;

        public int TemplateId
        {
            get => _templateId;
            set
            {
                if (SetProperty(ref _templateId, value) && value > 0)
                {
                    LoadChecklistAsync().ConfigureAwait(false);
                }
            }
        }

        [ObservableProperty]
        private ObservableCollection<ChecklistItemGroup> groupedItems = new();

        [ObservableProperty]
        private int totalItems;

        [ObservableProperty]
        private int completedItems;

        [ObservableProperty]
        private string progressText = string.Empty;

        [ObservableProperty]
        private double progressValue;

        public ChecklistDetailViewModel(DatabaseService databaseService, FlightSessionService flightSessionService)
        {
            _databaseService = databaseService;
            _flightSessionService = flightSessionService;
        }

        private async Task LoadChecklistAsync()
        {
            _template = await _databaseService.GetTemplateAsync(TemplateId);
            if (_template == null) return;

            Title = _template.Name;

            var templateItems = await _databaseService.GetTemplateItemsAsync(TemplateId);
            TotalItems = templateItems.Count;

            // Convert template items to runtime ChecklistItems
            var items = templateItems.Select((ti, index) => new ChecklistItem
            {
                Id = index + 1,
                TemplateItemId = ti.Id,
                Section = ti.Section ?? "General",
                Description = ti.Description,
                Order = ti.DisplayOrder,
                IsChecked = false,
                HasCounter = ti.ItemType == ChecklistItemType.Counter,
                Count = 0
            }).ToList();

            var groups = items
                .GroupBy(i => i.Section)
                .Select(g => new ChecklistItemGroup(g.Key, g.ToList()))
                .ToList();

            GroupedItems = new ObservableCollection<ChecklistItemGroup>(groups);
            UpdateProgress();
        }

        [RelayCommand]
        void IncrementCounter(ChecklistItem item)
        {
            item.Count++;
            item.IsChecked = item.Count > 0;
            UpdateProgress();
        }

        [RelayCommand]
        void DecrementCounter(ChecklistItem item)
        {
            if (item.Count > 0)
            {
                item.Count--;
                item.IsChecked = item.Count > 0;
                UpdateProgress();
            }
        }

        [RelayCommand]
        void ToggleItem(ChecklistItem item)
        {
            item.IsChecked = !item.IsChecked;
            UpdateProgress();
        }

        [RelayCommand]
        void ResetChecklist()
        {
            foreach (var group in GroupedItems)
            {
                foreach (var item in group)
                {
                    item.IsChecked = false;
                    if (item.HasCounter) item.Count = 0;
                }
            }
            UpdateProgress();
        }

        [RelayCommand]
        async Task CompleteChecklist()
        {
            // Save the checklist log
            var log = new ChecklistLog
            {
                TemplateId = TemplateId,
                TemplateName = _template?.Name ?? "Unknown",
                ChecklistType = 0, // Legacy field
                CompletedAt = DateTime.Now,
                TotalItems = TotalItems,
                CheckedItems = CompletedItems
            };

            // Link to active flight session if one is running
            if (_flightSessionService.IsSessionActive && _flightSessionService.ActiveFlight != null)
            {
                log.FlightId = _flightSessionService.ActiveFlight.Id;
            }

            await _databaseService.SaveChecklistLogAsync(log);

            // Save individual item states
            var logItems = GroupedItems.SelectMany(g => g).Select(item => new ChecklistLogItem
            {
                ChecklistLogId = log.Id,
                TemplateItemId = item.TemplateItemId,
                Description = item.Description,
                Section = item.Section,
                ItemType = item.HasCounter ? ChecklistItemType.Counter : ChecklistItemType.Checkbox,
                IsChecked = item.IsChecked,
                CountValue = item.Count
            }).ToList();

            await _databaseService.SaveChecklistLogItemsAsync(logItems);

            // Notify flight session
            if (_flightSessionService.IsSessionActive)
            {
                _flightSessionService.AddCompletedChecklist(log.Id);
            }

            await Shell.Current.DisplayAlertAsync(
                "Checklist Completed",
                $"{Title} completed at {log.CompletedAt:h:mm tt}\n{CompletedItems} of {TotalItems} items checked",
                "OK");

            await Shell.Current.GoToAsync("..");
        }

        private void UpdateProgress()
        {
            CompletedItems = GroupedItems.SelectMany(g => g).Count(i => i.IsChecked);
            ProgressValue = TotalItems > 0 ? (double)CompletedItems / TotalItems : 0;
            ProgressText = $"{CompletedItems} of {TotalItems} completed";
        }
    }

    public class ChecklistItemGroup : List<ChecklistItem>
    {
        public string SectionName { get; set; }

        public ChecklistItemGroup(string sectionName, List<ChecklistItem> items) : base(items)
        {
            SectionName = sectionName;
        }
    }
}
