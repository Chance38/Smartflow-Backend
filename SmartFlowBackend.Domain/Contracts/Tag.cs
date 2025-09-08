using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartFlowBackend.Domain.Contracts;

public class AddTagRequest
{
    [JsonPropertyName("tagName")]
    [Required(ErrorMessage = "Tag name is required")]
    public required string TagName { get; set; }
}

public class Tag
{
    [JsonPropertyName("tagName")]
    public required string TagName { get; set; }
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
    [JsonPropertyName("tagName")]
    [Required(ErrorMessage = "Tag name is required")]
    public required string TagName { get; set; }
}

public class UpdateTagRequest
{
    [JsonPropertyName("oldTagName")]
    [Required(ErrorMessage = "OldName is required")]
    public required string OldName { get; set; }

    [JsonPropertyName("newTagName")]
    [Required(ErrorMessage = "NewName is required")]
    public required string NewName { get; set; }
}