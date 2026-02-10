using SQLite;
using powered_parachute.Models.Enums;

namespace powered_parachute.Models
{
    [Table("checklist_log_items")]
    public class ChecklistLogItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int ChecklistLogId { get; set; }

        public int? TemplateItemId { get; set; }

        [NotNull]
        public string Description { get; set; } = string.Empty;

        public string? Section { get; set; }

        public ChecklistItemType ItemType { get; set; }

        public bool IsChecked { get; set; }

        public int CountValue { get; set; }

        public int? RemoteId { get; set; }
        public int SyncStatus { get; set; } // 0=Synced, 1=Modified, 2=New
    }
}
