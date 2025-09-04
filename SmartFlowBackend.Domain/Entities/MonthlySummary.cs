using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFlowBackend.Domain.Entities;

public class MonthlySummary
{
    [Key]
    public Guid Id { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }
    public float Income { get; set; }
    public float Expense { get; set; }

    [ForeignKey("User")]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
