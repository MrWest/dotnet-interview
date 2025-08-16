using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

namespace TodoApi.Mediation.TodoList
{
    /// <summary>
    /// Handles the creation of TodoList entities.
    /// </summary>
    public class TodoListCreateCommandHandler : IRequestHandler<TodoListCreateCommand, TodoApi.Infrastructure.ServiceResult<TodoListCreateCommandResponse>>
    {
        private readonly TodoContext _dbContext;
        private readonly ILogger<TodoListCreateCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TodoListCreateCommandHandler"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public TodoListCreateCommandHandler(
            TodoContext dbContext,
            ILogger<TodoListCreateCommandHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the TodoList creation command.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A service result containing the created TodoList information.</returns>
        public async Task<TodoApi.Infrastructure.ServiceResult<TodoListCreateCommandResponse>> Handle(
            TodoListCreateCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new TodoList with name: {Name}", command.Name);

                // Validate input
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    _logger.LogWarning("TodoList creation failed: Name is required");
                    return TodoApi.Infrastructure.ServiceResult<TodoListCreateCommandResponse>.Failure(
                        "Name is required", 400);
                }

                if (command.Name.Length > 200)
                {
                    _logger.LogWarning("TodoList creation failed: Name exceeds maximum length");
                    return TodoApi.Infrastructure.ServiceResult<TodoListCreateCommandResponse>.Failure(
                        "Name cannot exceed 200 characters", 400);
                }

                // Check for duplicate names (optional business rule)
                var existingTodoList = await _dbContext.TodoList
                    .FirstOrDefaultAsync(tl => tl.Name.ToLower() == command.Name.ToLower(), cancellationToken)
                    .ConfigureAwait(false);

                if (existingTodoList != null)
                {
                    _logger.LogWarning("TodoList creation failed: Name already exists - {Name}", command.Name);
                    return TodoApi.Infrastructure.ServiceResult<TodoListCreateCommandResponse>.Failure(
                        "A TodoList with this name already exists", 409);
                }

                // Create the new TodoList
                var todoList = new Models.TodoList
                {
                    Name = command.Name.Trim()
                };

                _dbContext.TodoList.Add(todoList);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Successfully created TodoList with ID: {Id}", todoList.Id);

                // Create response
                var response = new TodoListCreateCommandResponse
                {
                    Id = todoList.Id,
                    Name = todoList.Name,
                    CreatedAt = DateTime.UtcNow
                };

                return TodoApi.Infrastructure.ServiceResult<TodoListCreateCommandResponse>.Created(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating TodoList with name: {Name}", command.Name);
                return TodoApi.Infrastructure.ServiceResult<TodoListCreateCommandResponse>.Failure(
                    "An error occurred while creating the TodoList", 500, ex.Message);
            }
        }
    }
}