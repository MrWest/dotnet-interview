using System.Diagnostics.CodeAnalysis;
using MediatR;
using TodoApi.Infrastructure;

namespace TodoApi.Mediation.TodoList
{
     /// <summary>
    /// Represents a command to delete a TodoList.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DeleteTodoListCommand : IRequest<ServiceResult<DeleteTodoListCommandResponse>>
    {
        /// <summary>
        /// Gets or sets the ID of the TodoList to delete.
        /// </summary>
        public long Id { get; set; }
    }

}