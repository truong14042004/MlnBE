using DigitalDetox.Api.Models;

namespace DigitalDetox.Api.Services;

public static class RecommendationEngine
{
    /// <summary>Cheap rule-based recommendations; can later be swapped for an LLM call.</summary>
    public static List<string> Generate(IReadOnlyList<ScreenTimeLog> logs)
    {
        var tips = new List<string>();
        if (logs.Count == 0)
        {
            tips.Add("Chào mừng bạn! Hãy đặt giới hạn mỗi ngày cho từng app để bắt đầu hành trình detox.");
            return tips;
        }

        var totalsBySite = logs
            .GroupBy(l => l.Website)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.DurationSeconds));

        var top = totalsBySite.OrderByDescending(kv => kv.Value).FirstOrDefault();
        if (top.Key != null && top.Value > 0)
        {
            var minutes = top.Value / 60;
            tips.Add($"Bạn dành nhiều thời gian nhất cho {top.Key} ({minutes} phút). Hãy thử giảm giới hạn 10-20%.");
        }

        // Late-night usage heuristic
        var lateNight = logs.Where(l => l.CreatedAt.Hour >= 22 || l.CreatedAt.Hour < 2).Sum(l => l.DurationSeconds);
        if (lateNight > 30 * 60)
        {
            tips.Add("Bạn hay lướt mạng xã hội sau 10 giờ tối. Thử ngưng dùng màn hình 1 tiếng trước khi ngủ để cải thiện giấc ngủ.");
        }

        // Weekend bingeing
        var weekend = logs.Where(l => l.CreatedAt.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday).Sum(l => l.DurationSeconds);
        var weekday = logs.Where(l => l.CreatedAt.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)).Sum(l => l.DurationSeconds);
        if (weekend > weekday && weekend > 60 * 60)
        {
            tips.Add("Bạn lướt nhiều hơn vào cuối tuần. Hãy lên kế hoạch một hoạt động offline mỗi cuối tuần để cân bằng lại.");
        }

        if (totalsBySite.TryGetValue("TikTok", out var tt) && tt > 60 * 60)
        {
            tips.Add("TikTok đang chiếm khá nhiều thời gian. Feed vô tận được thiết kế để giữ bạn lướt mãi - hãy đặt khung giờ ngắn và có chủ đích.");
        }

        if (tips.Count == 0)
        {
            tips.Add("Bạn đang giữ cân bằng tốt. Duy trì giới hạn ổn định trong một tuần để hình thành thói quen.");
        }

        return tips;
    }

    public static int AwarenessScore(int totalSeconds)
    {
        var minutes = totalSeconds / 60.0;
        var score = 100 - (minutes / 10.0);
        return (int)Math.Clamp(Math.Round(score), 0, 100);
    }
}
