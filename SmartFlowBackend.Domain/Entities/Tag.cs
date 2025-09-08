using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFlowBackend.Domain.Entities;

public class Tag
{
    [Key]
    public Guid TagId { get; set; }
    public required string TagName { get; set; }

    [ForeignKey("User")]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Record> Records { get; set; } = new List<Record>();
}