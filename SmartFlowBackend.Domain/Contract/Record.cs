using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Contract;

public class AddRecordRequest
{
    [JsonPropertyName("category")]
    [Required(ErrorMessage = "Category is required")]
    public required Category Category { get; set; }

    [JsonPropertyName("tags")]
    public List<Tag>? Tags { get; set; }

    [JsonPropertyName("amount")]
    [Required(ErrorMessage = "Amount is required")]
    public required float Amount { get; set; }

    [JsonPropertyName("date")]
    [Required(ErrorMessage = "Date is required")]
    public required DateOnly Date { get; set; }
}

public class Expense
{
    [JsonPropertyName("categoryName")]
    public required string CategoryName { get; set; }

    [JsonPropertyName("amount")]
    public required float Amount { get; set; }
}

public class GetThisMonthExpensesResponse
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("expenses")]
    public List<Expense> Expenses { get; set; } = new List<Expense>();
}
