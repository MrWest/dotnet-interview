using System.Text.Json.Serialization;

namespace TodoApi.ExternalApi.Models
{
    public class ExternalTodoItem
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("source_id")]
        public string? SourceId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("completed")]
        public bool? Completed { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
