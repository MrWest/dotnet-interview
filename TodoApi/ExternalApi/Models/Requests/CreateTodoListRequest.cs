using System.Text.Json.Serialization;

namespace TodoApi.ExternalApi.Models.Requests
{
    public class CreateTodoListRequest
    {
        [JsonPropertyName("source_id")]
        public string? SourceId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("items")]
        public List<CreateTodoItemRequest>? Items { get; set; }
    }

    public class CreateTodoItemRequest
    {
        [JsonPropertyName("source_id")]
        public string? SourceId { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("completed")]
        public bool? Completed { get; set; }
    }
}
