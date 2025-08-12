using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Mediation.TodoListItem
{ 
    /// <summary>
    /// Represents the response from an update TodoListItem command.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UpdateTodoListItemCommandResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier of the updated TodoListItem.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent TodoList.
        /// </summary>
        public long TodoListId { get; set; }

        /// <summary>
        /// Gets or sets the updated name of the TodoListItem.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the updated description of the TodoListItem.
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
        /// Gets or sets the update timestamp.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}