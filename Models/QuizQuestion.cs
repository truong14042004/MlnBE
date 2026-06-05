using System.ComponentModel.DataAnnotations;

namespace DigitalDetox.Api.Models;

/// <summary>
/// A single multiple-choice quiz question (Marxist-Leninist philosophy set,
/// originally sourced from question.pdf). Persisted in MongoDB so the extension
/// pulls questions from the backend instead of hardcoding them.
/// </summary>
public class QuizQuestion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Question { get; set; } = string.Empty;

    /// <summary>Answer choices (2-4 options).</summary>
    public List<string> Options { get; set; } = new();

    /// <summary>Zero-based index of the correct option.</summary>
    public int AnswerIdx { get; set; }
}
