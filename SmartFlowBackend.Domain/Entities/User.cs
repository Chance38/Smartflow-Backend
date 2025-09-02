using System.ComponentModel.DataAnnotations;

namespace SmartFlowBackend.Domain.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; }

    public required string Username { get; set; }
    public required string Account { get; set; }
    public required string Password { get; set; }

    public float InitialBalance { get; set; }
    public float SettingsBalance { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Record> Records { get; set; } = new List<Record>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}