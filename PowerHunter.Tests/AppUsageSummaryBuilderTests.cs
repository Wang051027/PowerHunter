using PowerHunter.Models;
using PowerHunter.Services;
using Xunit;

namespace PowerHunter.Tests;

public sealed class AppUsageSummaryBuilderTests
{
    [Fact]
    public void Build_prefers_official_power_stats_when_available()
    {
        var syncedAtUtc = new DateTime(2026, 3, 30, 4, 0, 0, DateTimeKind.Utc);
        List<RawAppUsage> rawUsage =
        [
            new(
                PackageName: "com.maps.nav",
                AppLabel: "Maps",
                ForegroundTimeMs: 2 * 60_000,
                Date: syncedAtUtc.Date,
                ConsumedPowerMah: 12.5),
            new(
                PackageName: "com.video.stream",
                AppLabel: "Video",
                ForegroundTimeMs: 8 * 60_000,
                Date: syncedAtUtc.Date,
                ConsumedPowerMah: 37.5),
        ];

        var records = AppUsageSummaryBuilder.Build(rawUsage, syncedAtUtc);

        Assert.Equal(2, records.Count);
        Assert.Equal("com.video.stream", records[0].AppId);
        Assert.Equal(AppUsageSourceKind.OfficialBatteryStats, records[0].UsageSource);
        Assert.True(records.All(record => record.IsOfficialPowerData));
        Assert.Equal(75.0, records[0].UsagePercentage);
        Assert.Equal(37.5, records[0].PowerConsumedMah);
        Assert.Equal(syncedAtUtc, records[0].LastSyncedAtUtc);
    }

    [Fact]
    public void Build_falls_back_to_system_usage_stats_when_official_power_is_unavailable()
    {
        var syncedAtUtc = new DateTime(2026, 3, 30, 5, 30, 0, DateTimeKind.Utc);
        List<RawAppUsage> rawUsage =
        [
            new(
                PackageName: "com.chat.sync",
                AppLabel: "Chat",
                ForegroundTimeMs: 10 * 60_000,
                Date: syncedAtUtc.Date,
                VisibleTimeMs: 18 * 60_000,
                ForegroundServiceTimeMs: 6 * 60_000),
            new(
                PackageName: "com.reader.news",
                AppLabel: "News",
                ForegroundTimeMs: 4 * 60_000,
                Date: syncedAtUtc.Date,
                VisibleTimeMs: 4 * 60_000,
                ForegroundServiceTimeMs: 0),
        ];

        var records = AppUsageSummaryBuilder.Build(rawUsage, syncedAtUtc);

        Assert.Equal(2, records.Count);
        Assert.Equal("com.chat.sync", records[0].AppId);
        Assert.Equal(AppUsageSourceKind.SystemUsageStats, records[0].UsageSource);
        Assert.False(records[0].IsOfficialPowerData);
        Assert.Equal(83.7, records[0].UsagePercentage);
        Assert.Equal(0, records[0].PowerConsumedMah);
    }

    [Fact]
    public void Build_preserves_original_system_category_for_distribution_views()
    {
        var syncedAtUtc = new DateTime(2026, 3, 30, 7, 0, 0, DateTimeKind.Utc);
        List<RawAppUsage> rawUsage =
        [
            new(
                PackageName: "com.example.docs",
                AppLabel: "Docs",
                ForegroundTimeMs: 8 * 60_000,
                Date: syncedAtUtc.Date,
                CategorySignal: new AppCategorySignal(
                    PrimaryCategoryHint: "productivity")),
        ];

        var records = AppUsageSummaryBuilder.Build(rawUsage, syncedAtUtc);

        var record = Assert.Single(records);
        Assert.Equal(AppCategoryResolver.Tools, record.Category);
        Assert.Equal("Productivity", record.OriginalCategory);
    }

    [Fact]
    public void Build_keeps_positive_power_records_even_when_activity_window_is_short()
    {
        var syncedAtUtc = new DateTime(2026, 3, 30, 6, 0, 0, DateTimeKind.Utc);
        List<RawAppUsage> rawUsage =
        [
            new(
                PackageName: "com.sync.worker",
                AppLabel: "Sync Worker",
                ForegroundTimeMs: 15_000,
                Date: syncedAtUtc.Date,
                ConsumedPowerMah: 4.8),
        ];

        var records = AppUsageSummaryBuilder.Build(rawUsage, syncedAtUtc);

        var record = Assert.Single(records);
        Assert.Equal(AppUsageSourceKind.OfficialBatteryStats, record.UsageSource);
        Assert.True(record.IsOfficialPowerData);
        Assert.Equal(100.0, record.UsagePercentage);
    }
}
