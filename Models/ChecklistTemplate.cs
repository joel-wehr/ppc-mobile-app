using SQLite;

namespace powered_parachute.Models
{
    [Table("checklist_templates")]
    public class ChecklistTemplate
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; } = true;

        public int? RemoteId { get; set; }
        public int SyncStatus { get; set; } // 0=Synced, 1=Modified, 2=New

        [NotNull]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }
    }
}
