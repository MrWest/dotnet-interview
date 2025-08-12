using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Mediation.TodoListItem
{
    /// <summary>
    /// Represents the response from a get TodoListItem query.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetTodoListItemQueryResponse
    {
        /// <summary>
        /// Gets or sets the TodoListItem data.
        /// </summary>
        public required Dtos.TodoListItemDetailResponseDto Item { get; set; }
    }

}
