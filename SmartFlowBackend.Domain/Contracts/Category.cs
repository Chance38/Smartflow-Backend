using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Contracts;

public class Category
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required CategoryType Type { get; set; }
}

public class AddCategoryRequest
{
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "Category Name is required")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    [Required(ErrorMessage = "Category Type is required")]
    public required CategoryType Type { get; set; }
}

public class DeleteCategoryRequest
{
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "Category Name is required")]
    public required string Name { get; set; }
}
