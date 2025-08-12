using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Infrastructure;

namespace TodoApi.Mediation.TodoListItem
{
     /// <summary>
    /// Handles the retrieval of a single TodoListItem.
    /// </summary>
    public class GetTodoListItemQueryHandler : IRequestHandler<GetTodoListItemQuery, ServiceResult<GetTodoListItemQueryResponse>>
    {
        private readonly TodoContext _dbContext;
        private readonly ILogger<GetTodoListItemQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetTodoListItemQueryHandler"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public GetTodoListItemQueryHandler(
            TodoContext dbContext,
            ILogger<GetTodoListItemQueryHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the get TodoListItem query.
        /// </summary>
        /// <param name="query">The query to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A service result containing the TodoListItem.</returns>
        public async Task<ServiceResult<GetTodoListItemQueryResponse>> Handle(
            GetTodoListItemQuery query, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving TodoListItem with ID: {Id} from TodoList: {TodoListId}, IncludeTodoList: {IncludeTodoList}", 
                    query.Id, query.TodoListId, query.IncludeTodoList);

                // Validate input
                if (query.TodoListId <= 0)
                {
                    _logger.LogWarning("Invalid TodoList ID provided: {TodoListId}", query.TodoListId);
                    return ServiceResult<GetTodoListItemQueryResponse>.Failure("Invalid TodoList ID", 400);
                }

                if (query.Id <= 0)
                {
                    _logger.LogWarning("Invalid TodoListItem ID provided: {Id}", query.Id);
                    return ServiceResult<GetTodoListItemQueryResponse>.Failure("Invalid TodoListItem ID", 400);
                }

                // Build the query with composite key
                IQueryable<Models.TodoListItem> queryable = _dbContext.TodoListItem
                    .Where(item => item.TodoListId == query.TodoListId && item.Id == query.Id);

                if (query.IncludeTodoList)
                {
                    queryable = queryable.Include(item => item.TodoList);
                }

                var todoListItem = await queryable
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (todoListItem == null)
                {
                    _logger.LogWarning("TodoListItem not found with ID: {Id} in TodoList: {TodoListId}", query.Id, query.TodoListId);
                    return ServiceResult<GetTodoListItemQueryResponse>.NotFound($"TodoListItem with ID {query.Id} not found in TodoList {query.TodoListId}");
                }

                var response = new GetTodoListItemQueryResponse
                {
                    Item = new Dtos.TodoListItemDetailResponseDto
                    {
                        Id = todoListItem.Id,
                        TodoListId = todoListItem.TodoListId,
                        Name = todoListItem.Name,
                        Description = todoListItem.Description,
                        Completed = todoListItem.Completed,
                        Progress = todoListItem.Progress,
                        TodoList = query.IncludeTodoList && todoListItem.TodoList != null ? new Dtos.TodoListSummaryDto
                        {
                            Id = todoListItem.TodoList.Id,
                            Name = todoListItem.TodoList.Name
                        } : null
                    }
                };

                _logger.LogInformation("Successfully retrieved TodoListItem with ID: {Id} from TodoList: {TodoListId}", query.Id, query.TodoListId);

                return ServiceResult<GetTodoListItemQueryResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving TodoListItem with ID: {Id} from TodoList: {TodoListId}", query.Id, query.TodoListId);
                return ServiceResult<GetTodoListItemQueryResponse>.Failure(
                    "An error occurred while retrieving the TodoListItem", 500, ex.Message);
            }
        }
    }
}