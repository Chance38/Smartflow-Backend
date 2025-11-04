using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entity;

[Index(nameof(UserId))]
public class Balance
{
    [Key]
    public Guid Id { get; set; }
    public float Amount { get; set; }
    public Guid UserId { get; set; }
}