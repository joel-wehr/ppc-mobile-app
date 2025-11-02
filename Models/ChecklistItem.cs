using CommunityToolkit.Mvvm.ComponentModel;

namespace powered_parachute.Models
{
    /// <summary>
    /// Represents a single item in a checklist
    /// </summary>
    public partial class ChecklistItem : ObservableObject
    {
        public int Id { get; set; }

        public ChecklistType ChecklistType { get; set; }

        public string Section { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Order { get; set; }

        [ObservableProperty]
        private bool isChecked;

        /// <summary>
        /// For practice/maneuver items - tracks number of times performed
        /// </summary>
        [ObservableProperty]
        private int count;

        /// <summary>
        /// Indicates if this item uses a counter instead of checkbox
        /// </summary>
        public bool HasCounter { get; set; }

        /// <summary>
        /// Creates a deep copy of this checklist item
        /// </summary>
        public ChecklistItem Clone()
        {
            return new ChecklistItem
            {
                Id = this.Id,
                ChecklistType = this.ChecklistType,
                Section = this.Section,
                Description = this.Description,
                Order = this.Order,
                IsChecked = false, // Always start unchecked
                HasCounter = this.HasCounter, // Preserve counter flag
                Count = 0 // Always start at zero
            };
        }
    }
}
