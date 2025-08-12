using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Infrastructure;
using TodoApi.Mediation.TodoListItem;
using TodoApi.Mediation.TodoListItem.Dtos;

namespace TodoApi.Controllers
{
    /// <summary>
    /// Controller for managing TodoListItem operations using CQRS pattern.
    /// Handles all CRUD operations for items within specific TodoLists.
    /// </summary>
    [Route("api/todolists/{todoListId}/items")]
    [ApiController]
    public class TodoListItemController : BaseController
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TodoListItemController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator instance for handling commands and queries.</param>
        public TodoListItemController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Gets all TodoListItems for a specific TodoList with optional filtering and pagination.
        /// </summary>
        /// <param name="todoListId">The ID of the parent TodoList.</param>
        /// <param name="includeTodoList">Optional parameter to include parent TodoList information.</param>
        /// <param name="completedFilter">Optional filter for completion status (null = all, true = completed, false = incomplete).</param>
        /// <param name="page">Page number for pagination (default: 1).</param>
        /// <param name="pageSize">Page size for pagination (default: 50, max: 100).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paginated list of TodoListItems for the specified TodoList.</returns>
        [HttpGet]
        public async Task<ActionResult<GetAllTodoListItemsQueryResponse>> GetTodoListItems(
            [FromRoute] long todoListId,
            [FromQuery] bool includeTodoList = false,
            [FromQuery] bool? completedFilter = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAllTodoListItemsQuery
            {
                TodoListId = todoListId,
                IncludeTodoList = includeTodoList,
                CompletedFilter = completedFilter,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, cancellationToken);
            return ServiceResult(result);
        }

        /// <summary>
        /// Gets a specific TodoListItem by composite key (TodoListId + ItemId).
        /// </summary>
        /// <param name="todoListId">The ID of the parent TodoList.</param>
        /// <param name="id">The ID of the TodoListItem to retrieve.</param>
        /// <param name="includeTodoList">Optional parameter to include parent TodoList information.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The TodoListItem with the specified composite key.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<GetTodoListItemQueryResponse>> GetTodoListItem(
            [FromRoute] long todoListId,
            [FromRoute] long id,
            [FromQuery] bool includeTodoList = false,
            CancellationToken cancellationToken = default)
        {
            var query = new GetTodoListItemQuery
            {
                TodoListId = todoListId,
                Id = id,
                IncludeTodoList = includeTodoList
            };

            var result = await _mediator.Send(query, cancellationToken);
            return ServiceResult(result);
        }

        /// <summary>
        /// Updates an existing TodoListItem with comprehensive field support.
        /// </summary>
        /// <param name="todoListId">The ID of the parent TodoList.</param>
        /// <param name="id">The ID of the TodoListItem to update.</param>
        /// <param name="dto">The update data transfer object containing all updatable fields.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated TodoListItem information with timestamp.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<UpdateTodoListItemCommandResponse>> PutTodoListItem(
            [FromRoute] long todoListId,
            [FromRoute] long id,
            [FromBody] UpdateTodoListItemDto dto,
            CancellationToken cancellationToken = default)
        {
            var command = new UpdateTodoListItemCommand
            {
                TodoListId = todoListId,
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                Completed = dto.Completed,
                Progress = dto.Progress
            };

            var result = await _mediator.Send(command, cancellationToken);
            return ServiceResult(result);
        }

        /// <summary>
        /// Creates a new TodoListItem within the specified TodoList.
        /// </summary>
        /// <param name="todoListId">The ID of the parent TodoList.</param>
        /// <param name="dto">The creation data transfer object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created TodoListItem information with proper location header.</returns>
        [HttpPost]
        public async Task<ActionResult<TodoListItemCreateCommandResponse>> PostTodoListItem(
            [FromRoute] long todoListId,
            [FromBody] CreateTodoListItemDto dto,
            CancellationToken cancellationToken = default)
        {
            var command = new TodoListItemCreateCommand
            {
                TodoListId = todoListId,
                Name = dto.Name,
                Description = dto.Description,
                Completed = dto.Completed,
                Progress = dto.Progress
            };

            var result = await _mediator.Send(command, cancellationToken);
            
            // Return CreatedAtAction with proper location header for RESTful compliance
            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetTodoListItem),
                    new { todoListId = todoListId, id = result.Data.Id },
                    result.Data);
            }

            return ServiceResult(result);
        }

        /// <summary>
        /// Deletes a TodoListItem using composite key validation.
        /// </summary>
        /// <param name="todoListId">The ID of the parent TodoList.</param>
        /// <param name="id">The ID of the TodoListItem to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Confirmation of the deletion with audit information.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<DeleteTodoListItemCommandResponse>> DeleteTodoListItem(
            [FromRoute] long todoListId,
            [FromRoute] long id,
            CancellationToken cancellationToken = default)
        {
            var command = new DeleteTodoListItemCommand
            {
                TodoListId = todoListId,
                Id = id
            };

            var result = await _mediator.Send(command, cancellationToken);
            return ServiceResult(result);
        }
    }
}
