using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFlowBackend.Domain.Entities;

public class Category
{
    [Key]
    public Guid CategoryId { get; set; }

    public required string CategoryName { get; set; }
    public CategoryType Type { get; set; }

    [ForeignKey("User")]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Record> Records { get; set; } = new List<Record>();
}

public enum CategoryType
{
    [EnumMember(Value = "income")]
    Income,

    [EnumMember(Value = "expense")]
    Expense
}