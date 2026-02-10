using SQLite;
using powered_parachute.Models.Enums;

namespace powered_parachute.Models
{
    [Table("wings")]
    public class Wing
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int PpcFrameId { get; set; }

        public string? Manufacturer { get; set; }

        public string? Model { get; set; }

        public double? SizeSqFt { get; set; }

        public int? CellCount { get; set; }

        public WingType WingType { get; set; }

        public double? TotalHours { get; set; }

        public DateTime? ManufactureDate { get; set; }

        public DateTime? LastInspectionDate { get; set; }

        public string? Notes { get; set; }

        public int? RemoteId { get; set; }
        public int SyncStatus { get; set; } // 0=Synced, 1=Modified, 2=New

        [NotNull]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }
    }
}
