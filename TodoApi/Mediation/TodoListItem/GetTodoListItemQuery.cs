using System.Diagnostics.CodeAnalysis;
using MediatR;
using TodoApi.Infrastructure;

namespace TodoApi.Mediation.TodoListItem
{
     /// <summary>
    /// Represents a query to get a single TodoListItem by composite key.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetTodoListItemQuery : IRequest<ServiceResult<GetTodoListItemQueryResponse>>
    {
        /// <summary>
        /// Gets or sets the ID of the parent TodoList.
        /// </summary>
        public long TodoListId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the TodoListItem.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include parent TodoList information.
        /// </summary>
        public bool IncludeTodoList { get; set; } = false;
    }
}