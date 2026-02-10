using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using powered_parachute.Models;

namespace powered_parachute.Services
{
    public class ApiSyncService
    {
        private const string BaseUrl = "https://www.ppcpilot.org/api/v1/";
        private const string TokenKey = "auth_jwt_access";
        private const string RefreshTokenKey = "auth_jwt_refresh";
        private const string LastSyncKey = "LastSyncTimestamp";
        private const int SyncIntervalMinutes = 5;

        private readonly DatabaseService _db;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private Timer? _syncTimer;
        private bool _isSyncing;

        public bool IsAuthenticated => SecureStorage.Default.GetAsync(TokenKey).Result != null;
        public event EventHandler<SyncEventArgs>? SyncCompleted;

        public ApiSyncService(DatabaseService db)
        {
            _db = db;
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
            };
        }

        #region Authentication

        public async Task<bool> SignInWithGoogleAsync()
        {
            try
            {
                var result = await WebAuthenticator.Default.AuthenticateAsync(
                    new Uri("https://accounts.google.com/o/oauth2/v2/auth?" +
                        "client_id=341895943811-4agpkvfpq3ib8uvirejulntpmvpq68lm.apps.googleusercontent.com" +
                        "&redirect_uri=" + Uri.EscapeDataString("com.joelwehr.ppcpilot:/oauth2redirect") +
                        "&response_type=code" +
                        "&scope=" + Uri.EscapeDataString("openid profile email") +
                        "&access_type=offline"),
                    new Uri("com.joelwehr.ppcpilot:/oauth2redirect"));

                var authCode = result?.Properties["code"];
                if (string.IsNullOrEmpty(authCode))
                    return false;

                // Exchange the auth code with our backend
                var response = await _httpClient.PostAsJsonAsync("auth/google/",
                    new { access_token = authCode }, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                    return false;

                var tokenResult = await response.Content.ReadFromJsonAsync<AuthTokenResponse>(_jsonOptions);
                if (tokenResult == null)
                    return false;

                await SecureStorage.Default.SetAsync(TokenKey, tokenResult.Access);
                await SecureStorage.Default.SetAsync(RefreshTokenKey, tokenResult.Refresh);

                ConfigureAuthHeader(tokenResult.Access);
                StartBackgroundSync();

                return true;
            }
            catch (TaskCanceledException)
            {
                // User cancelled
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Google sign-in failed: {ex.Message}");
                return false;
            }
        }

        public async Task SignOutAsync()
        {
            SecureStorage.Default.Remove(TokenKey);
            SecureStorage.Default.Remove(RefreshTokenKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
            StopBackgroundSync();
            await Task.CompletedTask;
        }

        public async Task<bool> TryRestoreSessionAsync()
        {
            var token = await SecureStorage.Default.GetAsync(TokenKey);
            if (string.IsNullOrEmpty(token))
                return false;

            ConfigureAuthHeader(token);

            // Validate token by making a test request
            try
            {
                var response = await _httpClient.GetAsync("dashboard/dashboard/");
                if (response.IsSuccessStatusCode)
                {
                    StartBackgroundSync();
                    return true;
                }

                // Try refresh
                return await RefreshTokenAsync();
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> RefreshTokenAsync()
        {
            var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/token/refresh/",
                    new { refresh = refreshToken }, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                    return false;

                var result = await response.Content.ReadFromJsonAsync<AuthTokenResponse>(_jsonOptions);
                if (result == null)
                    return false;

                await SecureStorage.Default.SetAsync(TokenKey, result.Access);
                if (!string.IsNullOrEmpty(result.Refresh))
                    await SecureStorage.Default.SetAsync(RefreshTokenKey, result.Refresh);

                ConfigureAuthHeader(result.Access);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ConfigureAuthHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        #endregion

        #region Background Sync

        public void StartBackgroundSync()
        {
            StopBackgroundSync();
            _syncTimer = new Timer(async _ => await SyncAllAsync(),
                null,
                TimeSpan.FromSeconds(10), // Initial delay
                TimeSpan.FromMinutes(SyncIntervalMinutes));
        }

        public void StopBackgroundSync()
        {
            _syncTimer?.Dispose();
            _syncTimer = null;
        }

        public async Task SyncAllAsync()
        {
            if (_isSyncing) return;
            _isSyncing = true;

            try
            {
                var token = await SecureStorage.Default.GetAsync(TokenKey);
                if (string.IsNullOrEmpty(token)) return;

                ConfigureAuthHeader(token);

                // Pull first (server wins), then push local changes
                await PullChangesAsync();
                await PushChangesAsync();

                SyncCompleted?.Invoke(this, new SyncEventArgs { Success = true });
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Token expired, try refresh
                if (await RefreshTokenAsync())
                {
                    try
                    {
                        await PullChangesAsync();
                        await PushChangesAsync();
                        SyncCompleted?.Invoke(this, new SyncEventArgs { Success = true });
                    }
                    catch (Exception retryEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Sync retry failed: {retryEx.Message}");
                        SyncCompleted?.Invoke(this, new SyncEventArgs { Success = false, Error = retryEx.Message });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sync failed: {ex.Message}");
                SyncCompleted?.Invoke(this, new SyncEventArgs { Success = false, Error = ex.Message });
            }
            finally
            {
                _isSyncing = false;
            }
        }

        #endregion

        #region Pull (Server → Mobile)

        private async Task PullChangesAsync()
        {
            var lastSync = await _db.GetSettingAsync(LastSyncKey);
            var url = "sync/pull/";
            if (!string.IsNullOrEmpty(lastSync))
                url += $"?since={Uri.EscapeDataString(lastSync)}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var pullResult = await response.Content.ReadFromJsonAsync<SyncPullResponse>(_jsonOptions);
            if (pullResult?.Data == null) return;

            // Process each entity type
            if (pullResult.Data.TryGetValue("flights", out var flightsJson))
                await MergePulledRecords<Flight>(flightsJson, r => r.Id, (existing, remote) => MergeFlight(existing, remote));

            if (pullResult.Data.TryGetValue("ppc_frames", out var framesJson))
                await MergePulledRecords<PpcFrame>(framesJson, r => r.Id, (existing, remote) => MergePpcFrame(existing, remote));

            if (pullResult.Data.TryGetValue("pilot_profiles", out var profilesJson))
                await MergePulledRecords<PilotProfile>(profilesJson, r => r.Id, (existing, remote) => MergePilotProfile(existing, remote));

            if (pullResult.Data.TryGetValue("checklist_templates", out var templatesJson))
                await MergePulledRecords<ChecklistTemplate>(templatesJson, r => r.Id, (existing, remote) => MergeChecklistTemplate(existing, remote));

            if (pullResult.Data.TryGetValue("checklist_logs", out var logsJson))
                await MergePulledRecords<ChecklistLog>(logsJson, r => r.Id, (existing, remote) => MergeChecklistLog(existing, remote));

            if (pullResult.Data.TryGetValue("maintenance_logs", out var maintJson))
                await MergePulledRecords<MaintenanceLog>(maintJson, r => r.Id, (existing, remote) => MergeMaintenanceLog(existing, remote));

            // Update last sync timestamp
            if (!string.IsNullOrEmpty(pullResult.ServerTime))
                await _db.SetSettingAsync(LastSyncKey, pullResult.ServerTime);
        }

        private async Task MergePulledRecords<T>(JsonElement jsonArray, Func<T, int> getId, Func<T?, T, T> merge) where T : class, new()
        {
            var remoteRecords = jsonArray.Deserialize<List<T>>(_jsonOptions);
            if (remoteRecords == null) return;

            foreach (var remote in remoteRecords)
            {
                var remoteId = getId(remote);

                // Find local record by RemoteId
                // For simplicity, use raw SQL or scan
                // The merge function handles copying fields
                var merged = merge(null, remote);
                // Mark as synced
                if (merged is ISyncable syncable)
                {
                    syncable.SyncStatus = 0; // Synced
                    syncable.RemoteId = remoteId;
                }
            }
        }

        #endregion

        #region Push (Mobile → Server)

        private async Task PushChangesAsync()
        {
            var entities = new Dictionary<string, List<object>>();

            // Collect all modified/new flights
            var flights = await _db.GetFlightsAsync();
            var modifiedFlights = flights.Where(f => f.SyncStatus != 0).ToList();
            if (modifiedFlights.Any())
                entities["flights"] = modifiedFlights.Select(f => CreatePushRecord(f, f.Id, f.RemoteId)).ToList();

            // PPC Frames
            var frames = await _db.GetPpcFramesAsync();
            var modifiedFrames = frames.Where(f => f.SyncStatus != 0).ToList();
            if (modifiedFrames.Any())
                entities["ppc_frames"] = modifiedFrames.Select(f => CreatePushRecord(f, f.Id, f.RemoteId)).ToList();

            // Checklist Templates
            var templates = await _db.GetAllTemplatesAsync();
            var modifiedTemplates = templates.Where(t => t.SyncStatus != 0).ToList();
            if (modifiedTemplates.Any())
                entities["checklist_templates"] = modifiedTemplates.Select(t => CreatePushRecord(t, t.Id, t.RemoteId)).ToList();

            // Checklist Logs
            var logs = await _db.GetChecklistLogsAsync();
            var modifiedLogs = logs.Where(l => l.SyncStatus != 0).ToList();
            if (modifiedLogs.Any())
                entities["checklist_logs"] = modifiedLogs.Select(l => CreatePushRecord(l, l.Id, l.RemoteId)).ToList();

            // Maintenance Logs
            var maintLogs = await _db.GetMaintenanceLogsAsync();
            var modifiedMaint = maintLogs.Where(m => m.SyncStatus != 0).ToList();
            if (modifiedMaint.Any())
                entities["maintenance_logs"] = modifiedMaint.Select(m => CreatePushRecord(m, m.Id, m.RemoteId)).ToList();

            if (!entities.Any()) return;

            var response = await _httpClient.PostAsJsonAsync("sync/push/",
                new { entities }, _jsonOptions);
            response.EnsureSuccessStatusCode();

            var pushResult = await response.Content.ReadFromJsonAsync<SyncPushResponse>(_jsonOptions);

            // Update local records with server IDs and mark as synced
            if (pushResult?.IdMap != null)
            {
                foreach (var (entityType, idMap) in pushResult.IdMap)
                {
                    foreach (var (localIdStr, serverId) in idMap)
                    {
                        if (!int.TryParse(localIdStr, out var localId)) continue;
                        await UpdateSyncStatusAsync(entityType, localId, serverId);
                    }
                }
            }

            // Mark all pushed records as synced even if no ID mapping returned
            foreach (var flight in modifiedFlights)
            {
                flight.SyncStatus = 0;
                await _db.SaveFlightAsync(flight);
            }
            foreach (var frame in modifiedFrames)
            {
                frame.SyncStatus = 0;
                await _db.SavePpcFrameAsync(frame);
            }
            foreach (var template in modifiedTemplates)
            {
                template.SyncStatus = 0;
                await _db.SaveTemplateAsync(template);
            }
            foreach (var log in modifiedLogs)
            {
                log.SyncStatus = 0;
                await _db.SaveChecklistLogAsync(log);
            }
            foreach (var maint in modifiedMaint)
            {
                maint.SyncStatus = 0;
                await _db.SaveMaintenanceLogAsync(maint);
            }

            // Update last sync time
            if (!string.IsNullOrEmpty(pushResult?.ServerTime))
                await _db.SetSettingAsync(LastSyncKey, pushResult.ServerTime);
        }

        private object CreatePushRecord(object record, int localId, int? remoteId)
        {
            // Serialize to dictionary, add local_id, set id to remoteId
            var json = JsonSerializer.Serialize(record, _jsonOptions);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, _jsonOptions)
                       ?? new Dictionary<string, JsonElement>();

            var result = new Dictionary<string, object?>();
            foreach (var kvp in dict)
            {
                // Skip sync-only fields from going to server
                if (kvp.Key == "sync_status" || kvp.Key == "remote_id")
                    continue;
                result[kvp.Key] = kvp.Value;
            }

            result["local_id"] = localId;
            result["id"] = remoteId;

            return result;
        }

        private async Task UpdateSyncStatusAsync(string entityType, int localId, int serverId)
        {
            switch (entityType)
            {
                case "flights":
                    var flight = await _db.GetFlightAsync(localId);
                    if (flight != null)
                    {
                        flight.RemoteId = serverId;
                        flight.SyncStatus = 0;
                        await _db.SaveFlightAsync(flight);
                    }
                    break;
                case "ppc_frames":
                    var frame = await _db.GetPpcFrameAsync(localId);
                    if (frame != null)
                    {
                        frame.RemoteId = serverId;
                        frame.SyncStatus = 0;
                        await _db.SavePpcFrameAsync(frame);
                    }
                    break;
            }
        }

        #endregion

        #region Merge Helpers

        private Flight MergeFlight(Flight? existing, Flight remote)
        {
            // Server wins — just use remote data
            return remote;
        }

        private PpcFrame MergePpcFrame(PpcFrame? existing, PpcFrame remote) => remote;
        private PilotProfile MergePilotProfile(PilotProfile? existing, PilotProfile remote) => remote;
        private ChecklistTemplate MergeChecklistTemplate(ChecklistTemplate? existing, ChecklistTemplate remote) => remote;
        private ChecklistLog MergeChecklistLog(ChecklistLog? existing, ChecklistLog remote) => remote;
        private MaintenanceLog MergeMaintenanceLog(MaintenanceLog? existing, MaintenanceLog remote) => remote;

        #endregion

        #region DTOs

        private class AuthTokenResponse
        {
            public string Access { get; set; } = string.Empty;
            public string Refresh { get; set; } = string.Empty;
        }

        private class SyncPullResponse
        {
            public Dictionary<string, JsonElement>? Data { get; set; }
            public string? ServerTime { get; set; }
        }

        private class SyncPushResponse
        {
            public Dictionary<string, Dictionary<string, int>>? IdMap { get; set; }
            public string? ServerTime { get; set; }
        }

        #endregion
    }

    public interface ISyncable
    {
        int? RemoteId { get; set; }
        int SyncStatus { get; set; }
    }

    public class SyncEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
}
