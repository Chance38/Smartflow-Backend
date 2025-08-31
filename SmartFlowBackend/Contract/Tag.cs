using System.Text.Json.Serialization;

namespace Contracts.Tag;

public class AddTagRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("category")]
    public required string Category { get; set; }
}
