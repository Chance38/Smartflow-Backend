using System.Text.Json.Serialization;

namespace SmartFlowBackend.Domain.Contracts;

public class Tag
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("category")]
    public required string Category { get; set; }
}

public class AddTagRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("category")]
    public required string Category { get; set; }
}

public class DeleteTagRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

public class UpdateTagRequest
{
    [JsonPropertyName("oldName")]
    public required string OldName { get; set; }

    [JsonPropertyName("newName")]
    public required string NewName { get; set; }

    [JsonPropertyName("category")]
    public required string CategoryName { get; set; }
}