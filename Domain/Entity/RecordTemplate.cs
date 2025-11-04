using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entity;

[Index(nameof(Name))]
[Index(nameof(UserId))]
public class RecordTemplate
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? CategoryName { get; set; }
    public CategoryType? CategoryType { get; set; }
    public List<string> TagNames { get; set; } = new List<string>();
    public float Amount { get; set; }
    public Guid UserId { get; set; }
}