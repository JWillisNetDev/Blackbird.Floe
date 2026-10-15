using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Blackbird.Floe.Data.Models.Identity;

public class User
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required, EmailAddress] 
    public required string Email { get; set; }
    
    [Required]
    public required string PasswordHash { get; set; }
    
    public bool IsAdmin { get; set; }

    [SetsRequiredMembers]
    public User(string email, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        Email = email.Trim();

        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash.Trim();
    }
}
