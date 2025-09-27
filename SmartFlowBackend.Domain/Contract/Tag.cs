using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Contract;

public class Tag
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

public class AddTagRequest
{
    [JsonPropertyName("tag")]
    [Required(ErrorMessage = "Tag is required")]
    public required Tag Tag { get; set; }
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
    [JsonPropertyName("tag")]
    [Required(ErrorMessage = "Tag is required")]
    public required Tag Tag { get; set; }
}

public class UpdateTagRequest
{
    [JsonPropertyName("oldTag")]
    [Required(ErrorMessage = "Old Tag is required")]
    public required Tag OldTag { get; set; }

    [JsonPropertyName("newTag")]
    [Required(ErrorMessage = "New Tag is required")]
    public required Tag NewTag { get; set; }
}