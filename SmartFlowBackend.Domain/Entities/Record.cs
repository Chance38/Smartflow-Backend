using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFlowBackend.Domain.Entities;

[Index(nameof(Date))]
public class Record
{
    [Key]
    public Guid RecordId { get; set; }
    public CategoryType CategoryType { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<string> TagNames { get; set; } = new List<string>();
    public float Amount { get; set; }
    public DateOnly Date { get; set; }

    [ForeignKey("Category")]
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    [ForeignKey("User")]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}