using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Mediation.TodoListItem
{
    /// <summary>
    /// Represents the response from a get all TodoListItems query.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetAllTodoListItemsQueryResponse
    {
        /// <summary>
        /// Gets or sets the collection of TodoListItems.
        /// </summary>
        public IList<Dtos.TodoListItemDetailResponseDto> Items { get; set; } = new List<Dtos.TodoListItemDetailResponseDto>();

        /// <summary>
        /// Gets or sets the total count of TodoListItems (before pagination).
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent TodoList.
        /// </summary>
        public long TodoListId { get; set; }

        /// <summary>
        /// Gets or sets the parent TodoList information.
        /// </summary>
        public Dtos.TodoListSummaryDto? TodoList { get; set; }
    }
}