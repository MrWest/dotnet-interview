using System.Diagnostics.CodeAnalysis;
using MediatR;
using TodoApi.Infrastructure;

namespace TodoApi.Mediation.TodoListItem
{
    /// <summary>
    /// Represents a query to get all TodoListItems for a specific TodoList.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetAllTodoListItemsQuery : IRequest<ServiceResult<GetAllTodoListItemsQueryResponse>>
    {
        /// <summary>
        /// Gets or sets the ID of the parent TodoList.
        /// </summary>
        public long TodoListId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include parent TodoList information.
        /// </summary>
        public bool IncludeTodoList { get; set; } = false;

        /// <summary>
        /// Gets or sets the filter for completion status (null = all, true = completed only, false = incomplete only).
        /// </summary>
        public bool? CompletedFilter { get; set; }

        /// <summary>
        /// Gets or sets the page number for pagination (1-based).
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Gets or sets the page size for pagination.
        /// </summary>
        public int PageSize { get; set; } = 50;
    }
}
