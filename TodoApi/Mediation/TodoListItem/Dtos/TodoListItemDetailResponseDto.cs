using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Mediation.TodoListItem.Dtos
{
     /// <summary>
    /// Represents the comprehensive response data transfer object for TodoListItem operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TodoListItemDetailResponseDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the TodoListItem.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent TodoList.
        /// </summary>
        public long TodoListId { get; set; }

        /// <summary>
        /// Gets or sets the name of the TodoListItem.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the TodoListItem.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the item is completed.
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// Gets or sets the progress percentage.
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Gets or sets the parent TodoList information (optional).
        /// </summary>
        public TodoListSummaryDto? TodoList { get; set; }
    }
}
