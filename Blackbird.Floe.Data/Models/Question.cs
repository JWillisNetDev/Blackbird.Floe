using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Blackbird.Floe.Data;

public class Question
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required, MinLength(1)]
    public List<string> Answers { get; set; } = [];

    public Question? NextQuestion { get; set; }

    [ForeignKey(nameof(NextQuestion))]
    public string? NextQuestionId { get; set; }

    [SetsRequiredMembers]
    public Question(string title, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title.Trim();

        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        Description = description.Trim();
    }
}
