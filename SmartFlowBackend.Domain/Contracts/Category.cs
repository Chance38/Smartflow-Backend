using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Contracts;

public class Category
{
    [JsonPropertyName("categoryName")]
    public required string CategoryName { get; set; }

    [JsonPropertyName("categoryType")]
    public required CategoryType CategoryType { get; set; }
}

public class AddCategoryRequest
{
    [JsonPropertyName("categoryName")]
    [Required(ErrorMessage = "Category Name is required")]
    public required string CategoryName { get; set; }

    [JsonPropertyName("categoryType")]
    [Required(ErrorMessage = "Category Type is required")]
    public required CategoryType CategoryType { get; set; }
}

public class GetAllCategoriesResponse
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("categories")]
    public required List<Category> Categories { get; set; }
}

public class DeleteCategoryRequest
{
    [JsonPropertyName("categoryName")]
    [Required(ErrorMessage = "Category Name is required")]
    public required string CategoryName { get; set; }

    [JsonPropertyName("categoryType")]
    [Required(ErrorMessage = "Category Type is required")]
    public required CategoryType CategoryType { get; set; }
}

public class UpdateCategoryRequest
{
    [JsonPropertyName("oldCategoryName")]
    [Required(ErrorMessage = "Old Category Name is required")]
    public required string OldCategoryName { get; set; }

    [JsonPropertyName("newCategoryName")]
    [Required(ErrorMessage = "New Category Name is required")]
    public required string NewCategoryName { get; set; }

    [JsonPropertyName("oldCategoryType")]
    [Required(ErrorMessage = "Category Type is required")]
    public required CategoryType OldCategoryType { get; set; }

    [JsonPropertyName("newCategoryType")]
    [Required(ErrorMessage = "Category Type is required")]
    public required CategoryType NewCategoryType { get; set; }
}
