using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Infrastructure;

namespace TodoApi.Mediation.TodoListItem
{
    /// <summary>
    /// Handles the updating of TodoListItem entities.
    /// </summary>
    public class UpdateTodoListItemCommandHandler : IRequestHandler<UpdateTodoListItemCommand, ServiceResult<UpdateTodoListItemCommandResponse>>
    {
        private readonly TodoContext _dbContext;
        private readonly ILogger<UpdateTodoListItemCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateTodoListItemCommandHandler"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public UpdateTodoListItemCommandHandler(
            TodoContext dbContext,
            ILogger<UpdateTodoListItemCommandHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the TodoListItem update command.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A service result containing the updated TodoListItem information.</returns>
        public async Task<ServiceResult<UpdateTodoListItemCommandResponse>> Handle(
            UpdateTodoListItemCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating TodoListItem with ID: {Id} in TodoList: {TodoListId}, new name: {Name}", 
                    command.Id, command.TodoListId, command.Name);

                // Validate input
                if (command.TodoListId <= 0)
                {
                    _logger.LogWarning("Invalid TodoList ID provided: {TodoListId}", command.TodoListId);
                    return ServiceResult<UpdateTodoListItemCommandResponse>.Failure("Invalid TodoList ID", 400);
                }

                if (command.Id <= 0)
                {
                    _logger.LogWarning("Invalid TodoListItem ID provided: {Id}", command.Id);
                    return ServiceResult<UpdateTodoListItemCommandResponse>.Failure("Invalid TodoListItem ID", 400);
                }

                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    _logger.LogWarning("TodoListItem update failed: Name is required");
                    return ServiceResult<UpdateTodoListItemCommandResponse>.Failure("Name is required", 400);
                }

                if (command.Name.Length > 500)
                {
                    _logger.LogWarning("TodoListItem update failed: Name exceeds maximum length");
                    return ServiceResult<UpdateTodoListItemCommandResponse>.Failure(
                        "Name cannot exceed 500 characters", 400);
                }

                if (!string.IsNullOrEmpty(command.Description) && command.Description.Length > 2000)
                {
                    _logger.LogWarning("TodoListItem update failed: Description exceeds maximum length");
                    return ServiceResult<UpdateTodoListItemCommandResponse>.Failure(
                        "Description cannot exceed 2000 characters", 400);
                }

                if (command.Progress < 0 || command.Progress > 100)
                {
                    _logger.LogWarning("TodoListItem update failed: Progress must be between 0 and 100");
                    return ServiceResult<UpdateTodoListItemCommandResponse>.Failure(
                        "Progress must be between 0 and 100", 400);
                }

                // Find the existing TodoListItem using composite key
                var todoListItem = await _dbContext.TodoListItem
                    .FirstOrDefaultAsync(item => item.TodoListId == command.TodoListId && item.Id == command.Id, cancellationToken)
                    .ConfigureAwait(false);

                if (todoListItem == null)
                {
                    _logger.LogWarning("TodoListItem not found with ID: {Id} in TodoList: {TodoListId}", command.Id, command.TodoListId);
                    return ServiceResult<UpdateTodoListItemCommandResponse>.NotFound($"TodoListItem with ID {command.Id} not found in TodoList {command.TodoListId}");
                }

                // Update the TodoListItem
                todoListItem.Name = command.Name.Trim();
                todoListItem.Description = command.Description?.Trim() ?? string.Empty;
                todoListItem.Completed = command.Completed;
                todoListItem.Progress = command.Progress;

                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Successfully updated TodoListItem with ID: {Id} in TodoList: {TodoListId}", command.Id, command.TodoListId);

                // Create response
                var response = new UpdateTodoListItemCommandResponse
                {
                    Id = todoListItem.Id,
                    TodoListId = todoListItem.TodoListId,
                    Name = todoListItem.Name,
                    Description = todoListItem.Description,
                    Completed = todoListItem.Completed,
                    Progress = todoListItem.Progress,
                    UpdatedAt = DateTime.UtcNow
                };

                return ServiceResult<UpdateTodoListItemCommandResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating TodoListItem with ID: {Id} in TodoList: {TodoListId}", command.Id, command.TodoListId);
                return ServiceResult<UpdateTodoListItemCommandResponse>.Failure(
                    "An error occurred while updating the TodoListItem", 500, ex.Message);
            }
        }
    }
}