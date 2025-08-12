using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MediatR;

namespace TodoApi.Mediation.TodoList
{
    /// <summary>
    /// Represents a command to create a new TodoList.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TodoListCreateCommand : IRequest<TodoApi.Infrastructure.ServiceResult<TodoListCreateCommandResponse>>
    {
        /// <summary>
        /// Gets or sets the name of the TodoList to create.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public required string Name { get; set; }
    }
}