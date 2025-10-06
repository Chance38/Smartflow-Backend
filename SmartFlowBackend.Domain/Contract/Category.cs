using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Domain.Contract;

public enum CategoryType
{
    [EnumMember(Value = "INCOME")]
    INCOME,

    [EnumMember(Value = "EXPENSE")]
    EXPENSE
}

public class Category
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required CategoryType Type { get; set; }
}

public class AddCategoryRequest
{
    [JsonPropertyName("category")]
    public required Category Category { get; set; }
}

public class GetAllCategoriesResponse
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("categories")]
    public List<Category> Categories { get; set; } = new List<Category>();
}

public class DeleteCategoryRequest
{
    [JsonPropertyName("category")]
    public required Category Category { get; set; }
}

public class UpdateCategoryRequest
{
    [JsonPropertyName("oldCategory")]
    [Required(ErrorMessage = "Old Category is required")]
    public required Category OldCategory { get; set; }

    [JsonPropertyName("newCategory")]
    [Required(ErrorMessage = "New Category is required")]
    public required Category NewCategory { get; set; }
}
