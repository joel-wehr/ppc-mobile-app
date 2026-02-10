using SQLite;
using powered_parachute.Models.Enums;

namespace powered_parachute.Models
{
    [Table("ppc_frames")]
    public class PpcFrame
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? Manufacturer { get; set; }

        public string? Model { get; set; }

        public string? SerialNumber { get; set; }

        public string? NNumber { get; set; }

        public int? Year { get; set; }

        public double? EmptyWeight { get; set; }

        public SeatConfig SeatConfig { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Notes { get; set; }

        public int? RemoteId { get; set; }
        public int SyncStatus { get; set; } // 0=Synced, 1=Modified, 2=New

        [NotNull]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }

        [Ignore]
        public string DisplayName => $"{Manufacturer} {Model}".Trim();
    }
}
