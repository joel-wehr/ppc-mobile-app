using SQLite;

namespace powered_parachute.Models
{
    [Table("checklist_logs")]
    public class ChecklistLog
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int? FlightId { get; set; }

        // Keep old column for migration compatibility
        [NotNull]
        public int ChecklistType { get; set; }

        // New: links to checklist_templates
        public int? TemplateId { get; set; }

        [NotNull]
        public DateTime CompletedAt { get; set; }

        public int TotalItems { get; set; }

        public int CheckedItems { get; set; }

        public string? Notes { get; set; }

        /// <summary>
        /// Name snapshot so logs remain readable even if template is deleted
        /// </summary>
        public string? TemplateName { get; set; }

        public int? RemoteId { get; set; }
        public int SyncStatus { get; set; } // 0=Synced, 1=Modified, 2=New

        [NotNull]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }
    }
}
