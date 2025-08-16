using System.Text.Json.Serialization;

namespace TodoApi.ExternalApi.Models
{
    /// <summary>
    /// Represents a TodoList as returned by the external API.
    /// Maps to the external API's TodoList model structure.
    /// </summary>
    public class ExternalTodoList
    {
        /// <summary>
        /// Unique identifier assigned by the external system.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Identifier of the source system that created this list.
        /// </summary>
        [JsonPropertyName("source_id")]
        public string? SourceId { get; set; }

        /// <summary>
        /// Name of the TodoList.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Timestamp when the list was created in the external system.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the list was last updated in the external system.
        /// </summary>
        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Collection of TodoItems associated with this list.
        /// </summary>
        [JsonPropertyName("items")]
        public List<ExternalTodoItem> Items { get; set; } = new();
    }
}