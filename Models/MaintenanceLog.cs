using SQLite;
using powered_parachute.Models.Enums;

namespace powered_parachute.Models
{
    [Table("maintenance_logs")]
    public class MaintenanceLog
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int PpcFrameId { get; set; }

        [NotNull]
        public DateTime MaintenanceDate { get; set; }

        public MaintenanceType MaintenanceType { get; set; }

        public MaintenanceComponent Component { get; set; }

        public string? Description { get; set; }

        public string? PartsUsed { get; set; }

        public double? Cost { get; set; }

        public double? EngineHoursAtService { get; set; }

        public double? NextServiceDueHours { get; set; }

        public DateTime? NextServiceDueDate { get; set; }

        public string? PerformedBy { get; set; }

        public string? Notes { get; set; }

        public int? RemoteId { get; set; }
        public int SyncStatus { get; set; } // 0=Synced, 1=Modified, 2=New

        [NotNull]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }
    }
}
