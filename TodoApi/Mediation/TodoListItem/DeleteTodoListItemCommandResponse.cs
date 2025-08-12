using System.Diagnostics.CodeAnalysis;
using MediatR;
using TodoApi.Infrastructure;

namespace TodoApi.Mediation.TodoListItem
{
    /// <summary>
    /// Represents the response from a delete TodoListItem command.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DeleteTodoListItemCommandResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier of the deleted TodoListItem.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent TodoList.
        /// </summary>
        public long TodoListId { get; set; }

        /// <summary>
        /// Gets or sets a message indicating the result of the deletion.
        /// </summary>
        public string Message { get; set; } = "TodoListItem deleted successfully";

        /// <summary>
        /// Gets or sets the deletion timestamp.
        /// </summary>
        public DateTime DeletedAt { get; set; }

        /// <summary>
        /// Gets or sets the name of the deleted item for confirmation.
        /// </summary>
        public string DeletedItemName { get; set; } = string.Empty;
    }
}