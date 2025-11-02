using SQLite;

namespace powered_parachute.Models
{
    /// <summary>
    /// Represents a completed checklist with timestamp
    /// </summary>
    [Table("checklist_logs")]
    public class ChecklistLog
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Reference to the flight this checklist belongs to (optional)
        /// </summary>
        public int? FlightId { get; set; }

        [NotNull]
        public ChecklistType ChecklistType { get; set; }

        /// <summary>
        /// When the checklist was completed
        /// </summary>
        [NotNull]
        public DateTime CompletedAt { get; set; }

        /// <summary>
        /// Total items in the checklist
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Items that were checked
        /// </summary>
        public int CheckedItems { get; set; }

        /// <summary>
        /// Optional notes for this checklist completion
        /// </summary>
        public string? Notes { get; set; }

        [NotNull]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
