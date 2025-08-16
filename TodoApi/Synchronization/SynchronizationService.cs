using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoApi.Data;
using TodoApi.ExternalApi;
using TodoApi.ExternalApi.Models;
using TodoApi.Models;
using TodoApi.Synchronization.Models;

namespace TodoApi.Synchronization
{
    /// <summary>
    /// Implementation of the synchronization service that handles bidirectional sync
    /// between local TodoApi and external Todo API.
    /// </summary>
    public class SynchronizationService : ISynchronizationService
    {
        private readonly TodoContext _dbContext;
        private readonly IExternalTodoApiClient _externalApiClient;
        private readonly ILogger<SynchronizationService> _logger;
        private readonly SemaphoreSlim _syncSemaphore;
        private static SyncStatus _lastSyncStatus = new();

        public SynchronizationService(
            TodoContext dbContext,
            IExternalTodoApiClient externalApiClient,
            ILogger<SynchronizationService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _externalApiClient = externalApiClient ?? throw new ArgumentNullException(nameof(externalApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _syncSemaphore = new SemaphoreSlim(1, 1); // Only allow one sync operation at a time
        }

        /// <summary>
        /// Performs complete bidirectional synchronization.
        /// </summary>
        public async Task<SyncResult> SynchronizeAsync(CancellationToken cancellationToken = default)
        {
            if (!await _syncSemaphore.WaitAsync(100, cancellationToken))
            {
                return SyncResult.Failure("Another synchronization operation is already in progress");
            }

            try
            {
                _lastSyncStatus.IsSyncInProgress = true;
                _lastSyncStatus.LastAttemptedSync = DateTime.UtcNow;

                _logger.LogInformation("Starting bidirectional synchronization");
                var startTime = DateTime.UtcNow;

                var result = new SyncResult { StartedAt = startTime };
                var statistics = new SyncStatistics();
                var errors = new List<SyncError>();
                var conflicts = new List<SyncConflict>();

                try
                {
                    // Phase 1: Pull changes from external API
                    _logger.LogInformation("Phase 1: Pulling changes from external API");
                    var pullResult = await PullFromExternalInternalAsync(cancellationToken);

                    statistics.TodoListsPulled = pullResult.Statistics.TodoListsPulled;
                    statistics.TodoItemsPulled = pullResult.Statistics.TodoItemsPulled;
                    statistics.ConflictsResolved += pullResult.Statistics.ConflictsResolved;
                    errors.AddRange(pullResult.Errors);
                    conflicts.AddRange(pullResult.ResolvedConflicts);

                    // Phase 2: Push local changes to external API
                    _logger.LogInformation("Phase 2: Pushing local changes to external API");
                    var pushResult = await PushToExternalInternalAsync(cancellationToken);

                    statistics.TodoListsPushed = pushResult.Statistics.TodoListsPushed;
                    statistics.TodoItemsPushed = pushResult.Statistics.TodoItemsPushed;
                    statistics.ConflictsResolved += pushResult.Statistics.ConflictsResolved;
                    errors.AddRange(pushResult.Errors);
                    conflicts.AddRange(pushResult.ResolvedConflicts);

                    var duration = DateTime.UtcNow - startTime;
                    var isSuccess = errors.Count == 0;

                    result.IsSuccess = isSuccess;
                    result.Message = isSuccess
                        ? $"Bidirectional synchronization completed successfully in {duration.TotalMilliseconds:F0}ms"
                        : $"Bidirectional synchronization completed with {errors.Count} errors in {duration.TotalMilliseconds:F0}ms";
                    result.Statistics = statistics;
                    result.Errors = errors;
                    result.ResolvedConflicts = conflicts;
                    result.Duration = duration;
                    result.CompletedAt = DateTime.UtcNow;

                    // Update sync status
                    _lastSyncStatus.LastSyncStatistics = statistics;
                    _lastSyncStatus.ConsecutiveFailures = isSuccess ? 0 : _lastSyncStatus.ConsecutiveFailures + 1;
                    if (isSuccess)
                    {
                        _lastSyncStatus.LastSuccessfulSync = DateTime.UtcNow;
                    }

                    _logger.LogInformation("Bidirectional synchronization completed. Success: {IsSuccess}, Duration: {Duration}ms, Errors: {ErrorCount}",
                        isSuccess, duration.TotalMilliseconds, errors.Count);

                    return result;
                }
                catch (Exception ex)
                {
                    var duration = DateTime.UtcNow - startTime;
                    _logger.LogError(ex, "Bidirectional synchronization failed after {Duration}ms", duration.TotalMilliseconds);

                    _lastSyncStatus.ConsecutiveFailures++;

                    return SyncResult.Failure($"Synchronization failed: {ex.Message}", new List<SyncError>
                    {
                        new SyncError
                        {
                            EntityType = "System",
                            Operation = "Synchronize",
                            Message = ex.Message,
                            ExceptionDetails = ex.ToString()
                        }
                    });
                }
            }
            finally
            {
                _lastSyncStatus.IsSyncInProgress = false;
                _syncSemaphore.Release();
            }
        }

        /// <summary>
        /// Pulls changes from external API only.
        /// </summary>
        public async Task<SyncResult> PullFromExternalAsync(CancellationToken cancellationToken = default)
        {
            if (!await _syncSemaphore.WaitAsync(100, cancellationToken))
            {
                return SyncResult.Failure("Another synchronization operation is already in progress");
            }

            try
            {
                _lastSyncStatus.IsSyncInProgress = true;
                return await PullFromExternalInternalAsync(cancellationToken);
            }
            finally
            {
                _lastSyncStatus.IsSyncInProgress = false;
                _syncSemaphore.Release();
            }
        }

        /// <summary>
        /// Pushes local changes to external API only.
        /// </summary>
        public async Task<SyncResult> PushToExternalAsync(CancellationToken cancellationToken = default)
        {
            if (!await _syncSemaphore.WaitAsync(100, cancellationToken))
            {
                return SyncResult.Failure("Another synchronization operation is already in progress");
            }

            try
            {
                _lastSyncStatus.IsSyncInProgress = true;
                return await PushToExternalInternalAsync(cancellationToken);
            }
            finally
            {
                _lastSyncStatus.IsSyncInProgress = false;
                _syncSemaphore.Release();
            }
        }

        /// <summary>
        /// Gets the current synchronization status.
        /// </summary>
        public async Task<SyncStatus> GetLastSyncStatusAsync()
        {
            // Update pending sync count
            var pendingCount = await _dbContext.TodoList
                .Where(tl => !tl.IsSynced)
                .CountAsync() +
                await _dbContext.TodoListItem
                .Where(tli => !tli.IsSynced)
                .CountAsync();

            _lastSyncStatus.PendingSyncCount = pendingCount;
            _lastSyncStatus.StatusMessage = _lastSyncStatus.IsSyncInProgress
                ? "Synchronization in progress"
                : $"Ready for synchronization. {pendingCount} entities pending.";

            return _lastSyncStatus;
        }

        /// <summary>
        /// Internal implementation of pull operation.
        /// </summary>
        private async Task<SyncResult> PullFromExternalInternalAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting pull from external API");
            var startTime = DateTime.UtcNow;
            var statistics = new SyncStatistics();
            var errors = new List<SyncError>();
            var conflicts = new List<SyncConflict>();

            try
            {
                // Get all external TodoLists
                var externalTodoLists = await _externalApiClient.GetAllTodoListsAsync(cancellationToken);

                foreach (var externalList in externalTodoLists)
                {
                    try
                    {
                        await ProcessExternalTodoList(externalList, statistics, conflicts, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing external TodoList {ExternalId}", externalList.Id);
                        errors.Add(new SyncError
                        {
                            EntityType = "TodoList",
                            EntityId = externalList.Id?.ToString() ?? "unknown",
                            Operation = "Pull",
                            Message = ex.Message,
                            ExceptionDetails = ex.ToString()
                        });
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                var duration = DateTime.UtcNow - startTime;
                var isSuccess = errors.Count == 0;

                return new SyncResult
                {
                    IsSuccess = isSuccess,
                    Message = isSuccess
                        ? $"Pull completed successfully in {duration.TotalMilliseconds:F0}ms"
                        : $"Pull completed with {errors.Count} errors in {duration.TotalMilliseconds:F0}ms",
                    Statistics = statistics,
                    Errors = errors,
                    ResolvedConflicts = conflicts,
                    Duration = duration,
                    StartedAt = startTime,
                    CompletedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "Pull operation failed after {Duration}ms", duration.TotalMilliseconds);

                return SyncResult.Failure($"Pull operation failed: {ex.Message}", new List<SyncError>
                {
                    new SyncError
                    {
                        EntityType = "System",
                        Operation = "Pull",
                        Message = ex.Message,
                        ExceptionDetails = ex.ToString()
                    }
                });
            }
        }

        /// <summary>
        /// Internal implementation of push operation.
        /// </summary>
        private async Task<SyncResult> PushToExternalInternalAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting push to external API");
            var startTime = DateTime.UtcNow;
            var statistics = new SyncStatistics();
            var errors = new List<SyncError>();
            var conflicts = new List<SyncConflict>();

            try
            {
                // Get local TodoLists that need syncing
                var localTodoLists = await _dbContext.TodoList
                    .Include(tl => tl.Items)
                    .Where(tl => !tl.IsSynced || tl.Items.Any(item => !item.IsSynced))
                    .ToListAsync(cancellationToken);

                foreach (var localList in localTodoLists)
                {
                    try
                    {
                        await ProcessLocalTodoList(localList, statistics, conflicts, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing local TodoList {LocalId}", localList.Id);
                        errors.Add(new SyncError
                        {
                            EntityType = "TodoList",
                            EntityId = localList.Id.ToString(),
                            Operation = "Push",
                            Message = ex.Message,
                            ExceptionDetails = ex.ToString()
                        });
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                var duration = DateTime.UtcNow - startTime;
                var isSuccess = errors.Count == 0;

                return new SyncResult
                {
                    IsSuccess = isSuccess,
                    Message = isSuccess
                        ? $"Push completed successfully in {duration.TotalMilliseconds:F0}ms"
                        : $"Push completed with {errors.Count} errors in {duration.TotalMilliseconds:F0}ms",
                    Statistics = statistics,
                    Errors = errors,
                    ResolvedConflicts = conflicts,
                    Duration = duration,
                    StartedAt = startTime,
                    CompletedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "Push operation failed after {Duration}ms", duration.TotalMilliseconds);

                return SyncResult.Failure($"Push operation failed: {ex.Message}", new List<SyncError>
                {
                    new SyncError
                    {
                        EntityType = "System",
                        Operation = "Push",
                        Message = ex.Message,
                        ExceptionDetails = ex.ToString()
                    }
                });
            }
        }

        /// <summary>
        /// Processes an external TodoList and syncs it locally.
        /// </summary>
        private async Task ProcessExternalTodoList(
            ExternalTodoList externalList,
            SyncStatistics statistics,
            List<SyncConflict> conflicts,
            CancellationToken cancellationToken)
        {
            // Find existing local TodoList by ExternalId
            var localList = await _dbContext.TodoList
                .Include(tl => tl.Items)
                .FirstOrDefaultAsync(tl => tl.ExternalId == externalList.Id.ToString(), cancellationToken);

            if (localList == null)
            {
                // Create new local TodoList
                localList = new TodoList
                {
                    Name = externalList.Name,
                    ExternalId = externalList.Id.ToString(),
                    SourceId = externalList.SourceId,
                    CreatedAt = externalList.CreatedAt,
                    UpdatedAt = externalList.UpdatedAt,
                    LastSyncedAt = DateTime.UtcNow,
                    IsSynced = true
                };

                _dbContext.TodoList.Add(localList);
                statistics.TodoListsPulled++;

                _logger.LogDebug("Created new local TodoList from external ID {ExternalId}", externalList.Id);
            }
            else
            {
                // Check for conflicts and update if needed
                if (externalList.UpdatedAt > localList.UpdatedAt)
                {
                    if (localList.UpdatedAt > localList.LastSyncedAt)
                    {
                        // Conflict detected - external and local both modified since last sync
                        conflicts.Add(new SyncConflict
                        {
                            EntityType = "TodoList",
                            EntityId = localList.Id.ToString(),
                            ConflictDescription = $"Both external and local TodoList modified. External: {externalList.UpdatedAt}, Local: {localList.UpdatedAt}",
                            Resolution = "External version takes precedence (last-write-wins)"
                        });
                    }

                    // Update local with external data
                    localList.Name = externalList.Name;
                    localList.UpdatedAt = externalList.UpdatedAt;
                    localList.LastSyncedAt = DateTime.UtcNow;
                    localList.IsSynced = true;

                    statistics.TodoListsPulled++;
                    _logger.LogDebug("Updated local TodoList {LocalId} from external ID {ExternalId}", localList.Id, externalList.Id);
                }
            }

            // Process TodoItems
            if (externalList.TodoItems != null)
            {
                foreach (var externalItem in externalList.TodoItems)
                {
                    await ProcessExternalTodoItem(externalItem, localList, statistics, conflicts, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Processes an external TodoItem and syncs it locally.
        /// </summary>
        private async Task ProcessExternalTodoItem(
            ExternalTodoItem externalItem,
            TodoList parentList,
            SyncStatistics statistics,
            List<SyncConflict> conflicts,
            CancellationToken cancellationToken)
        {
            var localItem = parentList.Items.FirstOrDefault(item => item.ExternalId == externalItem.Id.ToString());

            if (localItem == null)
            {
                // Create new local TodoItem
                localItem = new TodoListItem
                {
                    Name = externalItem.Name,
                    Description = externalItem.Description ?? string.Empty,
                    Completed = externalItem.Completed,
                    Progress = 0, // External API doesn't have progress field
                    TodoListId = parentList.Id,
                    ExternalId = externalItem.Id.ToString(),
                    SourceId = externalItem.SourceId,
                    CreatedAt = externalItem.CreatedAt,
                    UpdatedAt = externalItem.UpdatedAt,
                    LastSyncedAt = DateTime.UtcNow,
                    IsSynced = true
                };

                parentList.Items.Add(localItem);
                statistics.TodoItemsPulled++;

                _logger.LogDebug("Created new local TodoItem from external ID {ExternalId}", externalItem.Id);
            }
            else
            {
                // Check for conflicts and update if needed
                if (externalItem.UpdatedAt > localItem.UpdatedAt)
                {
                    if (localItem.UpdatedAt > localItem.LastSyncedAt)
                    {
                        // Conflict detected
                        conflicts.Add(new SyncConflict
                        {
                            EntityType = "TodoItem",
                            EntityId = localItem.Id.ToString(),
                            ConflictDescription = $"Both external and local TodoItem modified. External: {externalItem.UpdatedAt}, Local: {localItem.UpdatedAt}",
                            Resolution = "External version takes precedence (last-write-wins)"
                        });
                    }

                    // Update local with external data
                    localItem.Name = externalItem.Name;
                    localItem.Description = externalItem.Description ?? string.Empty;
                    localItem.Completed = externalItem.Completed;
                    localItem.UpdatedAt = externalItem.UpdatedAt;
                    localItem.LastSyncedAt = DateTime.UtcNow;
                    localItem.IsSynced = true;

                    statistics.TodoItemsPulled++;
                    _logger.LogDebug("Updated local TodoItem {LocalId} from external ID {ExternalId}", localItem.Id, externalItem.Id);
                }
            }
        }
    }
}

