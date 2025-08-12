using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Mediation.TodoList
{
    /// <summary>
    /// Represents the response from a delete TodoList command.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DeleteTodoListCommandResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier of the deleted TodoList.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets a message indicating the result of the deletion.
        /// </summary>
        public string Message { get; set; } = "TodoList deleted successfully";

        /// <summary>
        /// Gets or sets the deletion timestamp.
        /// </summary>
        public DateTime DeletedAt { get; set; }

        /// <summary>
        /// Gets or sets the number of related TodoListItems that were also deleted.
        /// </summary>
        public int DeletedItemsCount { get; set; }
    }
}