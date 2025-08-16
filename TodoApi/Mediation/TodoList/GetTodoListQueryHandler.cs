using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Infrastructure;
using TodoApi.Data;

namespace TodoApi.Mediation.TodoList
{
    /// <summary>
    /// Handles the retrieval of a single TodoList.
    /// </summary>
    public class GetTodoListQueryHandler : IRequestHandler<GetTodoListQuery, ServiceResult<GetTodoListQueryResponse>>
    {
        private readonly TodoContext _dbContext;
        private readonly ILogger<GetTodoListQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetTodoListQueryHandler"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public GetTodoListQueryHandler(
            TodoContext dbContext,
            ILogger<GetTodoListQueryHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the get TodoList query.
        /// </summary>
        /// <param name="query">The query to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A service result containing the TodoList.</returns>
        public async Task<ServiceResult<GetTodoListQueryResponse>> Handle(
            GetTodoListQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving TodoList with ID: {Id}, IncludeItems: {IncludeItems}",
                    query.Id, query.IncludeItems);

                if (query.Id <= 0)
                {
                    _logger.LogWarning("Invalid TodoList ID provided: {Id}", query.Id);
                    return ServiceResult<GetTodoListQueryResponse>.Failure("Invalid TodoList ID", 400);
                }

                IQueryable<Models.TodoList> queryable = _dbContext.TodoList;

                if (query.IncludeItems)
                {
                    queryable = queryable.Include(tl => tl.Items);
                }

                var todoList = await queryable
                    .FirstOrDefaultAsync(tl => tl.Id == query.Id, cancellationToken)
                    .ConfigureAwait(false);

                if (todoList == null)
                {
                    _logger.LogWarning("TodoList not found with ID: {Id}", query.Id);
                    return ServiceResult<GetTodoListQueryResponse>.NotFound($"TodoList with ID {query.Id} not found");
                }

                var response = new GetTodoListQueryResponse
                {
                    TodoList = new Dtos.TodoListResponseDto
                    {
                        Id = todoList.Id,
                        Name = todoList.Name,
                        Items = query.IncludeItems ? todoList.Items?.Select(item => new Dtos.TodoListItemResponseDto
                        {
                            Id = item.Id,
                            TodoListId = item.TodoListId,
                            Name = item.Name,
                            Description = item.Description,
                            Completed = item.Completed,
                            Progress = item.Progress
                        }).ToList() : null
                    }
                };

                _logger.LogInformation("Successfully retrieved TodoList with ID: {Id}", query.Id);

                return ServiceResult<GetTodoListQueryResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving TodoList with ID: {Id}", query.Id);
                return ServiceResult<GetTodoListQueryResponse>.Failure(
                    "An error occurred while retrieving the TodoList", 500, ex.Message);
            }
        }
    }
}