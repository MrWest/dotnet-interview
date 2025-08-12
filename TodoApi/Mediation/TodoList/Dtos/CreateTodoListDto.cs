using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Mediation.TodoList.Dtos
{
    /// <summary>
    /// Represents the data transfer object for creating a new TodoList.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CreateTodoListDto
    {
        /// <summary>
        /// Gets or sets the name of the TodoList.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public required string Name { get; set; }
    }
}