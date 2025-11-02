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

        public DatabaseService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "pegasus_flight.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Flight>().Wait();
            _database.CreateTableAsync<ChecklistLog>().Wait();
        }

        #region Flight Operations

        public Task<List<Flight>> GetFlightsAsync()
        {
            return _database.Table<Flight>()
                .OrderByDescending(f => f.FlightDate)
                .ThenByDescending(f => f.StartTime)
                .ToListAsync();
        }

        public Task<Flight> GetFlightAsync(int id)
        {
            return _database.Table<Flight>()
                .Where(f => f.Id == id)
                .FirstOrDefaultAsync();
        }

        public Task<int> SaveFlightAsync(Flight flight)
        {
            flight.ModifiedAt = DateTime.UtcNow;

            if (flight.Id != 0)
            {
                return _database.UpdateAsync(flight);
            }
            else
            {
                flight.CreatedAt = DateTime.UtcNow;
                return _database.InsertAsync(flight);
            }
        }

        public Task<int> DeleteFlightAsync(Flight flight)
        {
            return _database.DeleteAsync(flight);
        }

        #endregion

        #region ChecklistLog Operations

        public Task<List<ChecklistLog>> GetChecklistLogsAsync()
        {
            return _database.Table<ChecklistLog>()
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

        public Task<int> SaveChecklistLogAsync(ChecklistLog log)
        {
            if (log.Id != 0)
            {
                return _database.UpdateAsync(log);
            }
            else
            {
                log.CreatedAt = DateTime.UtcNow;
                return _database.InsertAsync(log);
            }
        }

        public Task<int> DeleteChecklistLogAsync(ChecklistLog log)
        {
            return _database.DeleteAsync(log);
        }

        /// <summary>
        /// Gets flights with their associated checklist logs grouped by date
        /// </summary>
        public async Task<List<Flight>> GetFlightsWithLogsAsync()
        {
            var flights = new List<Flight>();

            // Get all checklist logs
            var logs = await _database.Table<ChecklistLog>()
                .OrderByDescending(c => c.CompletedAt)
                .ToListAsync();

            if (!logs.Any())
                return flights;

            // Group logs by date
            var logsByDate = logs.GroupBy(l => l.CompletedAt.Date)
                .OrderByDescending(g => g.Key);

            foreach (var dateGroup in logsByDate)
            {
                var dateLogs = dateGroup.OrderBy(l => l.CompletedAt).ToList();

                // Check if there's an existing flight for this date
                var existingFlight = await _database.Table<Flight>()
                    .Where(f => f.FlightDate.Date == dateGroup.Key)
                    .FirstOrDefaultAsync();

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
                        FlightDate = dateGroup.Key,
                        StartTime = dateLogs.First().CompletedAt,
                        EndTime = dateLogs.Last().CompletedAt,
                        CreatedAt = DateTime.UtcNow
                    };
                    flight.CalculateDuration();

                    // Save the flight
                    await SaveFlightAsync(flight);

                    // Update logs with flight ID
                    foreach (var log in dateLogs)
                    {
                        log.FlightId = flight.Id;
                        await _database.UpdateAsync(log);
                    }
                }

                flights.Add(flight);
            }

            return flights;
        }

        #endregion
    }
}
