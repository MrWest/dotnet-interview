using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Infrastructure;

namespace TodoApi.Mediation.TodoList
{
     /// <summary>
    /// Handles the deletion of TodoList entities.
    /// </summary>
    public class DeleteTodoListCommandHandler : IRequestHandler<DeleteTodoListCommand, ServiceResult<DeleteTodoListCommandResponse>>
    {
        private readonly TodoContext _dbContext;
        private readonly ILogger<DeleteTodoListCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteTodoListCommandHandler"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public DeleteTodoListCommandHandler(
            TodoContext dbContext,
            ILogger<DeleteTodoListCommandHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the TodoList deletion command.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A service result containing the deletion confirmation.</returns>
        public async Task<ServiceResult<DeleteTodoListCommandResponse>> Handle(
            DeleteTodoListCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting TodoList with ID: {Id}", command.Id);

                // Validate input
                if (command.Id <= 0)
                {
                    _logger.LogWarning("Invalid TodoList ID provided: {Id}", command.Id);
                    return ServiceResult<DeleteTodoListCommandResponse>.Failure("Invalid TodoList ID", 400);
                }

                // Find the existing TodoList with its items
                var todoList = await _dbContext.TodoList
                    .Include(tl => tl.Items)
                    .FirstOrDefaultAsync(tl => tl.Id == command.Id, cancellationToken)
                    .ConfigureAwait(false);

                if (todoList == null)
                {
                    _logger.LogWarning("TodoList not found with ID: {Id}", command.Id);
                    return ServiceResult<DeleteTodoListCommandResponse>.NotFound($"TodoList with ID {command.Id} not found");
                }

                // Count items that will be deleted (for response)
                var itemsCount = todoList.Items?.Count ?? 0;

                // Delete the TodoList (cascade delete will handle items due to OnDelete(DeleteBehavior.Cascade))
                _dbContext.TodoList.Remove(todoList);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Successfully deleted TodoList with ID: {Id} and {ItemsCount} related items", 
                    command.Id, itemsCount);

                // Create response
                var response = new DeleteTodoListCommandResponse
                {
                    Id = command.Id,
                    Message = "TodoList deleted successfully",
                    DeletedAt = DateTime.UtcNow,
                    DeletedItemsCount = itemsCount
                };

                return ServiceResult<DeleteTodoListCommandResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting TodoList with ID: {Id}", command.Id);
                return ServiceResult<DeleteTodoListCommandResponse>.Failure(
                    "An error occurred while deleting the TodoList", 500, ex.Message);
            }
        }
    }
}