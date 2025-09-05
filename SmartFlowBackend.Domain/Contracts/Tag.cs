using System.ComponentModel.DataAnnotations;
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
    [Required(ErrorMessage = "Name is required")]
    public required string Name { get; set; }

    [JsonPropertyName("category")]
    [Required(ErrorMessage = "Category is required")]
    public required string Category { get; set; }
}

public class GetAllTagsResponse
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("tags")]
    public required List<Tag> Tags { get; set; }
}

public class DeleteTagRequest
{
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "Name is required")]
    public required string Name { get; set; }
}

public class UpdateTagRequest
{
    [JsonPropertyName("oldName")]
    [Required(ErrorMessage = "OldName is required")]
    public required string OldName { get; set; }

    [JsonPropertyName("newName")]
    [Required(ErrorMessage = "NewName is required")]
    public required string NewName { get; set; }

    [JsonPropertyName("category")]
    [Required(ErrorMessage = "Category is required")]
    public required string CategoryName { get; set; }
}