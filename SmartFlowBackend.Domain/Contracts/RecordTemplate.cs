using System.Text.Json.Serialization;
using SmartFlowBackend.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace SmartFlowBackend.Domain.Contracts;

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
    public required List<RecordTemplate> RecordTemplates { get; set; }
}

public class RecordTemplate
{
    [JsonPropertyName("recordTemplateName")]
    [Required(ErrorMessage = "RecordTemplateName is required")]
    public required string RecordTemplateName { get; set; }

    [JsonPropertyName("categoryName")]
    public string? CategoryName { get; set; }

    [JsonPropertyName("categoryType")]
    [Required(ErrorMessage = "CategoryType is required")]
    public required CategoryType CategoryType { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new List<string>();

    [JsonPropertyName("amount")]
    [Required(ErrorMessage = "Amount is required")]
    public required float Amount { get; set; }
}

public class DeleteRecordTemplateRequest
{
    [JsonPropertyName("recordTemplateName")]
    [Required(ErrorMessage = "RecordTemplateName is required")]
    public required string RecordTemplateName { get; set; }
}