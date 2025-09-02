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
    [ForeignKey("Tag")]
    public Guid TagId { get; set; }
    [ForeignKey("Category")]
    public Guid CategoryId { get; set; }
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    public Tag Tag { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public User User { get; set; } = null!;
}