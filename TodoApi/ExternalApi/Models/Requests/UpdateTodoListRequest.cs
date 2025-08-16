using System.Text.Json.Serialization;

namespace TodoApi.ExternalApi.Models.Requests
{
    public class UpdateTodoListRequest
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class UpdateTodoItemRequest
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("completed")]
        public bool? Completed { get; set; }
    }
}
