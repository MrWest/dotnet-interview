using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using TodoApi.ExternalApi.Models;
using TodoApi.ExternalApi.Models.Requests;

namespace TodoApi.ExternalApi
{
    /// <summary>
    /// HTTP client for communicating with the external Todo API.
    /// Implements resilience patterns using Polly for retry and circuit breaker functionality.
    /// </summary>
    public class ExternalTodoApiClient : IExternalTodoApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalTodoApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ExternalApiOptions _options;

        public ExternalTodoApiClient(
            HttpClient httpClient,
            ILogger<ExternalTodoApiClient> logger,
            IOptions<ExternalApiOptions> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            // Configure base address and default headers
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        /// <inheritdoc />
        public async Task<List<ExternalTodoList>> GetAllTodoListsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all TodoLists from external API");

            try
            {
                var response = await _httpClient.GetAsync("/todolists", cancellationToken);
                await EnsureSuccessStatusCodeAsync(response);

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var todoLists = JsonSerializer.Deserialize<List<ExternalTodoList>>(content, _jsonOptions) ?? new List<ExternalTodoList>();

                _logger.LogInformation("Successfully fetched {Count} TodoLists from external API", todoLists.Count);
                return todoLists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch TodoLists from external API");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<ExternalTodoList> CreateTodoListAsync(CreateTodoListRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating TodoList in external API with name: {Name}", request.Name);

            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/todolists", content, cancellationToken);
                await EnsureSuccessStatusCodeAsync(response);

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var createdList = JsonSerializer.Deserialize<ExternalTodoList>(responseContent, _jsonOptions);

                _logger.LogInformation("Successfully created TodoList in external API with ID: {ExternalId}", createdList?.Id);
                return createdList ?? throw new InvalidOperationException("Failed to deserialize created TodoList");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create TodoList in external API");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<ExternalTodoList> UpdateTodoListAsync(string externalId, UpdateTodoListRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating TodoList {ExternalId} in external API", externalId);

            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync($"/todolists/{externalId}", content, cancellationToken);
                await EnsureSuccessStatusCodeAsync(response);

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var updatedList = JsonSerializer.Deserialize<ExternalTodoList>(responseContent, _jsonOptions);

                _logger.LogInformation("Successfully updated TodoList {ExternalId} in external API", externalId);
                return updatedList ?? throw new InvalidOperationException("Failed to deserialize updated TodoList");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update TodoList {ExternalId} in external API", externalId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteTodoListAsync(string externalId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting TodoList {ExternalId} from external API", externalId);

            try
            {
                var response = await _httpClient.DeleteAsync($"/todolists/{externalId}", cancellationToken);
                await EnsureSuccessStatusCodeAsync(response);

                _logger.LogInformation("Successfully deleted TodoList {ExternalId} from external API", externalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete TodoList {ExternalId} from external API", externalId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<ExternalTodoItem> UpdateTodoItemAsync(string listExternalId, string itemExternalId, UpdateTodoItemRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating TodoItem {ItemExternalId} in TodoList {ListExternalId} via external API", itemExternalId, listExternalId);

            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync($"/todolists/{listExternalId}/todoitems/{itemExternalId}", content, cancellationToken);
                await EnsureSuccessStatusCodeAsync(response);

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var updatedItem = JsonSerializer.Deserialize<ExternalTodoItem>(responseContent, _jsonOptions);

                _logger.LogInformation("Successfully updated TodoItem {ItemExternalId} in external API", itemExternalId);
                return updatedItem ?? throw new InvalidOperationException("Failed to deserialize updated TodoItem");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update TodoItem {ItemExternalId} in external API", itemExternalId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteTodoItemAsync(string listExternalId, string itemExternalId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting TodoItem {ItemExternalId} from TodoList {ListExternalId} via external API", itemExternalId, listExternalId);

            try
            {
                var response = await _httpClient.DeleteAsync($"/todolists/{listExternalId}/todoitems/{itemExternalId}", cancellationToken);
                await EnsureSuccessStatusCodeAsync(response);

                _logger.LogInformation("Successfully deleted TodoItem {ItemExternalId} from external API", itemExternalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete TodoItem {ItemExternalId} from external API", itemExternalId);
                throw;
            }
        }

        /// <summary>
        /// Ensures the HTTP response indicates success, throwing detailed exceptions for failures.
        /// </summary>
        private async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            var content = await response.Content.ReadAsStringAsync();
            var errorMessage = $"External API request failed with status {response.StatusCode}: {content}";

            _logger.LogError("External API error: {StatusCode} - {Content}", response.StatusCode, content);

            throw response.StatusCode switch
            {
                HttpStatusCode.NotFound => new ExternalApiNotFoundException(errorMessage),
                HttpStatusCode.BadRequest => new ExternalApiBadRequestException(errorMessage),
                HttpStatusCode.Unauthorized => new ExternalApiUnauthorizedException(errorMessage),
                HttpStatusCode.TooManyRequests => new ExternalApiRateLimitException(errorMessage),
                _ => new ExternalApiException(errorMessage)
            };
        }
    }

    /// <summary>
    /// Configuration options for the external API client.
    /// </summary>
    public class ExternalApiOptions
    {
        public const string SectionName = "ExternalApi";

        /// <summary>
        /// Base URL of the external API.
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost";

        /// <summary>
        /// Timeout for HTTP requests in seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Maximum number of retry attempts.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Source ID to identify our local system in the external API.
        /// </summary>
        public string SourceId { get; set; } = "local-todo-api";
    }
}