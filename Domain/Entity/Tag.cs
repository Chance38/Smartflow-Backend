using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Entity;

[Index(nameof(UserId))]
public class Tag
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid UserId { get; set; }

    public ICollection<Record> Records { get; set; } = new List<Record>();
}