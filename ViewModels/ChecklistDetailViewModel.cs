using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using powered_parachute.Models;
using powered_parachute.Services;
using System.Collections.ObjectModel;

namespace powered_parachute.ViewModels
{
    [QueryProperty(nameof(ChecklistTypeString), "ChecklistType")]
    public partial class ChecklistDetailViewModel : BaseViewModel
    {
        private readonly ChecklistService _checklistService;
        private readonly DatabaseService _databaseService;

        private ChecklistType _checklistType;

        [ObservableProperty]
        private string checklistTypeString = string.Empty;

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

        public ChecklistDetailViewModel(ChecklistService checklistService, DatabaseService databaseService)
        {
            _checklistService = checklistService;
            _databaseService = databaseService;
        }

        partial void OnChecklistTypeStringChanged(string value)
        {
            if (Enum.TryParse<ChecklistType>(value, out var checklistType))
            {
                _checklistType = checklistType;
                LoadChecklist();
            }
        }

        private void LoadChecklist()
        {
            var items = _checklistService.GetChecklist(_checklistType);
            TotalItems = items.Count;

            // Group items by section
            var groups = items
                .GroupBy(i => i.Section)
                .Select(g => new ChecklistItemGroup(g.Key, g.ToList()))
                .ToList();

            GroupedItems = new ObservableCollection<ChecklistItemGroup>(groups);

            // Set title based on checklist type
            Title = _checklistType switch
            {
                ChecklistType.PreflightDetailed => "Preflight (Detailed)",
                ChecklistType.PreflightAbbreviated => "Preflight (Quick)",
                ChecklistType.WarmUp => "Warm Up",
                ChecklistType.PreStartAndTakeoff => "Pre-Start & Takeoff",
                ChecklistType.WingLayout => "Wing Layout",
                ChecklistType.InFlight => "In-Flight Practice",
                ChecklistType.PostFlight => "Post Flight",
                _ => "Checklist"
            };

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
                }
            }
            UpdateProgress();
        }

        [RelayCommand]
        async Task CompleteChecklist()
        {
            var log = new ChecklistLog
            {
                ChecklistType = _checklistType,
                CompletedAt = DateTime.Now,
                TotalItems = TotalItems,
                CheckedItems = CompletedItems
            };

            await _databaseService.SaveChecklistLogAsync(log);

            // Show confirmation and navigate back
            await Shell.Current.DisplayAlert(
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
