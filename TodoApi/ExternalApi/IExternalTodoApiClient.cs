using TodoApi.ExternalApi.Models;
using TodoApi.ExternalApi.Models.Requests;

namespace TodoApi.ExternalApi
{
    /// <summary>
    /// Interface for communicating with the external Todo API.
    /// Provides methods for all CRUD operations supported by the external system.
    /// </summary>
    public interface IExternalTodoApiClient
    {
        /// <summary>
        /// Retrieves all TodoLists and their associated items from the external API.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>List of all TodoLists with their items.</returns>
        Task<List<ExternalTodoList>> GetAllTodoListsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new TodoList in the external API.
        /// </summary>
        /// <param name="request">The creation request containing list details and optional items.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>The created TodoList as returned by the external API.</returns>
        Task<ExternalTodoList> CreateTodoListAsync(CreateTodoListRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing TodoList in the external API.
        /// </summary>
        /// <param name="externalId">The external ID of the TodoList to update.</param>
        /// <param name="request">The update request containing new values.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>The updated TodoList as returned by the external API.</returns>
        Task<ExternalTodoList> UpdateTodoListAsync(long externalId, UpdateTodoListRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a TodoList and all its items from the external API.
        /// </summary>
        /// <param name="externalId">The external ID of the TodoList to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        Task DeleteTodoListAsync(long externalId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing TodoItem in the external API.
        /// </summary>
        /// <param name="listExternalId">The external ID of the parent TodoList.</param>
        /// <param name="itemExternalId">The external ID of the TodoItem to update.</param>
        /// <param name="request">The update request containing new values.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>The updated TodoItem as returned by the external API.</returns>
        Task<ExternalTodoItem> UpdateTodoItemAsync(long listExternalId, long itemExternalId, UpdateTodoItemRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a TodoItem from the external API.
        /// </summary>
        /// <param name="listExternalId">The external ID of the parent TodoList.</param>
        /// <param name="itemExternalId">The external ID of the TodoItem to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        Task DeleteTodoItemAsync(long listExternalId, long itemExternalId, CancellationToken cancellationToken = default);
    }
}