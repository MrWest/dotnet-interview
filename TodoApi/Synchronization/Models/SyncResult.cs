using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Synchronization.Models
{
    /// <summary>
    /// Represents the result of a synchronization operation with detailed statistics and error information.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SyncResult
    {
        /// <summary>
        /// Indicates whether the synchronization operation was successful overall.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Human-readable message describing the result of the synchronization.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Detailed statistics about the synchronization operation.
        /// </summary>
        public SyncStatistics Statistics { get; set; } = new();

        /// <summary>
        /// List of errors that occurred during synchronization.
        /// </summary>
        public List<SyncError> Errors { get; set; } = new();

        /// <summary>
        /// List of conflicts that were resolved during synchronization.
        /// </summary>
        public List<SyncConflict> ResolvedConflicts { get; set; } = new();

        /// <summary>
        /// Duration of the synchronization operation.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Timestamp when the synchronization started.
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when the synchronization completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Creates a successful sync result.
        /// </summary>
        public static SyncResult Success(string message, SyncStatistics? statistics = null)
        {
            return new SyncResult
            {
                IsSuccess = true,
                Message = message,
                Statistics = statistics ?? new SyncStatistics(),
                CompletedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a failed sync result.
        /// </summary>
        public static SyncResult Failure(string message, List<SyncError>? errors = null)
        {
            return new SyncResult
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<SyncError>(),
                CompletedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Statistics about a synchronization operation.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SyncStatistics
    {
        /// <summary>
        /// Number of TodoLists pulled from external API.
        /// </summary>
        public int TodoListsPulled { get; set; }

        /// <summary>
        /// Number of TodoLists pushed to external API.
        /// </summary>
        public int TodoListsPushed { get; set; }

        /// <summary>
        /// Number of TodoListItems pulled from external API.
        /// </summary>
        public int TodoItemsPulled { get; set; }

        /// <summary>
        /// Number of TodoListItems pushed to external API.
        /// </summary>
        public int TodoItemsPushed { get; set; }

        /// <summary>
        /// Number of conflicts that were resolved.
        /// </summary>
        public int ConflictsResolved { get; set; }

        /// <summary>
        /// Number of entities that failed to sync.
        /// </summary>
        public int FailedEntities { get; set; }

        /// <summary>
        /// Total number of entities processed.
        /// </summary>
        public int TotalEntitiesProcessed => TodoListsPulled + TodoListsPushed + TodoItemsPulled + TodoItemsPushed;
    }

    /// <summary>
    /// Represents an error that occurred during synchronization.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SyncError
    {
        /// <summary>
        /// Type of entity that caused the error.
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// ID of the entity that caused the error.
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Operation that was being performed when the error occurred.
        /// </summary>
        public string Operation { get; set; } = string.Empty;

        /// <summary>
        /// Error message describing what went wrong.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Full exception details if available.
        /// </summary>
        public string? ExceptionDetails { get; set; }

        /// <summary>
        /// Timestamp when the error occurred.
        /// </summary>
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents a conflict that was resolved during synchronization.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SyncConflict
    {
        /// <summary>
        /// Type of entity that had the conflict.
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// ID of the entity that had the conflict.
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Description of the conflict.
        /// </summary>
        public string ConflictDescription { get; set; } = string.Empty;

        /// <summary>
        /// How the conflict was resolved.
        /// </summary>
        public string Resolution { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the conflict was resolved.
        /// </summary>
        public DateTime ResolvedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents the current status of the synchronization system.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SyncStatus
    {
        /// <summary>
        /// Whether a synchronization operation is currently in progress.
        /// </summary>
        public bool IsSyncInProgress { get; set; }

        /// <summary>
        /// Timestamp of the last successful synchronization.
        /// </summary>
        public DateTime? LastSuccessfulSync { get; set; }

        /// <summary>
        /// Timestamp of the last attempted synchronization (successful or failed).
        /// </summary>
        public DateTime? LastAttemptedSync { get; set; }

        /// <summary>
        /// Number of entities pending synchronization.
        /// </summary>
        public int PendingSyncCount { get; set; }

        /// <summary>
        /// Number of consecutive failed synchronization attempts.
        /// </summary>
        public int ConsecutiveFailures { get; set; }

        /// <summary>
        /// Message describing the current status.
        /// </summary>
        public string StatusMessage { get; set; } = string.Empty;

        /// <summary>
        /// Statistics from the last synchronization operation.
        /// </summary>
        public SyncStatistics? LastSyncStatistics { get; set; }
    }
}

