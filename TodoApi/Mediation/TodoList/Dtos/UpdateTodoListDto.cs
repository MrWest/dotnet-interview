using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Mediation.TodoList.Dtos
{
    /// <summary>
    /// Represents the data transfer object for updating an existing TodoList.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UpdateTodoListDto
    {
        /// <summary>
        /// Gets or sets the updated name of the TodoList.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public required string Name { get; set; }
    }
}