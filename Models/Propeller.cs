using SQLite;

namespace powered_parachute.Models
{
    [Table("propellers")]
    public class Propeller
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int PpcFrameId { get; set; }

        public string? Manufacturer { get; set; }

        public string? Model { get; set; }

        public double? Diameter { get; set; }

        public double? Pitch { get; set; }

        public string? Material { get; set; }

        public string? Notes { get; set; }

        public int? RemoteId { get; set; }
        public int SyncStatus { get; set; } // 0=Synced, 1=Modified, 2=New

        [NotNull]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }
    }
}
