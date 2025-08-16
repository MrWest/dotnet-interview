using System.Diagnostics.CodeAnalysis;
using MediatR;
using TodoApi.Infrastructure;
using TodoApi.Synchronization.Models;

namespace TodoApi.Mediation.Synchronization.Commands
{
    /// <summary>
    /// Command to manually trigger a synchronization operation.
    /// Useful for admin interfaces or testing purposes.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TriggerSyncCommand : IRequest<ServiceResult<TriggerSyncCommandResponse>>
    {
        /// <summary>
        /// Type of synchronization to perform.
        /// </summary>
        public SyncType SyncType { get; set; } = SyncType.Bidirectional;

        /// <summary>
        /// Whether to force synchronization even if one is already in progress.
        /// </summary>
        public bool Force { get; set; } = false;
    }

    /// <summary>
    /// Types of synchronization operations that can be triggered.
    /// </summary>
    public enum SyncType
    {
        /// <summary>
        /// Pull changes from external API only.
        /// </summary>
        PullOnly,

        /// <summary>
        /// Push local changes to external API only.
        /// </summary>
        PushOnly,

        /// <summary>
        /// Perform complete bidirectional synchronization.
        /// </summary>
        Bidirectional
    }

    /// <summary>
    /// Response for the trigger sync command.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TriggerSyncCommandResponse
    {
        /// <summary>
        /// Indicates whether the synchronization was triggered successfully.
        /// </summary>
        public bool WasTriggered { get; set; }

        /// <summary>
        /// Message describing the result of the trigger operation.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Detailed result of the synchronization operation, if completed.
        /// </summary>
        public SyncResult? SyncResult { get; set; }

        /// <summary>
        /// Timestamp when the synchronization was triggered.
        /// </summary>
        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    }
}