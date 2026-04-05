using PowerHunter.Models;
using PowerHunter.Services;
using Xunit;

namespace PowerHunter.Tests;

public sealed class BackgroundDrainAnalyzerTests
{
    [Fact]
    public void Analyze_flags_app_with_high_background_time_and_drain()
    {
        List<AppUsageRecord> records =
        [
            new AppUsageRecord
            {
                AppId = "com.chatty.sync",
                AppName = "Chatty",
                UsagePercentage = 8.6,
                UsageMinutes = 6,
                BackgroundUsageMinutes = 26,
                ForegroundServiceMinutes = 4,
            },
        ];

        var findings = BackgroundDrainAnalyzer.Analyze(records);

        var finding = Assert.Single(findings);
        Assert.Equal("com.chatty.sync", finding.AppId);
        Assert.Equal("Chatty", finding.AppName);
        Assert.Equal("high", finding.Severity);
        Assert.Contains("background", finding.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Analyze_ignores_apps_that_are_mostly_foreground_usage()
    {
        List<AppUsageRecord> records =
        [
            new AppUsageRecord
            {
                AppId = "com.video.player",
                AppName = "Video Player",
                UsagePercentage = 11.4,
                UsageMinutes = 92,
                BackgroundUsageMinutes = 3,
                ForegroundServiceMinutes = 0,
            },
        ];

        var findings = BackgroundDrainAnalyzer.Analyze(records);

        Assert.Empty(findings);
    }

    [Fact]
    public void Analyze_treats_foreground_service_time_as_background_drain_signal()
    {
        List<AppUsageRecord> records =
        [
            new AppUsageRecord
            {
                AppId = "com.music.stream",
                AppName = "Music Stream",
                UsagePercentage = 4.3,
                UsageMinutes = 3,
                BackgroundUsageMinutes = 5,
                ForegroundServiceMinutes = 18,
            },
        ];

        var findings = BackgroundDrainAnalyzer.Analyze(records);

        var finding = Assert.Single(findings);
        Assert.Equal("medium", finding.Severity);
        Assert.True(finding.ForegroundServiceMinutes >= 18);
    }

    [Fact]
    public void Analyze_uses_source_aware_summary_when_data_is_not_official_power()
    {
        List<AppUsageRecord> records =
        [
            new AppUsageRecord
            {
                AppId = "com.sync.agent",
                AppName = "Sync Agent",
                UsagePercentage = 5.2,
                UsageMinutes = 4,
                BackgroundUsageMinutes = 18,
                ForegroundServiceMinutes = 0,
                UsageSource = AppUsageSourceKind.SystemUsageStats,
                IsOfficialPowerData = false,
            },
        ];

        var findings = BackgroundDrainAnalyzer.Analyze(records);

        var finding = Assert.Single(findings);
        Assert.Contains("impact score", finding.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("used 5.2% battery", finding.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Analyze_orders_findings_by_estimated_drain_descending()
    {
        List<AppUsageRecord> records =
        [
            new AppUsageRecord
            {
                AppId = "com.low",
                AppName = "Low Drain",
                UsagePercentage = 4.2,
                UsageMinutes = 4,
                BackgroundUsageMinutes = 15,
                ForegroundServiceMinutes = 0,
            },
            new AppUsageRecord
            {
                AppId = "com.high",
                AppName = "High Drain",
                UsagePercentage = 9.8,
                UsageMinutes = 7,
                BackgroundUsageMinutes = 28,
                ForegroundServiceMinutes = 3,
            },
        ];

        var findings = BackgroundDrainAnalyzer.Analyze(records);

        Assert.Equal(2, findings.Count);
        Assert.Equal("com.high", findings[0].AppId);
        Assert.Equal("com.low", findings[1].AppId);
    }
}
