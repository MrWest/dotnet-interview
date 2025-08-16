using TodoApi.Synchronization.Models;

namespace TodoApi.Synchronization
{
    /// <summary>
    /// Service interface for managing synchronization between local and external Todo APIs.
    /// Provides methods for bidirectional synchronization, conflict resolution, and status monitoring.
    /// </summary>
    public interface ISynchronizationService
    {
        /// <summary>
        /// Performs a complete bidirectional synchronization between local and external APIs.
        /// This includes both pulling changes from external API and pushing local changes.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
        /// <returns>Result of the synchronization operation with detailed statistics.</returns>
        Task<SyncResult> SynchronizeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Pulls changes from the external API and applies them locally.
        /// This is a one-way synchronization from external to local.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
        /// <returns>Result of the pull operation with statistics.</returns>
        Task<SyncResult> PullFromExternalAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Pushes local changes to the external API.
        /// This is a one-way synchronization from local to external.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
        /// <returns>Result of the push operation with statistics.</returns>
        Task<SyncResult> PushToExternalAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current status of the synchronization system.
        /// </summary>
        /// <returns>Current synchronization status and statistics.</returns>
        Task<SyncStatus> GetLastSyncStatusAsync();
    }
}

