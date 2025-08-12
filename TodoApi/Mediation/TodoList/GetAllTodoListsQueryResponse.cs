

using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Mediation.TodoList
{
    /// <summary>
    /// Represents the response from a get all TodoLists query.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetAllTodoListsQueryResponse
    {
        /// <summary>
        /// Gets or sets the collection of TodoLists.
        /// </summary>
        public IList<Dtos.TodoListResponseDto> TodoLists { get; set; } = new List<Dtos.TodoListResponseDto>();

        /// <summary>
        /// Gets or sets the total count of TodoLists.
        /// </summary>
        public int TotalCount { get; set; }
    }
}