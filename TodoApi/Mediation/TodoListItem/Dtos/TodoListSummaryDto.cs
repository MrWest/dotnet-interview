using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Mediation.TodoListItem.Dtos
{
    /// <summary>
    /// Represents a summary of TodoList information for nested responses.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TodoListSummaryDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the TodoList.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the TodoList.
        /// </summary>
        public required string Name { get; set; }
    }
}