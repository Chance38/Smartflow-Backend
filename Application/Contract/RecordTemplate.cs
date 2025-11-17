using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Application.Contract;

public class RecordTemplate
{
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "RecordTemplateName is required")]
    public required string Name { get; set; }

    [JsonPropertyName("categoryName")]
    public string? CategoryName { get; set; }

    [JsonPropertyName("categoryType")]
    [Required(ErrorMessage = "CategoryType is required")]
    public required CategoryType CategoryType { get; set; }

    [JsonPropertyName("tags")]
    public List<Tag> Tags { get; set; } = new List<Tag>();

    [JsonPropertyName("amount")]
    [Required(ErrorMessage = "Amount is required")]
    public required float Amount { get; set; }
}

public class AddRecordTemplateRequest
{
    [JsonPropertyName("recordTemplate")]
    [Required(ErrorMessage = "RecordTemplate is required")]
    public required RecordTemplate RecordTemplate { get; set; }
}

public class GetAllRecordTemplatesResponse
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("recordTemplates")]
    public List<RecordTemplate> RecordTemplates { get; set; } = new List<RecordTemplate>();
}

public class DeleteRecordTemplateRequest
{
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "RecordTemplate Name is required")]
    public required string Name { get; set; }
}