
using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

namespace TodoApi.Mediation.TodoListItem
{
    /// <summary>
    /// Handles the creation of TodoListItem entities.
    /// </summary>
    public class TodoListItemCreateCommandHandler : IRequestHandler<TodoListItemCreateCommand, TodoApi.Infrastructure.ServiceResult<TodoListItemCreateCommandResponse>>
    {
        private readonly TodoContext _dbContext;
        private readonly ILogger<TodoListItemCreateCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TodoListItemCreateCommandHandler"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public TodoListItemCreateCommandHandler(
            TodoContext dbContext,
            ILogger<TodoListItemCreateCommandHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the TodoListItem creation command.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A service result containing the created TodoListItem information.</returns>
        public async Task<TodoApi.Infrastructure.ServiceResult<TodoListItemCreateCommandResponse>> Handle(
            TodoListItemCreateCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new TodoListItem with name: {Name} for TodoList: {TodoListId}",
                    command.Name, command.TodoListId);

                // Validate input
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    _logger.LogWarning("TodoListItem creation failed: Name is required");
                    return TodoApi.Infrastructure.ServiceResult<TodoListItemCreateCommandResponse>.Failure(
                        "Name is required", 400);
                }

                if (command.Name.Length > 500)
                {
                    _logger.LogWarning("TodoListItem creation failed: Name exceeds maximum length");
                    return TodoApi.Infrastructure.ServiceResult<TodoListItemCreateCommandResponse>.Failure(
                        "Name cannot exceed 500 characters", 400);
                }

                if (!string.IsNullOrEmpty(command.Description) && command.Description.Length > 2000)
                {
                    _logger.LogWarning("TodoListItem creation failed: Description exceeds maximum length");
                    return TodoApi.Infrastructure.ServiceResult<TodoListItemCreateCommandResponse>.Failure(
                        "Description cannot exceed 2000 characters", 400);
                }

                if (command.Progress < 0 || command.Progress > 100)
                {
                    _logger.LogWarning("TodoListItem creation failed: Progress must be between 0 and 100");
                    return TodoApi.Infrastructure.ServiceResult<TodoListItemCreateCommandResponse>.Failure(
                        "Progress must be between 0 and 100", 400);
                }

                // Verify the parent TodoList exists
                var parentTodoList = await _dbContext.TodoList
                    .FirstOrDefaultAsync(tl => tl.Id == command.TodoListId, cancellationToken)
                    .ConfigureAwait(false);

                if (parentTodoList == null)
                {
                    _logger.LogWarning("TodoListItem creation failed: Parent TodoList not found - {TodoListId}",
                        command.TodoListId);
                    return TodoApi.Infrastructure.ServiceResult<TodoListItemCreateCommandResponse>.NotFound(
                        $"TodoList with ID {command.TodoListId} not found");
                }

                // // Generate the next available ID for this TodoList (FIXED VERSION)
                // var existingIds = await _dbContext.TodoListItem
                //     .Where(item => item.TodoListId == command.TodoListId)
                //     .Select(item => item.Id)
                //     .ToListAsync(cancellationToken)
                //     .ConfigureAwait(false);

                // var maxId = existingIds.Any() ? existingIds.Max() : 0;


                // Create the new TodoListItem
                var todoListItem = new Models.TodoListItem
                {
                    // Id = maxId + 1,
                    TodoListId = command.TodoListId,
                    Name = command.Name.Trim(),
                    Description = command.Description?.Trim() ?? string.Empty,
                    Completed = command.Completed,
                    Progress = command.Progress
                };

                _dbContext.TodoListItem.Add(todoListItem);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Successfully created TodoListItem with ID: {Id} for TodoList: {TodoListId}",
                    todoListItem.Id, todoListItem.TodoListId);

                // Create response
                var response = new TodoListItemCreateCommandResponse
                {
                    Id = todoListItem.Id,
                    TodoListId = todoListItem.TodoListId,
                    Name = todoListItem.Name,
                    Description = todoListItem.Description,
                    Completed = todoListItem.Completed,
                    Progress = todoListItem.Progress,
                    CreatedAt = DateTime.UtcNow
                };

                return TodoApi.Infrastructure.ServiceResult<TodoListItemCreateCommandResponse>.Created(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating TodoListItem with name: {Name} for TodoList: {TodoListId}",
                    command.Name, command.TodoListId);
                return TodoApi.Infrastructure.ServiceResult<TodoListItemCreateCommandResponse>.Failure(
                    "An error occurred while creating the TodoListItem", 500, ex.Message);
            }
        }
    }
}