namespace DigitalDetox.Api.Models;

/// <summary>One embedded challenge a user has configured.</summary>
public class ChallengeItem
{
    public string Id { get; set; } = string.Empty;
    public string Metric { get; set; } = "total";
    public int Target { get; set; }
}

/// <summary>
/// Per-user settings persisted in MongoDB (one document per user).
/// Holds daily limits, the configured challenges and the streak — everything
/// that used to live in browser localStorage.
/// </summary>
public class UserSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Owner of these settings (resolved from the JWT).</summary>
    public string UserId { get; set; } = string.Empty;

    // Daily limits in minutes (0 = no limit).
    public int YouTubeLimit { get; set; }
    public int FacebookLimit { get; set; }
    public int TikTokLimit { get; set; }

    // Daily Detox Challenge streak.
    public int StreakCount { get; set; }
    public string LastCompletedDay { get; set; } = string.Empty;

    public List<ChallengeItem> Challenges { get; set; } = new();

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
