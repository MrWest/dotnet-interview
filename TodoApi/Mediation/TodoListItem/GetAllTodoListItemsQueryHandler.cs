

using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Infrastructure;

namespace TodoApi.Mediation.TodoListItem
{
    /// <summary>
    /// Handles the retrieval of all TodoListItems for a specific TodoList.
    /// </summary>
    public class GetAllTodoListItemsQueryHandler : IRequestHandler<GetAllTodoListItemsQuery, ServiceResult<GetAllTodoListItemsQueryResponse>>
    {
        private readonly TodoContext _dbContext;
        private readonly ILogger<GetAllTodoListItemsQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllTodoListItemsQueryHandler"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public GetAllTodoListItemsQueryHandler(
            TodoContext dbContext,
            ILogger<GetAllTodoListItemsQueryHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the get all TodoListItems query.
        /// </summary>
        /// <param name="query">The query to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A service result containing all TodoListItems for the specified TodoList.</returns>
        public async Task<ServiceResult<GetAllTodoListItemsQueryResponse>> Handle(
            GetAllTodoListItemsQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving TodoListItems for TodoList: {TodoListId}, Page: {Page}, PageSize: {PageSize}, CompletedFilter: {CompletedFilter}",
                    query.TodoListId, query.Page, query.PageSize, query.CompletedFilter);

                // Validate input
                if (query.TodoListId <= 0)
                {
                    _logger.LogWarning("Invalid TodoList ID provided: {TodoListId}", query.TodoListId);
                    return ServiceResult<GetAllTodoListItemsQueryResponse>.Failure("Invalid TodoList ID", 400);
                }

                if (query.Page <= 0)
                {
                    query.Page = 1;
                }

                if (query.PageSize <= 0 || query.PageSize > 100)
                {
                    query.PageSize = 50;
                }

                // Verify the parent TodoList exists
                var parentTodoList = await _dbContext.TodoList
                    .FirstOrDefaultAsync(tl => tl.Id == query.TodoListId, cancellationToken)
                    .ConfigureAwait(false);

                if (parentTodoList == null)
                {
                    _logger.LogWarning("Parent TodoList not found: {TodoListId}", query.TodoListId);
                    return ServiceResult<GetAllTodoListItemsQueryResponse>.NotFound($"TodoList with ID {query.TodoListId} not found");
                }

                // Build the query
                IQueryable<Models.TodoListItem> queryable = _dbContext.TodoListItem
                    .Where(item => item.TodoListId == query.TodoListId);

                if (query.IncludeTodoList)
                {
                    queryable = queryable.Include(item => item.TodoList);
                }

                // Apply completion filter
                if (query.CompletedFilter.HasValue)
                {
                    queryable = queryable.Where(item => item.Completed == query.CompletedFilter.Value);
                }

                // Get total count before pagination
                var totalCount = await queryable.CountAsync(cancellationToken).ConfigureAwait(false);

                // Apply pagination and ordering
                var items = await queryable
                    .OrderBy(item => item.Id)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

                var response = new GetAllTodoListItemsQueryResponse
                {
                    TodoListId = query.TodoListId,
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = totalPages,
                    TodoList = query.IncludeTodoList ? new Dtos.TodoListSummaryDto
                    {
                        Id = parentTodoList.Id,
                        Name = parentTodoList.Name
                    } : null,
                    Items = items.Select(item => new Dtos.TodoListItemDetailResponseDto
                    {
                        Id = item.Id,
                        TodoListId = item.TodoListId,
                        Name = item.Name,
                        Description = item.Description,
                        Completed = item.Completed,
                        Progress = item.Progress,
                        TodoList = query.IncludeTodoList && item.TodoList != null ? new Dtos.TodoListSummaryDto
                        {
                            Id = item.TodoList.Id,
                            Name = item.TodoList.Name
                        } : null
                    }).ToList()
                };

                _logger.LogInformation("Successfully retrieved {Count} TodoListItems for TodoList: {TodoListId} (Page {Page} of {TotalPages})",
                    items.Count, query.TodoListId, query.Page, totalPages);

                return ServiceResult<GetAllTodoListItemsQueryResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving TodoListItems for TodoList: {TodoListId}", query.TodoListId);
                return ServiceResult<GetAllTodoListItemsQueryResponse>.Failure(
                    "An error occurred while retrieving TodoListItems", 500, ex.Message);
            }
        }
    }
}