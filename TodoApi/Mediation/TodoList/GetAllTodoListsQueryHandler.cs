
using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Infrastructure;
using TodoApi.Data;

namespace TodoApi.Mediation.TodoList
{ /// <summary>
  /// Handles the retrieval of all TodoLists.
  /// </summary>
    public class GetAllTodoListsQueryHandler : IRequestHandler<GetAllTodoListsQuery, ServiceResult<GetAllTodoListsQueryResponse>>
    {
        private readonly TodoContext _dbContext;
        private readonly ILogger<GetAllTodoListsQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllTodoListsQueryHandler"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public GetAllTodoListsQueryHandler(
            TodoContext dbContext,
            ILogger<GetAllTodoListsQueryHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the get all TodoLists query.
        /// </summary>
        /// <param name="query">The query to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A service result containing all TodoLists.</returns>
        public async Task<ServiceResult<GetAllTodoListsQueryResponse>> Handle(
            GetAllTodoListsQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving all TodoLists with IncludeItems: {IncludeItems}", query.IncludeItems);

                IQueryable<Models.TodoList> queryable = _dbContext.TodoList;

                if (query.IncludeItems)
                {
                    queryable = queryable.Include(tl => tl.Items);
                }

                var todoLists = await queryable
                    .OrderBy(tl => tl.Name)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                var response = new GetAllTodoListsQueryResponse
                {
                    TotalCount = todoLists.Count,
                    TodoLists = todoLists.Select(tl => new Dtos.TodoListResponseDto
                    {
                        Id = tl.Id,
                        Name = tl.Name,
                        Items = query.IncludeItems ? tl.Items?.Select(item => new Dtos.TodoListItemResponseDto
                        {
                            Id = item.Id,
                            TodoListId = item.TodoListId,
                            Name = item.Name,
                            Description = item.Description,
                            Completed = item.Completed,
                            Progress = item.Progress
                        }).ToList() : null
                    }).ToList()
                };

                _logger.LogInformation("Successfully retrieved {Count} TodoLists", response.TotalCount);

                return ServiceResult<GetAllTodoListsQueryResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all TodoLists");
                return ServiceResult<GetAllTodoListsQueryResponse>.Failure(
                    "An error occurred while retrieving TodoLists", 500, ex.Message);
            }
        }
    }
}