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

public class UpdateCategoryRequest
{
    [JsonPropertyName("oldName")]
    [Required(ErrorMessage = "Old Category Name is required")]
    public required string OldName { get; set; }

    [JsonPropertyName("newName")]
    [Required(ErrorMessage = "New Category Name is required")]
    public required string NewName { get; set; }

    [JsonPropertyName("type")]
    [Required(ErrorMessage = "Category Type is required")]
    public required CategoryType Type { get; set; }
}
