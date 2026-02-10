using powered_parachute.Models;

namespace powered_parachute.Services
{
    /// <summary>
    /// Singleton managing the active flight session.
    /// Timer state survives page navigation.
    /// </summary>
    public class FlightSessionService
    {
        private readonly DatabaseService _databaseService;

        public bool IsSessionActive { get; private set; }
        public DateTime? SessionStartTime { get; private set; }
        public Flight? ActiveFlight { get; private set; }
        public List<int> CompletedChecklistLogIds { get; } = new();

        public event EventHandler? SessionStateChanged;

        public FlightSessionService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task StartSessionAsync(int? ppcFrameId = null)
        {
            SessionStartTime = DateTime.Now;
            IsSessionActive = true;

            ActiveFlight = new Flight
            {
                FlightDate = DateTime.Now.Date,
                DepartureTime = DateTime.Now,
                StartTime = DateTime.Now,
                PpcFrameId = ppcFrameId,
                CreatedAt = DateTime.UtcNow
            };

            await _databaseService.SaveFlightAsync(ActiveFlight);
            CompletedChecklistLogIds.Clear();
            SessionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task EndSessionAsync()
        {
            if (ActiveFlight != null)
            {
                ActiveFlight.LandingTime = DateTime.Now;
                ActiveFlight.EndTime = DateTime.Now;
                ActiveFlight.CalculateDuration();
                await _databaseService.SaveFlightAsync(ActiveFlight);
            }

            IsSessionActive = false;
            SessionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddCompletedChecklist(int checklistLogId)
        {
            CompletedChecklistLogIds.Add(checklistLogId);
            SessionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public TimeSpan GetElapsedTime()
        {
            if (SessionStartTime.HasValue && IsSessionActive)
            {
                return DateTime.Now - SessionStartTime.Value;
            }
            return TimeSpan.Zero;
        }

        public void Reset()
        {
            IsSessionActive = false;
            SessionStartTime = null;
            ActiveFlight = null;
            CompletedChecklistLogIds.Clear();
            SessionStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
