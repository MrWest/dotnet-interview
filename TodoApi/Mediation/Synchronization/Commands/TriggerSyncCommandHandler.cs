using MediatR;
using Microsoft.Extensions.Logging;
using TodoApi.Infrastructure;
using TodoApi.Synchronization;

namespace TodoApi.Mediation.Synchronization.Commands
{
    /// <summary>
    /// Handler for manually triggered synchronization operations.
    /// Provides a way to trigger sync operations through the API.
    /// </summary>
    public class TriggerSyncCommandHandler : IRequestHandler<TriggerSyncCommand, ServiceResult<TriggerSyncCommandResponse>>
    {
        private readonly ISynchronizationService _synchronizationService;
        private readonly ILogger<TriggerSyncCommandHandler> _logger;

        public TriggerSyncCommandHandler(
            ISynchronizationService synchronizationService,
            ILogger<TriggerSyncCommandHandler> logger)
        {
            _synchronizationService = synchronizationService ?? throw new ArgumentNullException(nameof(synchronizationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the trigger sync command by executing the requested synchronization operation.
        /// </summary>
        public async Task<ServiceResult<TriggerSyncCommandResponse>> Handle(
            TriggerSyncCommand request, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Manual synchronization triggered. Type: {SyncType}, Force: {Force}",
                request.SyncType, request.Force);

            try
            {
                // Check if sync is already in progress (unless forced)
                if (!request.Force)
                {
                    var status = await _synchronizationService.GetLastSyncStatusAsync();
                    if (status.IsSyncInProgress)
                    {
                        return ServiceResult<TriggerSyncCommandResponse>.Success(new TriggerSyncCommandResponse
                        {
                            WasTriggered = false,
                            Message = "Synchronization is already in progress. Use Force=true to override."
                        });
                    }
                }

                // Execute the requested synchronization type
                var syncResult = request.SyncType switch
                {
                    SyncType.PullOnly => await _synchronizationService.PullFromExternalAsync(cancellationToken),
                    SyncType.PushOnly => await _synchronizationService.PushToExternalAsync(cancellationToken),
                    SyncType.Bidirectional => await _synchronizationService.SynchronizeAsync(cancellationToken),
                    _ => throw new ArgumentException($"Unknown sync type: {request.SyncType}")
                };

                var response = new TriggerSyncCommandResponse
                {
                    WasTriggered = true,
                    Message = syncResult.IsSuccess 
                        ? $"Synchronization completed successfully. {syncResult.Message}"
                        : $"Synchronization completed with errors. {syncResult.Message}",
                    SyncResult = syncResult
                };

                _logger.LogInformation("Manual synchronization completed. Success: {IsSuccess}, Duration: {Duration}ms",
                    syncResult.IsSuccess, syncResult.Duration.TotalMilliseconds);

                return ServiceResult<TriggerSyncCommandResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual synchronization trigger");
                
                return ServiceResult<TriggerSyncCommandResponse>.Failure(
                    "An error occurred while triggering synchronization",
                    400);
            }
        }
    }
}