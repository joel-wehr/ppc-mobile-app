using SQLite;

namespace powered_parachute.Models
{
    /// <summary>
    /// Represents a single flight log entry
    /// </summary>
    [Table("flights")]
    public class Flight
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public DateTime FlightDate { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Flight duration in minutes
        /// </summary>
        public int? DurationMinutes { get; set; }

        public string? Location { get; set; }

        public string? WeatherConditions { get; set; }

        public string? Notes { get; set; }

        [NotNull]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }

        /// <summary>
        /// Calculates duration from start and end times
        /// </summary>
        public void CalculateDuration()
        {
            if (StartTime.HasValue && EndTime.HasValue)
            {
                var duration = EndTime.Value - StartTime.Value;
                DurationMinutes = (int)duration.TotalMinutes;
            }
        }
    }
}
