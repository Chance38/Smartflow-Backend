using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Contracts;

public class AddRecordTemplateRequest
{
    public required string RecordTemplateName { get; set; }
    public required CategoryType CategoryType { get; set; }

}

public class GetAllRecordTemplatesResponse
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("expenses")]
    public required List<Expense> Expenses { get; set; }
}
