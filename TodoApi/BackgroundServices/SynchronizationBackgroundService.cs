using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TodoApi.Synchronization;

namespace TodoApi.BackgroundServices
{
    /// <summary>
    /// Background service that performs automatic synchronization with the external Todo API
    /// at regular intervals. Runs independently of user requests to keep data in sync.
    /// </summary>
    public class SynchronizationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SynchronizationBackgroundService> _logger;
        private readonly SynchronizationOptions _options;

        public SynchronizationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<SynchronizationBackgroundService> logger,
            IOptions<SynchronizationOptions> options)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Main execution loop of the background service.
        /// Runs synchronization at configured intervals until the service is stopped.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Synchronization background service started. Interval: {IntervalMinutes} minutes, Enabled: {Enabled}",
                _options.IntervalMinutes, _options.Enabled);

            if (!_options.Enabled)
            {
                _logger.LogInformation("Synchronization background service is disabled via configuration");
                return;
            }

            // Wait for initial delay before starting first sync
            if (_options.InitialDelayMinutes > 0)
            {
                _logger.LogInformation("Waiting {InitialDelayMinutes} minutes before starting synchronization",
                    _options.InitialDelayMinutes);
                
                await Task.Delay(TimeSpan.FromMinutes(_options.InitialDelayMinutes), stoppingToken);
            }

            // Main synchronization loop
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformSynchronization(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Synchronization background service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in synchronization background service");
                }

                // Wait for the configured interval before next sync
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(_options.IntervalMinutes), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Synchronization background service is stopping during delay");
                    break;
                }
            }

            _logger.LogInformation("Synchronization background service stopped");
        }

        /// <summary>
        /// Performs a single synchronization operation using a scoped service instance.
        /// </summary>
        private async Task PerformSynchronization(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting scheduled synchronization");
            var startTime = DateTime.UtcNow;

            try
            {
                // Create a new scope for this synchronization operation
                // This ensures proper disposal of DbContext and other scoped services
                using var scope = _serviceProvider.CreateScope();
                var synchronizationService = scope.ServiceProvider.GetRequiredService<ISynchronizationService>();

                // Perform the synchronization
                var result = await synchronizationService.SynchronizeAsync(cancellationToken);

                var duration = DateTime.UtcNow - startTime;

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Scheduled synchronization completed successfully in {Duration}ms. " +
                        "Lists: {TodoListsPulled} pulled, {TodoListsPushed} pushed. " +
                        "Items: {TodoItemsPulled} pulled, {TodoItemsPushed} pushed. " +
                        "Conflicts: {ConflictsResolved}. Errors: {ErrorCount}",
                        duration.TotalMilliseconds,
                        result.Statistics.TodoListsPulled,
                        result.Statistics.TodoListsPushed,
                        result.Statistics.TodoItemsPulled,
                        result.Statistics.TodoItemsPushed,
                        result.Statistics.ConflictsResolved,
                        result.Errors.Count);
                }
                else
                {
                    _logger.LogWarning(
                        "Scheduled synchronization completed with errors in {Duration}ms. " +
                        "Message: {Message}. Errors: {ErrorCount}",
                        duration.TotalMilliseconds,
                        result.Message,
                        result.Errors.Count);

                    // Log individual errors for debugging
                    foreach (var error in result.Errors.Take(5)) // Log first 5 errors to avoid spam
                    {
                        _logger.LogError(
                            "Sync error - Entity: {EntityType}, Operation: {Operation}, Message: {Message}",
                            error.EntityType, error.Operation, error.Message);
                    }

                    if (result.Errors.Count > 5)
                    {
                        _logger.LogWarning("... and {AdditionalErrors} more errors", result.Errors.Count - 5);
                    }
                }

                // Log conflicts for monitoring
                if (result.ResolvedConflicts.Any())
                {
                    _logger.LogInformation("Resolved {ConflictCount} conflicts during synchronization", 
                        result.ResolvedConflicts.Count);
                    
                    foreach (var conflict in result.ResolvedConflicts.Take(3)) // Log first 3 conflicts
                    {
                        _logger.LogInformation(
                            "Conflict resolved - Entity: {EntityType}, Description: {Description}, Resolution: {Resolution}",
                            conflict.EntityType, conflict.ConflictDescription, conflict.Resolution);
                    }
                }
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, 
                    "Scheduled synchronization failed after {Duration}ms with exception: {Message}",
                    duration.TotalMilliseconds, ex.Message);
            }
        }

        /// <summary>
        /// Called when the service is stopping. Allows for graceful shutdown.
        /// </summary>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Synchronization background service is stopping...");
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("Synchronization background service stopped gracefully");
        }
    }

    /// <summary>
    /// Configuration options for the synchronization background service.
    /// </summary>
    public class SynchronizationOptions
    {
        public const string SectionName = "Synchronization";

        /// <summary>
        /// Whether the background synchronization service is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Interval between synchronization runs in minutes.
        /// </summary>
        public int IntervalMinutes { get; set; } = 5;

        /// <summary>
        /// Initial delay before starting the first synchronization in minutes.
        /// Useful to allow the application to fully start up before beginning sync.
        /// </summary>
        public int InitialDelayMinutes { get; set; } = 1;

        /// <summary>
        /// Maximum number of consecutive failures before temporarily disabling sync.
        /// Set to 0 to disable this feature.
        /// </summary>
        public int MaxConsecutiveFailures { get; set; } = 0;

        /// <summary>
        /// Whether to run synchronization on application startup.
        /// </summary>
        public bool RunOnStartup { get; set; } = false;
    }
}