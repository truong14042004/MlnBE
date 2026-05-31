namespace DigitalDetox.Api.Dtos;

public record TrackingEntryDto(string Website, int DurationSeconds, string? UserId);

public record TrackingBatchDto(string UserId, string Day, List<TrackingEntryDto> Entries);

public record SiteTotalDto(string Website, int TotalSeconds);

public record DailyTotalDto(string Day, int TotalSeconds, Dictionary<string, int> ByWebsite);

public record SummaryDto(
    int TotalSeconds,
    int AwarenessScore,
    List<SiteTotalDto> ByWebsite,
    List<DailyTotalDto> Daily,
    List<string> Recommendations
);

public record ChallengeItemDto(string Id, string Metric, int Target);

public record UserSettingsDto(
    int YouTubeLimit,
    int FacebookLimit,
    int TikTokLimit,
    int StreakCount,
    string LastCompletedDay,
    List<ChallengeItemDto> Challenges
);
