using SQLite;
using powered_parachute.Models.Enums;

namespace powered_parachute.Models
{
    [Table("engines")]
    public class Engine
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int PpcFrameId { get; set; }

        public string? Manufacturer { get; set; }

        public string? Model { get; set; }

        public string? SerialNumber { get; set; }

        public EngineType EngineType { get; set; }

        public CoolingType CoolingType { get; set; }

        public double? TotalHours { get; set; }

        public double? TBOHours { get; set; }

        public DateTime? LastOverhaulDate { get; set; }

        public string? Notes { get; set; }

        public int? RemoteId { get; set; }
        public int SyncStatus { get; set; } // 0=Synced, 1=Modified, 2=New

        [NotNull]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }

        [Ignore]
        public double? HoursUntilTBO => (TBOHours.HasValue && TotalHours.HasValue)
            ? TBOHours.Value - TotalHours.Value
            : null;
    }
}
