
using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Mediation.TodoList
{
    /// <summary>
    /// Represents the response from a get TodoList query.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetTodoListQueryResponse
    {
        /// <summary>
        /// Gets or sets the TodoList data.
        /// </summary>
        public required Dtos.TodoListResponseDto TodoList { get; set; }
    }
}