using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Mediation.TodoList.Dtos
{
    /// <summary>
    /// Represents the data transfer object for updating an existing TodoList.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TodoListResponseDto
    {
         /// <summary>
        /// Gets or sets the unique identifier of the TodoList.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the TodoList.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection of TodoListItems (optional for some operations).
        /// </summary>
        public ICollection<TodoListItemResponseDto>? Items { get; set; }
    }
}