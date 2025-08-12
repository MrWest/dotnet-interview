using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Infrastructure;

namespace TodoApi.Mediation.TodoList
{
    /// <summary>
    /// Handles the updating of TodoList entities.
    /// </summary>
    public class UpdateTodoListCommandHandler : IRequestHandler<UpdateTodoListCommand, ServiceResult<UpdateTodoListCommandResponse>>
    {
        private readonly TodoContext _dbContext;
        private readonly ILogger<UpdateTodoListCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateTodoListCommandHandler"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public UpdateTodoListCommandHandler(
            TodoContext dbContext,
            ILogger<UpdateTodoListCommandHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the TodoList update command.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A service result containing the updated TodoList information.</returns>
        public async Task<ServiceResult<UpdateTodoListCommandResponse>> Handle(
            UpdateTodoListCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating TodoList with ID: {Id}, new name: {Name}", command.Id, command.Name);

                // Validate input
                if (command.Id <= 0)
                {
                    _logger.LogWarning("Invalid TodoList ID provided: {Id}", command.Id);
                    return ServiceResult<UpdateTodoListCommandResponse>.Failure("Invalid TodoList ID", 400);
                }

                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    _logger.LogWarning("TodoList update failed: Name is required");
                    return ServiceResult<UpdateTodoListCommandResponse>.Failure("Name is required", 400);
                }

                if (command.Name.Length > 200)
                {
                    _logger.LogWarning("TodoList update failed: Name exceeds maximum length");
                    return ServiceResult<UpdateTodoListCommandResponse>.Failure(
                        "Name cannot exceed 200 characters", 400);
                }

                // Find the existing TodoList
                var todoList = await _dbContext.TodoList
                    .FirstOrDefaultAsync(tl => tl.Id == command.Id, cancellationToken)
                    .ConfigureAwait(false);

                if (todoList == null)
                {
                    _logger.LogWarning("TodoList not found with ID: {Id}", command.Id);
                    return ServiceResult<UpdateTodoListCommandResponse>.NotFound($"TodoList with ID {command.Id} not found");
                }

                // Check for duplicate names (excluding current record)
                var existingTodoList = await _dbContext.TodoList
                    .FirstOrDefaultAsync(tl => tl.Name.ToLower() == command.Name.ToLower() && tl.Id != command.Id, cancellationToken)
                    .ConfigureAwait(false);

                if (existingTodoList != null)
                {
                    _logger.LogWarning("TodoList update failed: Name already exists - {Name}", command.Name);
                    return ServiceResult<UpdateTodoListCommandResponse>.Failure(
                        "A TodoList with this name already exists", 409);
                }

                // Update the TodoList
                todoList.Name = command.Name.Trim();
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Successfully updated TodoList with ID: {Id}", todoList.Id);

                // Create response
                var response = new UpdateTodoListCommandResponse
                {
                    Id = todoList.Id,
                    Name = todoList.Name,
                    UpdatedAt = DateTime.UtcNow
                };

                return ServiceResult<UpdateTodoListCommandResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating TodoList with ID: {Id}", command.Id);
                return ServiceResult<UpdateTodoListCommandResponse>.Failure(
                    "An error occurred while updating the TodoList", 500, ex.Message);
            }
        }
    }
}