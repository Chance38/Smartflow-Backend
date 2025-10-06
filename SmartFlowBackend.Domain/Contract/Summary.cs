using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Contract;

public class SummaryPerMonth
{
    [JsonPropertyName("year")]
    public required int Year { get; set; }

    [JsonPropertyName("month")]
    public required int Month { get; set; }

    [JsonPropertyName("expense")]
    public required float Expense { get; set; }

    [JsonPropertyName("income")]
    public required float Income { get; set; }
}

public class GetMonthSummariesResponse
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("summaries")]
    public List<SummaryPerMonth> Summaries { get; set; } = new List<SummaryPerMonth>();
}
