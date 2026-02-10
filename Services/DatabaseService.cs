using SQLite;
using powered_parachute.Models;
using powered_parachute.Models.Enums;

namespace powered_parachute.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;
        private readonly DefaultChecklistDataService _defaultChecklistData;
        private bool _initialized = false;
        private const int SchemaVersion = 3;

        public DatabaseService(DefaultChecklistDataService defaultChecklistData)
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "pegasus_flight.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            _defaultChecklistData = defaultChecklistData;
        }

        private async Task InitializeAsync()
        {
            if (_initialized) return;

            // Original tables
            await _database.CreateTableAsync<Flight>();
            await _database.CreateTableAsync<ChecklistLog>();

            // New tables
            await _database.CreateTableAsync<PilotProfile>();
            await _database.CreateTableAsync<PpcFrame>();
            await _database.CreateTableAsync<Engine>();
            await _database.CreateTableAsync<Wing>();
            await _database.CreateTableAsync<Propeller>();
            await _database.CreateTableAsync<ChecklistTemplate>();
            await _database.CreateTableAsync<ChecklistTemplateItem>();
            await _database.CreateTableAsync<ChecklistLogItem>();
            await _database.CreateTableAsync<MaintenanceLog>();
            await _database.CreateTableAsync<AppSetting>();

            _initialized = true;

            await SeedDefaultChecklistsIfNeeded();
            await MigrateExistingDataIfNeeded();
        }

        private async Task SeedDefaultChecklistsIfNeeded()
        {
            var existingTemplates = await _database.Table<ChecklistTemplate>().CountAsync();
            if (existingTemplates > 0) return;

            var templates = _defaultChecklistData.GetDefaultTemplates();
            foreach (var templateDef in templates)
            {
                var template = new ChecklistTemplate
                {
                    Name = templateDef.Name,
                    Description = templateDef.Description,
                    DisplayOrder = templateDef.DisplayOrder,
                    IsDefault = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _database.InsertAsync(template);

                foreach (var itemDef in templateDef.Items)
                {
                    var item = new ChecklistTemplateItem
                    {
                        TemplateId = template.Id,
                        Section = itemDef.Section,
                        Description = itemDef.Description,
                        DisplayOrder = itemDef.DisplayOrder,
                        ItemType = itemDef.ItemType,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _database.InsertAsync(item);
                }
            }
        }

        private async Task MigrateExistingDataIfNeeded()
        {
            var versionSetting = await _database.Table<AppSetting>()
                .Where(s => s.Key == "SchemaVersion")
                .FirstOrDefaultAsync();

            if (versionSetting != null && int.TryParse(versionSetting.Value, out var ver) && ver >= SchemaVersion)
                return;

            // Migrate old ChecklistType enum values to TemplateId
            var logsWithoutTemplate = await _database.Table<ChecklistLog>()
                .Where(l => l.TemplateId == null)
                .ToListAsync();

            if (logsWithoutTemplate.Any())
            {
                var allTemplates = await _database.Table<ChecklistTemplate>().ToListAsync();

                // Map old enum values to template names
                var enumToName = new Dictionary<int, string>
                {
                    { 0, "Pre-Flight (Detailed)" },
                    { 1, "Pre-Flight (Quick)" },
                    { 2, "Warm-Up / Run-Up" },
                    { 3, "Before Takeoff" },
                    { 4, "Wing Layout" },
                    { 5, "In-Flight Practice" },
                    { 6, "Post Flight" }
                };

                foreach (var log in logsWithoutTemplate)
                {
                    if (enumToName.TryGetValue(log.ChecklistType, out var name))
                    {
                        var matchingTemplate = allTemplates.FirstOrDefault(t => t.Name == name);
                        if (matchingTemplate != null)
                        {
                            log.TemplateId = matchingTemplate.Id;
                            log.TemplateName = matchingTemplate.Name;
                            await _database.UpdateAsync(log);
                        }
                    }
                }
            }

            // V3: Add sync fields (RemoteId, SyncStatus) to all syncable tables
            var currentVersion = versionSetting != null && int.TryParse(versionSetting.Value, out var v) ? v : 0;
            if (currentVersion < 3)
            {
                var syncTables = new[]
                {
                    "flights", "checklist_logs", "checklist_templates",
                    "checklist_template_items", "checklist_log_items",
                    "pilot_profiles", "ppc_frames", "engines",
                    "wings", "propellers", "maintenance_logs"
                };

                foreach (var table in syncTables)
                {
                    await TryAddColumnAsync(table, "RemoteId", "INTEGER");
                    await TryAddColumnAsync(table, "SyncStatus", "INTEGER DEFAULT 0");
                }

                // checklist_logs was missing ModifiedAt
                await TryAddColumnAsync("checklist_logs", "ModifiedAt", "TEXT");
            }

            // Save schema version
            if (versionSetting == null)
            {
                await _database.InsertAsync(new AppSetting { Key = "SchemaVersion", Value = SchemaVersion.ToString() });
            }
            else
            {
                versionSetting.Value = SchemaVersion.ToString();
                await _database.UpdateAsync(versionSetting);
            }
        }

        private async Task TryAddColumnAsync(string table, string column, string type)
        {
            try
            {
                await _database.ExecuteAsync($"ALTER TABLE {table} ADD COLUMN {column} {type}");
            }
            catch (SQLiteException)
            {
                // Column already exists - safe to ignore
            }
        }

        #region Flight Operations

        public async Task<List<Flight>> GetFlightsAsync()
        {
            await InitializeAsync();
            return await _database.Table<Flight>()
                .OrderByDescending(f => f.FlightDate)
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
            if (flight.SyncStatus == 0) flight.SyncStatus = 1; // Queue for sync

            if (flight.Id != 0)
            {
                return await _database.UpdateAsync(flight);
            }
            else
            {
                if (flight.SyncStatus == 1) flight.SyncStatus = 2; // New record
                flight.CreatedAt = DateTime.UtcNow;
                return await _database.InsertAsync(flight);
            }
        }

        public Task<int> DeleteFlightAsync(Flight flight)
        {
            return _database.DeleteAsync(flight);
        }

        public async Task<int> DeleteAllFlightsAsync()
        {
            await InitializeAsync();
            return await _database.ExecuteAsync("DELETE FROM flights");
        }

        public async Task<List<Flight>> GetFlightsWithLogsAsync()
        {
            await InitializeAsync();
            var flights = new List<Flight>();

            var logs = await _database.Table<ChecklistLog>()
                .OrderByDescending(c => c.CompletedAt)
                .ToListAsync();

            if (!logs.Any())
                return flights;

            var allFlights = await _database.Table<Flight>().ToListAsync();

            var logsByDate = logs.GroupBy(l => l.CompletedAt.Date)
                .OrderByDescending(g => g.Key);

            foreach (var dateGroup in logsByDate)
            {
                var dateLogs = dateGroup.OrderBy(l => l.CompletedAt).ToList();
                var flightDate = dateGroup.Key;

                var existingFlight = allFlights.FirstOrDefault(f => f.FlightDate.Date == flightDate);

                Flight flight;
                if (existingFlight != null)
                {
                    flight = existingFlight;
                }
                else
                {
                    flight = new Flight
                    {
                        FlightDate = flightDate,
                        StartTime = dateLogs.First().CompletedAt,
                        EndTime = dateLogs.Last().CompletedAt,
                        CreatedAt = DateTime.UtcNow
                    };
                    flight.CalculateDuration();
                    await SaveFlightAsync(flight);

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

        public async Task<List<Flight>> GetRecentFlightsAsync(int count = 3)
        {
            await InitializeAsync();
            return await _database.Table<Flight>()
                .OrderByDescending(f => f.FlightDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetFlightCountAsync()
        {
            await InitializeAsync();
            return await _database.Table<Flight>().CountAsync();
        }

        public async Task<int> GetFlightCountThisMonthAsync()
        {
            await InitializeAsync();
            var allFlights = await _database.Table<Flight>().ToListAsync();
            var now = DateTime.Now;
            return allFlights.Count(f => f.FlightDate.Year == now.Year && f.FlightDate.Month == now.Month);
        }

        public async Task<double> GetTotalHoursAsync()
        {
            await InitializeAsync();
            var allFlights = await _database.Table<Flight>().ToListAsync();
            return allFlights.Sum(f => f.HoursFlown ?? (f.DurationMinutes.HasValue ? f.DurationMinutes.Value / 60.0 : 0));
        }

        public async Task<int> GetLandingsLast90DaysAsync()
        {
            await InitializeAsync();
            var allFlights = await _database.Table<Flight>().ToListAsync();
            var cutoff = DateTime.Now.AddDays(-90);
            return allFlights.Where(f => f.FlightDate >= cutoff).Sum(f => f.LandingCount ?? 0);
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

        public Task<ChecklistLog> GetChecklistLogAsync(int id)
        {
            return _database.Table<ChecklistLog>()
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveChecklistLogAsync(ChecklistLog log)
        {
            await InitializeAsync();
            log.ModifiedAt = DateTime.UtcNow;
            if (log.SyncStatus == 0) log.SyncStatus = 1; // Queue for sync

            if (log.Id != 0)
            {
                return await _database.UpdateAsync(log);
            }
            else
            {
                if (log.SyncStatus == 1) log.SyncStatus = 2; // New record
                log.CreatedAt = DateTime.UtcNow;
                return await _database.InsertAsync(log);
            }
        }

        public Task<int> DeleteChecklistLogAsync(ChecklistLog log)
        {
            return _database.DeleteAsync(log);
        }

        public async Task<int> DeleteAllChecklistLogsAsync()
        {
            await InitializeAsync();
            return await _database.ExecuteAsync("DELETE FROM checklist_logs");
        }

        #endregion

        #region ChecklistLogItem Operations

        public async Task SaveChecklistLogItemsAsync(List<ChecklistLogItem> items)
        {
            await InitializeAsync();
            await _database.InsertAllAsync(items);
        }

        public async Task<List<ChecklistLogItem>> GetChecklistLogItemsAsync(int checklistLogId)
        {
            await InitializeAsync();
            return await _database.Table<ChecklistLogItem>()
                .Where(i => i.ChecklistLogId == checklistLogId)
                .ToListAsync();
        }

        #endregion

        #region ChecklistTemplate Operations

        public async Task<List<ChecklistTemplate>> GetActiveTemplatesAsync()
        {
            await InitializeAsync();
            return await _database.Table<ChecklistTemplate>()
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<ChecklistTemplate>> GetAllTemplatesAsync()
        {
            await InitializeAsync();
            return await _database.Table<ChecklistTemplate>()
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
        }

        public async Task<ChecklistTemplate> GetTemplateAsync(int id)
        {
            await InitializeAsync();
            return await _database.Table<ChecklistTemplate>()
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveTemplateAsync(ChecklistTemplate template)
        {
            await InitializeAsync();
            template.ModifiedAt = DateTime.UtcNow;
            if (template.SyncStatus == 0) template.SyncStatus = 1; // Queue for sync

            if (template.Id != 0)
            {
                return await _database.UpdateAsync(template);
            }
            else
            {
                if (template.SyncStatus == 1) template.SyncStatus = 2; // New record
                template.CreatedAt = DateTime.UtcNow;
                return await _database.InsertAsync(template);
            }
        }

        public async Task DeleteTemplateAsync(ChecklistTemplate template)
        {
            // Delete items first
            await _database.ExecuteAsync("DELETE FROM checklist_template_items WHERE TemplateId = ?", template.Id);
            await _database.DeleteAsync(template);
        }

        public async Task UpdateTemplateOrderAsync(List<ChecklistTemplate> templates)
        {
            for (int i = 0; i < templates.Count; i++)
            {
                templates[i].DisplayOrder = i;
            }
            await _database.UpdateAllAsync(templates);
        }

        #endregion

        #region ChecklistTemplateItem Operations

        public async Task<List<ChecklistTemplateItem>> GetTemplateItemsAsync(int templateId)
        {
            await InitializeAsync();
            return await _database.Table<ChecklistTemplateItem>()
                .Where(i => i.TemplateId == templateId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        public async Task<int> SaveTemplateItemAsync(ChecklistTemplateItem item)
        {
            await InitializeAsync();
            item.ModifiedAt = DateTime.UtcNow;
            if (item.SyncStatus == 0) item.SyncStatus = 1; // Queue for sync

            if (item.Id != 0)
            {
                return await _database.UpdateAsync(item);
            }
            else
            {
                if (item.SyncStatus == 1) item.SyncStatus = 2; // New record
                item.CreatedAt = DateTime.UtcNow;
                return await _database.InsertAsync(item);
            }
        }

        public Task<int> DeleteTemplateItemAsync(ChecklistTemplateItem item)
        {
            return _database.DeleteAsync(item);
        }

        public async Task DeleteAllTemplateItemsAsync(int templateId)
        {
            await _database.ExecuteAsync("DELETE FROM checklist_template_items WHERE TemplateId = ?", templateId);
        }

        public async Task ReorderTemplateItemsAsync(List<ChecklistTemplateItem> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].DisplayOrder = i;
            }
            await _database.UpdateAllAsync(items);
        }

        #endregion

        #region PilotProfile Operations

        public async Task<PilotProfile?> GetPilotProfileAsync()
        {
            await InitializeAsync();
            return await _database.Table<PilotProfile>().FirstOrDefaultAsync();
        }

        public async Task<int> SavePilotProfileAsync(PilotProfile profile)
        {
            await InitializeAsync();
            profile.ModifiedAt = DateTime.UtcNow;
            if (profile.SyncStatus == 0) profile.SyncStatus = 1; // Queue for sync

            if (profile.Id != 0)
            {
                return await _database.UpdateAsync(profile);
            }
            else
            {
                if (profile.SyncStatus == 1) profile.SyncStatus = 2; // New record
                profile.CreatedAt = DateTime.UtcNow;
                return await _database.InsertAsync(profile);
            }
        }

        #endregion

        #region PpcFrame Operations

        public async Task<List<PpcFrame>> GetPpcFramesAsync()
        {
            await InitializeAsync();
            return await _database.Table<PpcFrame>().ToListAsync();
        }

        public async Task<PpcFrame?> GetActivePpcFrameAsync()
        {
            await InitializeAsync();
            return await _database.Table<PpcFrame>()
                .Where(f => f.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<PpcFrame> GetPpcFrameAsync(int id)
        {
            await InitializeAsync();
            return await _database.Table<PpcFrame>()
                .Where(f => f.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SavePpcFrameAsync(PpcFrame frame)
        {
            await InitializeAsync();
            frame.ModifiedAt = DateTime.UtcNow;
            if (frame.SyncStatus == 0) frame.SyncStatus = 1; // Queue for sync

            if (frame.Id != 0)
            {
                return await _database.UpdateAsync(frame);
            }
            else
            {
                if (frame.SyncStatus == 1) frame.SyncStatus = 2; // New record
                frame.CreatedAt = DateTime.UtcNow;
                return await _database.InsertAsync(frame);
            }
        }

        public Task<int> DeletePpcFrameAsync(PpcFrame frame)
        {
            return _database.DeleteAsync(frame);
        }

        #endregion

        #region Engine Operations

        public async Task<Engine?> GetEngineByFrameAsync(int ppcFrameId)
        {
            await InitializeAsync();
            return await _database.Table<Engine>()
                .Where(e => e.PpcFrameId == ppcFrameId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveEngineAsync(Engine engine)
        {
            await InitializeAsync();
            engine.ModifiedAt = DateTime.UtcNow;
            if (engine.SyncStatus == 0) engine.SyncStatus = 1; // Queue for sync

            if (engine.Id != 0)
            {
                return await _database.UpdateAsync(engine);
            }
            else
            {
                if (engine.SyncStatus == 1) engine.SyncStatus = 2; // New record
                engine.CreatedAt = DateTime.UtcNow;
                return await _database.InsertAsync(engine);
            }
        }

        #endregion

        #region Wing Operations

        public async Task<Wing?> GetWingByFrameAsync(int ppcFrameId)
        {
            await InitializeAsync();
            return await _database.Table<Wing>()
                .Where(w => w.PpcFrameId == ppcFrameId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveWingAsync(Wing wing)
        {
            await InitializeAsync();
            wing.ModifiedAt = DateTime.UtcNow;
            if (wing.SyncStatus == 0) wing.SyncStatus = 1; // Queue for sync

            if (wing.Id != 0)
            {
                return await _database.UpdateAsync(wing);
            }
            else
            {
                if (wing.SyncStatus == 1) wing.SyncStatus = 2; // New record
                wing.CreatedAt = DateTime.UtcNow;
                return await _database.InsertAsync(wing);
            }
        }

        #endregion

        #region Propeller Operations

        public async Task<Propeller?> GetPropellerByFrameAsync(int ppcFrameId)
        {
            await InitializeAsync();
            return await _database.Table<Propeller>()
                .Where(p => p.PpcFrameId == ppcFrameId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SavePropellerAsync(Propeller propeller)
        {
            await InitializeAsync();
            propeller.ModifiedAt = DateTime.UtcNow;
            if (propeller.SyncStatus == 0) propeller.SyncStatus = 1; // Queue for sync

            if (propeller.Id != 0)
            {
                return await _database.UpdateAsync(propeller);
            }
            else
            {
                if (propeller.SyncStatus == 1) propeller.SyncStatus = 2; // New record
                propeller.CreatedAt = DateTime.UtcNow;
                return await _database.InsertAsync(propeller);
            }
        }

        #endregion

        #region MaintenanceLog Operations

        public async Task<List<MaintenanceLog>> GetMaintenanceLogsAsync(int? ppcFrameId = null)
        {
            await InitializeAsync();
            if (ppcFrameId.HasValue)
            {
                return await _database.Table<MaintenanceLog>()
                    .Where(m => m.PpcFrameId == ppcFrameId.Value)
                    .OrderByDescending(m => m.MaintenanceDate)
                    .ToListAsync();
            }
            return await _database.Table<MaintenanceLog>()
                .OrderByDescending(m => m.MaintenanceDate)
                .ToListAsync();
        }

        public async Task<MaintenanceLog> GetMaintenanceLogAsync(int id)
        {
            await InitializeAsync();
            return await _database.Table<MaintenanceLog>()
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveMaintenanceLogAsync(MaintenanceLog log)
        {
            await InitializeAsync();
            log.ModifiedAt = DateTime.UtcNow;
            if (log.SyncStatus == 0) log.SyncStatus = 1; // Queue for sync

            if (log.Id != 0)
            {
                return await _database.UpdateAsync(log);
            }
            else
            {
                if (log.SyncStatus == 1) log.SyncStatus = 2; // New record
                log.CreatedAt = DateTime.UtcNow;
                return await _database.InsertAsync(log);
            }
        }

        public Task<int> DeleteMaintenanceLogAsync(MaintenanceLog log)
        {
            return _database.DeleteAsync(log);
        }

        public async Task<List<MaintenanceLog>> GetUpcomingMaintenanceAsync()
        {
            await InitializeAsync();
            var all = await _database.Table<MaintenanceLog>().ToListAsync();
            var now = DateTime.Now;
            return all
                .Where(m => m.NextServiceDueDate.HasValue && m.NextServiceDueDate.Value > now)
                .OrderBy(m => m.NextServiceDueDate)
                .Take(5)
                .ToList();
        }

        #endregion

        #region AppSetting Operations

        public async Task<string?> GetSettingAsync(string key)
        {
            await InitializeAsync();
            var setting = await _database.Table<AppSetting>()
                .Where(s => s.Key == key)
                .FirstOrDefaultAsync();
            return setting?.Value;
        }

        public async Task SetSettingAsync(string key, string? value)
        {
            await InitializeAsync();
            var existing = await _database.Table<AppSetting>()
                .Where(s => s.Key == key)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.Value = value;
                await _database.UpdateAsync(existing);
            }
            else
            {
                await _database.InsertAsync(new AppSetting { Key = key, Value = value });
            }
        }

        #endregion

        #region Reset / Data Management

        public async Task ResetChecklistsToDefaultsAsync()
        {
            await InitializeAsync();
            // Delete all template items and templates
            await _database.ExecuteAsync("DELETE FROM checklist_template_items");
            await _database.ExecuteAsync("DELETE FROM checklist_templates");

            // Re-seed
            await SeedDefaultChecklistsIfNeeded();
        }

        public async Task ClearAllDataAsync()
        {
            await InitializeAsync();
            await _database.ExecuteAsync("DELETE FROM checklist_log_items");
            await _database.ExecuteAsync("DELETE FROM checklist_logs");
            await _database.ExecuteAsync("DELETE FROM flights");
            await _database.ExecuteAsync("DELETE FROM maintenance_logs");
        }

        #endregion
    }
}
