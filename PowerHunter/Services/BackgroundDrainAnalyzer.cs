using PowerHunter.Models;

namespace PowerHunter.Services;

/// <summary>
/// Detects apps that look suspiciously active in the background.
/// Uses balanced heuristics so findings are not too rare,
/// while still filtering obvious false positives.
/// </summary>
public static class BackgroundDrainAnalyzer
{
    private const double MinEstimatedDrainPercent = 2.5;
    private const double MinBackgroundMinutes = 8.0;
    private const double MinBackgroundRatio = 0.30;
    private const double MinForegroundServiceMinutes = 8.0;

    public static List<BackgroundDrainFinding> Analyze(IEnumerable<AppUsageRecord> records)
    {
        return records
            .Select(CreateFinding)
            .Where(finding => finding is not null)
            .Cast<BackgroundDrainFinding>()
            .OrderByDescending(finding => finding.EstimatedDrainPercent)
            .ThenByDescending(finding => finding.BackgroundUsageMinutes)
            .ToList();
    }

    private static BackgroundDrainFinding? CreateFinding(AppUsageRecord record)
    {
        var totalBackgroundMinutes = Math.Round(
            Math.Max(record.BackgroundUsageMinutes, 0) + Math.Max(record.ForegroundServiceMinutes, 0),
            1);

        var foregroundMinutes = Math.Round(Math.Max(record.UsageMinutes, 0), 1);
        var totalTrackedMinutes = foregroundMinutes + totalBackgroundMinutes;

        if (totalTrackedMinutes <= 0)
            return null;

        var backgroundRatio = totalBackgroundMinutes / totalTrackedMinutes;

        var looksBackgroundHeavy =
            record.UsagePercentage >= MinEstimatedDrainPercent &&
            totalBackgroundMinutes >= MinBackgroundMinutes &&
            backgroundRatio >= MinBackgroundRatio;

        var looksPersistent =
            record.UsagePercentage >= 4.0 &&
            totalBackgroundMinutes >= 15.0;

        var looksServiceDriven =
            record.UsagePercentage >= 2.0 &&
            record.ForegroundServiceMinutes >= MinForegroundServiceMinutes;

        if (!looksBackgroundHeavy && !looksPersistent && !looksServiceDriven)
            return null;

        var severity = DetermineSeverity(
            record.UsagePercentage,
            totalBackgroundMinutes,
            record.ForegroundServiceMinutes);

        var isOfficialPowerData =
            record.IsOfficialPowerData ||
            AppUsageSourceKind.IsOfficial(record.UsageSource);

        return new BackgroundDrainFinding(
            record.AppId,
            record.AppName,
            Math.Round(record.UsagePercentage, 1),
            totalBackgroundMinutes,
            foregroundMinutes,
            Math.Round(record.ForegroundServiceMinutes, 1),
            Math.Round(backgroundRatio, 2),
            record.UsageSource,
            isOfficialPowerData,
            severity,
            BuildSummary(
                record.AppName,
                record.UsagePercentage,
                totalBackgroundMinutes,
                record.ForegroundServiceMinutes,
                isOfficialPowerData));
    }

    private static string DetermineSeverity(
        double estimatedDrainPercent,
        double totalBackgroundMinutes,
        double foregroundServiceMinutes)
    {
        if (estimatedDrainPercent >= 7.0 || totalBackgroundMinutes >= 30.0 || foregroundServiceMinutes >= 20.0)
            return "high";

        if (estimatedDrainPercent >= 4.0 || totalBackgroundMinutes >= 15.0 || foregroundServiceMinutes >= 8.0)
            return "medium";

        return "low";
    }

    private static string BuildSummary(
        string appName,
        double estimatedDrainPercent,
        double totalBackgroundMinutes,
        double foregroundServiceMinutes,
        bool isOfficialPowerData)
    {
        var servicePart = foregroundServiceMinutes > 0
            ? $" (foreground service {foregroundServiceMinutes:F0} min)"
            : string.Empty;

        var metricPhrase = isOfficialPowerData
            ? $"accounted for about {estimatedDrainPercent:F1}% of the reported app battery usage"
            : $"reached a {estimatedDrainPercent:F1}% background activity impact score";

        return $"{appName} stayed active in the background for about {totalBackgroundMinutes:F0} min{servicePart} and {metricPhrase}.";
    }
}