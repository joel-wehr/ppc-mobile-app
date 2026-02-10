using SQLite;
using powered_parachute.Models.Enums;

namespace powered_parachute.Models
{
    [Table("checklist_template_items")]
    public class ChecklistTemplateItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int TemplateId { get; set; }

        public string? Section { get; set; }

        [NotNull]
        public string Description { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public ChecklistItemType ItemType { get; set; }

        public int? RemoteId { get; set; }
        public int SyncStatus { get; set; } // 0=Synced, 1=Modified, 2=New

        [NotNull]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }
    }
}
