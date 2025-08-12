using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Mediation.TodoListItem.Dtos
{
    /// <summary>
    /// Represents the data transfer object for creating a new TodoListItem.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CreateTodoListItemDto
    {
        /// <summary>
        /// Gets or sets the name of the TodoListItem.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(500, ErrorMessage = "Name cannot exceed 500 characters")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the TodoListItem.
        /// </summary>
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the item is completed.
        /// </summary>
        public bool Completed { get; set; } = false;

        /// <summary>
        /// Gets or sets the progress percentage (0-100).
        /// </summary>
        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
        public int Progress { get; set; } = 0;
    }
}