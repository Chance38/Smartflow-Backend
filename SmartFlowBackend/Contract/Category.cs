using System.Text.Json.Serialization;

namespace Contracts.Category;

public class AddCategoryRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required Type Type { get; set; }
}

public enum Type
{
    Expense,
    Income
}
