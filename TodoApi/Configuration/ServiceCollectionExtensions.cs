using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using TodoApi.BackgroundServices;
using TodoApi.ExternalApi;
using TodoApi.Synchronization;

namespace TodoApi.Configuration
{
    /// <summary>
    /// Extension methods for configuring synchronization services in the DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all synchronization-related services to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddSynchronizationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure options from appsettings.json
            services.Configure<ExternalApiOptions>(
                configuration.GetSection(ExternalApiOptions.SectionName));
            
            services.Configure<SynchronizationOptions>(
                configuration.GetSection(SynchronizationOptions.SectionName));

            // Add HTTP client with Polly resilience policies
            services.AddHttpClient<IExternalTodoApiClient, ExternalTodoApiClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<ExternalApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

            // Register synchronization services
            services.AddScoped<ISynchronizationService, SynchronizationService>();
            
            // Add background service for automatic synchronization
            services.AddHostedService<SynchronizationBackgroundService>();

            return services;
        }

        /// <summary>
        /// Creates a retry policy for HTTP requests with exponential backoff.
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // Handles HttpRequestException and 5XX, 408 status codes
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Handle rate limiting
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        Console.WriteLine($"HTTP request failed (attempt {retryCount}), retrying in {timespan.TotalMilliseconds}ms");
                    });
        }

        /// <summary>
        /// Creates a circuit breaker policy for HTTP requests.
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5, // Open circuit after 5 consecutive failures
                    durationOfBreak: TimeSpan.FromSeconds(30), // Keep circuit open for 30 seconds
                    onBreak: (exception, duration) =>
                    {
                        Console.WriteLine($"Circuit breaker opened for {duration.TotalSeconds} seconds");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("Circuit breaker closed, requests will be allowed through");
                    });
        }
    }
}