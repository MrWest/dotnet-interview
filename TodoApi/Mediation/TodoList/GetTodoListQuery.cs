using System.Diagnostics.CodeAnalysis;
using MediatR;

namespace TodoApi.Mediation.TodoList
{
    /// <summary>
    /// Represents a query to get a single TodoList by ID.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetTodoListQuery : IRequest<Infrastructure.ServiceResult<GetTodoListQueryResponse>>
    {
        /// <summary>
        /// Gets or sets the ID of the TodoList to retrieve.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include TodoListItems in the response.
        /// </summary>
        public bool IncludeItems { get; set; } = false;
    }
}