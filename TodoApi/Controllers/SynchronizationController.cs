using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Infrastructure;
using TodoApi.Mediation.Synchronization.Commands;
using TodoApi.Synchronization;

namespace TodoApi.Controllers
{
    /// <summary>
    /// Controller for managing synchronization operations.
    /// Provides endpoints for manually triggering sync and checking sync status.
    /// </summary>
    [Route("api/synchronization")]
    [ApiController]
    public class SynchronizationController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ISynchronizationService _synchronizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator instance for handling commands and queries.</param>
        /// <param name="synchronizationService">The synchronization service for status queries.</param>
        public SynchronizationController(
            IMediator mediator,
            ISynchronizationService synchronizationService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _synchronizationService = synchronizationService ?? throw new ArgumentNullException(nameof(synchronizationService));
        }

        /// <summary>
        /// Manually triggers a synchronization operation.
        /// </summary>
        /// <param name="command">The sync trigger command with options.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Result of the synchronization trigger operation.</returns>
        [HttpPost("trigger")]
        public async Task<ActionResult<TriggerSyncCommandResponse>> TriggerSync(
            [FromBody] TriggerSyncCommand command,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ServiceResult(result);
        }

        /// <summary>
        /// Gets the current synchronization status.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Current synchronization status information.</returns>
        [HttpGet("status")]
        public async Task<ActionResult<SyncStatus>> GetSyncStatus(CancellationToken cancellationToken = default)
        {
            var status = await _synchronizationService.GetLastSyncStatusAsync();
            return Ok(status);
        }

        /// <summary>
        /// Performs a quick health check of the external API connectivity.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Health check result.</returns>
        [HttpGet("health")]
        public async Task<ActionResult<object>> HealthCheck(CancellationToken cancellationToken = default)
        {
            try
            {
                // Try to get sync status as a basic health check
                var status = await _synchronizationService.GetLastSyncStatusAsync();
                
                return Ok(new
                {
                    Status = "Healthy",
                    LastSync = status.LastSuccessfulSync,
                    PendingSync = status.PendingSyncCount,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}