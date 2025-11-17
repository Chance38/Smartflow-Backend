using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entity;

[Index(nameof(CategoryName))]
[Index(nameof(TagNames))]
[Index(nameof(Date))]
[Index(nameof(UserId))]
public class Record
{
    [Key]
    public Guid Id { get; set; }
    public float Amount { get; set; }
    public string CategoryName { get; set; }
    public CategoryType Type { get; set; }
    public List<string> TagNames { get; set; } = new();
    public DateOnly Date { get; set; }
    public Guid UserId { get; set; }

    [ForeignKey("Category")]
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}