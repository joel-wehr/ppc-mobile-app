using CommunityToolkit.Mvvm.ComponentModel;
using powered_parachute.Models.Enums;

namespace powered_parachute.Models
{
    /// <summary>
    /// Runtime checklist item used in the UI (not stored in DB directly)
    /// </summary>
    public partial class ChecklistItem : ObservableObject
    {
        public int Id { get; set; }

        /// <summary>
        /// Links back to the template item this was created from
        /// </summary>
        public int? TemplateItemId { get; set; }

        public string Section { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Order { get; set; }

        [ObservableProperty]
        private bool isChecked;

        [ObservableProperty]
        private int count;

        public bool HasCounter { get; set; }

        public ChecklistItem Clone()
        {
            return new ChecklistItem
            {
                Id = this.Id,
                TemplateItemId = this.TemplateItemId,
                Section = this.Section,
                Description = this.Description,
                Order = this.Order,
                IsChecked = false,
                HasCounter = this.HasCounter,
                Count = 0
            };
        }
    }
}
