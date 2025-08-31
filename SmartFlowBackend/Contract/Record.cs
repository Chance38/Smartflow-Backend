// using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Contracts.Record;

public class AddRecordRequest
{
    [JsonPropertyName("category")]
    public required string Category { get; set; }

    [JsonPropertyName("type")]
    public required Category.Type Type { get; set; }

    [JsonPropertyName("tag")]
    public required string Tag { get; set; }

    [JsonPropertyName("amount")]
    public required string Amount { get; set; }

    [JsonPropertyName("date")]
    public required DateTime Date { get; set; }
}

public class GetThisMonthRecordResponse
{
    [JsonPropertyName("balance")]
    public required string Balance { get; set; }

    [JsonPropertyName("totalIncome")]
    public required int TotalIncome { get; set; }

    [JsonPropertyName("totalExpense")]
    public required int TotalExpense { get; set; }

    [JsonPropertyName("expenses")]
    public required List<Expense> Expenses { get; set; }
}

public class Expense
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("amount")]
    public required int Amount { get; set; }
}

public class GetAllMonthRecordsResponse
{
    [JsonPropertyName("year")]
    public required string Year { get; set; }

    [JsonPropertyName("month")]
    public required string Month { get; set; }

    [JsonPropertyName("expense")]
    public required int Expense { get; set; }

    [JsonPropertyName("income")]
    public required int Income { get; set; }
}

