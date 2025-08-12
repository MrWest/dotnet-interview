using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Mediation.TodoList
{
    /// <summary>
    /// Represents the response from an update TodoList command.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UpdateTodoListCommandResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier of the updated TodoList.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the updated name of the TodoList.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the update timestamp.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}