using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entity;

[Index(nameof(UserId))]
public class MonthlySummary
{
    [Key]
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public float Income { get; set; }
    public float Expense { get; set; }
    public Guid UserId { get; set; }
}
