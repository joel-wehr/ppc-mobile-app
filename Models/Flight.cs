using SQLite;
using powered_parachute.Models.Enums;

namespace powered_parachute.Models
{
    [Table("flights")]
    public class Flight
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public DateTime FlightDate { get; set; }

        // Keep old columns for backward compat, map to new names
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public DateTime? DepartureTime { get; set; }
        public DateTime? LandingTime { get; set; }

        public int? DurationMinutes { get; set; }

        // Original fields kept
        public string? Location { get; set; }
        public string? WeatherConditions { get; set; }
        public string? Notes { get; set; }

        // New fields
        public int? PpcFrameId { get; set; }

        public FlightType FlightType { get; set; }

        public string? DepartureLocation { get; set; }
        public string? LandingLocation { get; set; }

        public int? TakeoffCount { get; set; }
        public int? LandingCount { get; set; }

        public double? FuelStartGallons { get; set; }
        public double? FuelEndGallons { get; set; }
        public double? FuelConsumed { get; set; }

        public double? HobbsStart { get; set; }
        public double? HobbsEnd { get; set; }
        public double? HoursFlown { get; set; }

        public int? CruiseAltitudeAGL { get; set; }
        public int? MaxAltitude { get; set; }

        public double? WindSpeed { get; set; }
        public string? WindDirection { get; set; }
        public double? WindGusts { get; set; }
        public double? Temperature { get; set; }
        public double? Visibility { get; set; }
        public int? Ceiling { get; set; }
        public double? AltimeterSetting { get; set; }
        public int? DensityAltitude { get; set; }

        public string? WeatherNotes { get; set; }

        public string? InstructorName { get; set; }
        public string? MaintenanceNotes { get; set; }
        public string? LessonsLearned { get; set; }

        public double? GpsLatitude { get; set; }
        public double? GpsLongitude { get; set; }

        public int? RemoteId { get; set; }
        public int SyncStatus { get; set; } // 0=Synced, 1=Modified, 2=New

        [NotNull]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }

        public void CalculateDuration()
        {
            // Prefer new fields, fall back to old
            var start = DepartureTime ?? StartTime;
            var end = LandingTime ?? EndTime;

            if (start.HasValue && end.HasValue)
            {
                var duration = end.Value - start.Value;
                DurationMinutes = (int)duration.TotalMinutes;
            }
        }

        public void CalculateFuelConsumed()
        {
            if (FuelStartGallons.HasValue && FuelEndGallons.HasValue)
            {
                FuelConsumed = FuelStartGallons.Value - FuelEndGallons.Value;
            }
        }

        public void CalculateHoursFlown()
        {
            if (HobbsStart.HasValue && HobbsEnd.HasValue)
            {
                HoursFlown = HobbsEnd.Value - HobbsStart.Value;
            }
        }
    }
}
