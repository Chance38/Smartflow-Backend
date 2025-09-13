using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFlowBackend.Domain.Entities;

public class RecordTemplate
{
    [Key]
    public Guid RecordTemplateId { get; set; }
    public required string RecordTemplateName { get; set; }
    public string? CategoryName { get; set; }
    public CategoryType CategoryType { get; set; }
    public List<string> TagNames { get; set; } = new List<string>();
    public float Amount { get; set; }

    [ForeignKey("User")]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}