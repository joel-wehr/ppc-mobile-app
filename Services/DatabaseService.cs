using SQLite;
using powered_parachute.Models;

namespace powered_parachute.Services
{
    /// <summary>
    /// Service for managing SQLite database operations
    /// </summary>
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;
        private bool _initialized = false;

        public DatabaseService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "pegasus_flight.db3");
            _database = new SQLiteAsyncConnection(dbPath);
        }

        private async Task InitializeAsync()
        {
            if (_initialized) return;

            await _database.CreateTableAsync<Flight>();
            await _database.CreateTableAsync<ChecklistLog>();
            _initialized = true;
        }

        #region Flight Operations

        public async Task<List<Flight>> GetFlightsAsync()
        {
            await InitializeAsync();
            return await _database.Table<Flight>()
                .OrderByDescending(f => f.FlightDate)
                .ThenByDescending(f => f.StartTime)
                .ToListAsync();
        }

        public async Task<Flight> GetFlightAsync(int id)
        {
            await InitializeAsync();
            return await _database.Table<Flight>()
                .Where(f => f.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveFlightAsync(Flight flight)
        {
            await InitializeAsync();
            flight.ModifiedAt = DateTime.UtcNow;

            if (flight.Id != 0)
            {
                return await _database.UpdateAsync(flight);
            }
            else
            {
                flight.CreatedAt = DateTime.UtcNow;
                return await _database.InsertAsync(flight);
            }
        }

        public Task<int> DeleteFlightAsync(Flight flight)
        {
            return _database.DeleteAsync(flight);
        }

        /// <summary>
        /// Deletes all flights in a single transaction (much faster than one-by-one)
        /// </summary>
        public async Task<int> DeleteAllFlightsAsync()
        {
            await InitializeAsync();
            return await _database.ExecuteAsync("DELETE FROM flights");
        }

        #endregion

        #region ChecklistLog Operations

        public async Task<List<ChecklistLog>> GetChecklistLogsAsync()
        {
            await InitializeAsync();
            return await _database.Table<ChecklistLog>()
                .OrderByDescending(c => c.CompletedAt)
                .ToListAsync();
        }

        public Task<List<ChecklistLog>> GetChecklistLogsByFlightAsync(int flightId)
        {
            return _database.Table<ChecklistLog>()
                .Where(c => c.FlightId == flightId)
                .OrderBy(c => c.CompletedAt)
                .ToListAsync();
        }

        public Task<List<ChecklistLog>> GetRecentChecklistLogsAsync(int count = 20)
        {
            return _database.Table<ChecklistLog>()
                .OrderByDescending(c => c.CompletedAt)
                .Take(count)
                .ToListAsync();
        }

        public Task<ChecklistLog> GetChecklistLogAsync(int id)
        {
            return _database.Table<ChecklistLog>()
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveChecklistLogAsync(ChecklistLog log)
        {
            await InitializeAsync();
            if (log.Id != 0)
            {
                return await _database.UpdateAsync(log);
            }
            else
            {
                log.CreatedAt = DateTime.UtcNow;
                return await _database.InsertAsync(log);
            }
        }

        public Task<int> DeleteChecklistLogAsync(ChecklistLog log)
        {
            return _database.DeleteAsync(log);
        }

        /// <summary>
        /// Deletes all checklist logs in a single transaction (much faster than one-by-one)
        /// </summary>
        public async Task<int> DeleteAllChecklistLogsAsync()
        {
            await InitializeAsync();
            return await _database.ExecuteAsync("DELETE FROM checklist_logs");
        }

        /// <summary>
        /// Gets flights with their associated checklist logs grouped by date
        /// </summary>
        public async Task<List<Flight>> GetFlightsWithLogsAsync()
        {
            await InitializeAsync();
            var flights = new List<Flight>();

            // Get all checklist logs
            var logs = await _database.Table<ChecklistLog>()
                .OrderByDescending(c => c.CompletedAt)
                .ToListAsync();

            if (!logs.Any())
                return flights;

            // Get all existing flights
            var allFlights = await _database.Table<Flight>().ToListAsync();

            // Group logs by date
            var logsByDate = logs.GroupBy(l => l.CompletedAt.Date)
                .OrderByDescending(g => g.Key);

            foreach (var dateGroup in logsByDate)
            {
                var dateLogs = dateGroup.OrderBy(l => l.CompletedAt).ToList();
                var flightDate = dateGroup.Key;

                // Check if there's an existing flight for this date
                var existingFlight = allFlights.FirstOrDefault(f => f.FlightDate.Date == flightDate);

                Flight flight;
                if (existingFlight != null)
                {
                    flight = existingFlight;
                }
                else
                {
                    // Create a flight record from the logs
                    flight = new Flight
                    {
                        FlightDate = flightDate,
                        StartTime = dateLogs.First().CompletedAt,
                        EndTime = dateLogs.Last().CompletedAt,
                        CreatedAt = DateTime.UtcNow
                    };
                    flight.CalculateDuration();

                    // Save the flight
                    await SaveFlightAsync(flight);

                    // Update logs with flight ID in batch (much faster)
                    foreach (var log in dateLogs)
                    {
                        log.FlightId = flight.Id;
                    }
                    await _database.UpdateAllAsync(dateLogs);
                }

                flights.Add(flight);
            }

            return flights;
        }

        #endregion
    }
}
