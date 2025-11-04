using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Entity;

[Index(nameof(UserId))]
public class Category
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public CategoryType Type { get; set; }
    public Guid UserId { get; set; }

    public ICollection<Record> Records { get; set; } = new List<Record>();
}

public enum CategoryType
{
    [EnumMember(Value = "INCOME")]
    INCOME,

    [EnumMember(Value = "EXPENSE")]
    EXPENSE
}