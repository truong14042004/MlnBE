using System.ComponentModel.DataAnnotations;

namespace DigitalDetox.Api.Models;

public class ScreenTimeLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(64)]
    public string UserId { get; set; } = "anon";

    [Required, MaxLength(64)]
    public string Website { get; set; } = string.Empty;

    /// <summary>Incremental seconds for this entry (delta since last sync).</summary>
    public int DurationSeconds { get; set; }

    /// <summary>The local day this slice belongs to (yyyy-MM-dd), as reported by the client.</summary>
    [Required, MaxLength(10)]
    public string Day { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
