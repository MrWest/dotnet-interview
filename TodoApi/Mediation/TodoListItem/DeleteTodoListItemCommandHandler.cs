using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Infrastructure;

namespace TodoApi.Mediation.TodoListItem
{
    /// <summary>
    /// Handles the deletion of TodoListItem entities.
    /// </summary>
    public class DeleteTodoListItemCommandHandler : IRequestHandler<DeleteTodoListItemCommand, ServiceResult<DeleteTodoListItemCommandResponse>>
    {
        private readonly TodoContext _dbContext;
        private readonly ILogger<DeleteTodoListItemCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteTodoListItemCommandHandler"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public DeleteTodoListItemCommandHandler(
            TodoContext dbContext,
            ILogger<DeleteTodoListItemCommandHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the TodoListItem deletion command.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A service result containing the deletion confirmation.</returns>
        public async Task<ServiceResult<DeleteTodoListItemCommandResponse>> Handle(
            DeleteTodoListItemCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting TodoListItem with ID: {Id} from TodoList: {TodoListId}", command.Id, command.TodoListId);

                // Validate input
                if (command.TodoListId <= 0)
                {
                    _logger.LogWarning("Invalid TodoList ID provided: {TodoListId}", command.TodoListId);
                    return ServiceResult<DeleteTodoListItemCommandResponse>.Failure("Invalid TodoList ID", 400);
                }

                if (command.Id <= 0)
                {
                    _logger.LogWarning("Invalid TodoListItem ID provided: {Id}", command.Id);
                    return ServiceResult<DeleteTodoListItemCommandResponse>.Failure("Invalid TodoListItem ID", 400);
                }

                // Find the existing TodoListItem using composite key
                var todoListItem = await _dbContext.TodoListItem
                    .FirstOrDefaultAsync(item => item.TodoListId == command.TodoListId && item.Id == command.Id, cancellationToken)
                    .ConfigureAwait(false);

                if (todoListItem == null)
                {
                    _logger.LogWarning("TodoListItem not found with ID: {Id} in TodoList: {TodoListId}", command.Id, command.TodoListId);
                    return ServiceResult<DeleteTodoListItemCommandResponse>.NotFound($"TodoListItem with ID {command.Id} not found in TodoList {command.TodoListId}");
                }

                // Store the name for the response
                var deletedItemName = todoListItem.Name;

                // Delete the TodoListItem
                _dbContext.TodoListItem.Remove(todoListItem);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Successfully deleted TodoListItem with ID: {Id} from TodoList: {TodoListId}", command.Id, command.TodoListId);

                // Create response
                var response = new DeleteTodoListItemCommandResponse
                {
                    Id = command.Id,
                    TodoListId = command.TodoListId,
                    Message = "TodoListItem deleted successfully",
                    DeletedAt = DateTime.UtcNow,
                    DeletedItemName = deletedItemName
                };

                return ServiceResult<DeleteTodoListItemCommandResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting TodoListItem with ID: {Id} from TodoList: {TodoListId}", command.Id, command.TodoListId);
                return ServiceResult<DeleteTodoListItemCommandResponse>.Failure(
                    "An error occurred while deleting the TodoListItem", 500, ex.Message);
            }
        }
    }
}