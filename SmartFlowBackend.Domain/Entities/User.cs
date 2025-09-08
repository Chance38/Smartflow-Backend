using System.ComponentModel.DataAnnotations;

namespace SmartFlowBackend.Domain.Entities;

public class User
{
    [Key]
    public Guid UserId { get; set; }

    public required string Username { get; set; }
    public required string UserAccount { get; set; }
    public required string UserPassword { get; set; }

    public float Balance { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Record> Records { get; set; } = new List<Record>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}