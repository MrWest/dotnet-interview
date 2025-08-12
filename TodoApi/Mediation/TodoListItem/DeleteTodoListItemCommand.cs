
using System.Diagnostics.CodeAnalysis;
using MediatR;
using TodoApi.Infrastructure;

namespace TodoApi.Mediation.TodoListItem
{
    /// <summary>
    /// Represents a command to delete a TodoListItem.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DeleteTodoListItemCommand : IRequest<ServiceResult<DeleteTodoListItemCommandResponse>>
    {
        /// <summary>
        /// Gets or sets the ID of the parent TodoList.
        /// </summary>
        public long TodoListId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the TodoListItem to delete.
        /// </summary>
        public long Id { get; set; }
    }
}