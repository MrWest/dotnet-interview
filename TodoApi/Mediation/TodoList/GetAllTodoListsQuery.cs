using System.Diagnostics.CodeAnalysis;
using MediatR;
using TodoApi.Infrastructure;

namespace TodoApi.Mediation.TodoList
{
    /// <summary>
    /// Represents a query to get all TodoLists.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetAllTodoListsQuery : IRequest<ServiceResult<GetAllTodoListsQueryResponse>>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include TodoListItems in the response.
        /// </summary>
        public bool IncludeItems { get; set; } = false;
    }
}