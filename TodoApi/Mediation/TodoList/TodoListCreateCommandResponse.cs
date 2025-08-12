using System.Diagnostics.CodeAnalysis;


namespace TodoApi.Mediation.TodoList
{
    /// <summary>
    /// Represents the response from a TodoList creation command.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TodoListCreateCommandResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier of the created TodoList.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the created TodoList.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
