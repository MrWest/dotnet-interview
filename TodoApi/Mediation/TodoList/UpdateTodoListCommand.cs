using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using TodoApi.Infrastructure;

namespace TodoApi.Mediation.TodoList
{
    /// <summary>
    /// Represents a command to update an existing TodoList.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UpdateTodoListCommand : IRequest<ServiceResult<UpdateTodoListCommandResponse>>
    {
        /// <summary>
        /// Gets or sets the ID of the TodoList to update.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the updated name of the TodoList.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public required string Name { get; set; }
    }
}