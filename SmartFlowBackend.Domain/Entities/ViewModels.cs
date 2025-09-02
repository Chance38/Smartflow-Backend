namespace SmartFlowBackend.Domain.Entities;

public class MonthlyRecordView
{
    public Guid UserId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public float Income { get; set; }
    public float Expense { get; set; }
}

public class BalanceView
{
    public Guid UserId { get; set; }
    public float Balance { get; set; }
}
