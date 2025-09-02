using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Application.Contracts;

public class AddCategoryRequest
{
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "Category Name is required")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    [Required(ErrorMessage = "Category Type is required")]
    public required CategoryType Type { get; set; }
}
