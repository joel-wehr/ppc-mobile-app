using SQLite;
using powered_parachute.Models.Enums;

namespace powered_parachute.Models
{
    [Table("pilot_profiles")]
    public class PilotProfile
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? FullName { get; set; }

        public CertificateType CertificateType { get; set; }

        public string? CertificateNumber { get; set; }

        public MedicalType MedicalType { get; set; }

        public DateTime? MedicalExpiration { get; set; }

        public double? MaxWindSpeed { get; set; }

        public double? MaxCrosswind { get; set; }

        public double? MinVisibility { get; set; }

        public double? MinCeiling { get; set; }

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactPhone { get; set; }

        /// <summary>
        /// JSON string of endorsements
        /// </summary>
        public string? Endorsements { get; set; }

        public int? RemoteId { get; set; }
        public int SyncStatus { get; set; } // 0=Synced, 1=Modified, 2=New

        [NotNull]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }
    }
}
