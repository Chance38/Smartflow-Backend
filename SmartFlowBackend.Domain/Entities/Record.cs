namespace SmartFlowBackend.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[Index(nameof(Date))]
public class Record
{
    [Key]
    public Guid Id { get; set; }

    public float Amount { get; set; }
    public DateOnly Date { get; set; }
    public CategoryType Type { get; set; }

    // Foreign Keys
    [ForeignKey("Category")]
    public Guid? CategoryId { get; set; }
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    public string CategoryName { get; set; } = string.Empty;
    public List<string> TagNames { get; set; } = new();

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public Category? Category { get; set; }
    public User User { get; set; } = null!;
}