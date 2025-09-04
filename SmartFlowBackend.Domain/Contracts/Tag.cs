using System.Text.Json.Serialization;

namespace SmartFlowBackend.Domain.Contracts;

public class TagDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

public class AddTagRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("category")]
    public required string Category { get; set; }
}
