using System.Text.Json.Serialization;
using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Application.Contracts;

public class AddRecordRequest
{
    [JsonPropertyName("category")]
    public required string Category { get; set; }

    [JsonPropertyName("type")]
    public required CategoryType Type { get; set; }

    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("amount")]
    public required float Amount { get; set; }

    [JsonPropertyName("date")]
    public required DateOnly Date { get; set; }
}

public class GetThisMonthRecordResponse
{
    [JsonPropertyName("balance")]
    public required float Balance { get; set; }

    [JsonPropertyName("totalIncome")]
    public required float TotalIncome { get; set; }

    [JsonPropertyName("totalExpense")]
    public required float TotalExpense { get; set; }

    [JsonPropertyName("expenses")]
    public required List<Expense> Expenses { get; set; }
}

public class Expense
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("amount")]
    public required float Amount { get; set; }
}

public class GetAllMonthRecordsResponse
{
    [JsonPropertyName("records")]
    public required List<RecordPerMonth> Records { get; set; }
}

public class RecordPerMonth
{
    [JsonPropertyName("year")]
    public required string Year { get; set; }

    [JsonPropertyName("month")]
    public required string Month { get; set; }

    [JsonPropertyName("expense")]
    public required float Expense { get; set; }

    [JsonPropertyName("income")]
    public required float Income { get; set; }
}

