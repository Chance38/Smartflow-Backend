using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFlowBackend.Domain.Entities;

public class Tag
{
    [Key]
    public Guid Id { get; set; }

    public required string Name { get; set; }

    // Foreign Keys
    [ForeignKey("User")]
    public Guid UserId { get; set; }
    [ForeignKey("Category")]
    public Guid CategoryId { get; set; }

    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<Record> Records { get; set; } = new List<Record>();
}