using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFlowBackend.Domain.Entities;

public class Category
{
    [Key]
    public Guid Id { get; set; }

    public required string Name { get; set; }
    public CategoryType Type { get; set; }

    // Foreign Key
    [ForeignKey("User")]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Record> Records { get; set; } = new List<Record>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}

public enum CategoryType
{
    [EnumMember(Value = "income")]
    Income,

    [EnumMember(Value = "expense")]
    Expense
}