using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Infrastructure;
using TodoApi.Mediation.TodoList;
using TodoApi.Mediation.TodoList.Dtos;

namespace TodoApi.Controllers
{
    /// <summary>
    /// Controller for managing TodoList operations using CQRS pattern.
    /// </summary>
    [Route("api/todolists")]
    [ApiController]
    public class TodoListsController : BaseController
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TodoListsController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator instance for handling commands and queries.</param>
        public TodoListsController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Gets all TodoLists.
        /// </summary>
        /// <param name="includeItems">Optional parameter to include TodoListItems in the response.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of all TodoLists.</returns>
        [HttpGet]
        public async Task<ActionResult<GetAllTodoListsQueryResponse>> GetTodoLists(
            [FromQuery] bool includeItems = false,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAllTodoListsQuery
            {
                IncludeItems = includeItems
            };

            var result = await _mediator.Send(query, cancellationToken);
            return ServiceResult(result);
        }

        /// <summary>
        /// Gets a specific TodoList by ID.
        /// </summary>
        /// <param name="id">The ID of the TodoList to retrieve.</param>
        /// <param name="includeItems">Optional parameter to include TodoListItems in the response.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The TodoList with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<GetTodoListQueryResponse>> GetTodoList(
            long id,
            [FromQuery] bool includeItems = false,
            CancellationToken cancellationToken = default)
        {
            var query = new GetTodoListQuery
            {
                Id = id,
                IncludeItems = includeItems
            };

            var result = await _mediator.Send(query, cancellationToken);
            return ServiceResult(result);
        }

        /// <summary>
        /// Updates an existing TodoList.
        /// </summary>
        /// <param name="id">The ID of the TodoList to update.</param>
        /// <param name="dto">The update data transfer object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated TodoList information.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<UpdateTodoListCommandResponse>> PutTodoList(
            long id,
            [FromBody] UpdateTodoListDto dto,
            CancellationToken cancellationToken = default)
        {
            var command = new UpdateTodoListCommand
            {
                Id = id,
                Name = dto.Name
            };

            var result = await _mediator.Send(command, cancellationToken);
            return ServiceResult(result);
        }

        /// <summary>
        /// Creates a new TodoList.
        /// </summary>
        /// <param name="dto">The creation data transfer object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created TodoList information.</returns>
        [HttpPost]
        public async Task<ActionResult<TodoListCreateCommandResponse>> PostTodoList(
            [FromBody] CreateTodoListDto dto,
            CancellationToken cancellationToken = default)
        {
            var command = new TodoListCreateCommand
            {
                Name = dto.Name
            };

            var result = await _mediator.Send(command, cancellationToken);
            return ServiceResult(result);
        }

        /// <summary>
        /// Deletes a TodoList.
        /// </summary>
        /// <param name="id">The ID of the TodoList to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Confirmation of the deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<DeleteTodoListCommandResponse>> DeleteTodoList(
            long id,
            CancellationToken cancellationToken = default)
        {
            var command = new DeleteTodoListCommand
            {
                Id = id
            };

            var result = await _mediator.Send(command, cancellationToken);
            return ServiceResult(result);
        }
    }
}
