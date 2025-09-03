namespace SmartFlowBackend.Domain.Entities;

public class MonthlySummary
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public float Income { get; set; }
    public float Expense { get; set; }
    public Guid UserId { get; set; }
    public virtual User User { get; set; }
}
