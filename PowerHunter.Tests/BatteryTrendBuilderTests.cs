using PowerHunter.Models;
using PowerHunter.Services;
using Xunit;

namespace PowerHunter.Tests;

public sealed class BatteryTrendBuilderTests
{
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    [Fact]
    public void BuildIntradayUsageTrend_accumulates_only_battery_drops_for_the_selected_day()
    {
        DateTime selectedDate = new(2026, 3, 30);
        List<BatteryRecord> records =
        [
            new() { BatteryLevel = 90, RecordedAt = new DateTime(2026, 3, 30, 1, 0, 0, DateTimeKind.Utc) },
            new() { BatteryLevel = 84, RecordedAt = new DateTime(2026, 3, 30, 3, 0, 0, DateTimeKind.Utc) },
            new() { BatteryLevel = 88, RecordedAt = new DateTime(2026, 3, 30, 4, 0, 0, DateTimeKind.Utc) },
            new() { BatteryLevel = 79, RecordedAt = new DateTime(2026, 3, 30, 6, 0, 0, DateTimeKind.Utc) },
        ];

        var trend = BatteryTrendBuilder.BuildIntradayUsageTrend(records, selectedDate, Utc);

        Assert.Equal(4, trend.Count);
        Assert.Equal("01:00", trend[0].Label);
        Assert.Equal(0, trend[0].Value);
        Assert.Equal(6.0, trend[1].Value);
        Assert.Equal(6.0, trend[2].Value);
        Assert.Equal(15.0, trend[3].Value);
    }

    [Fact]
    public void BuildDailyUsageTrend_groups_by_local_day_and_marks_today()
    {
        DateTime fromDate = new(2026, 3, 29);
        DateTime toDate = new(2026, 3, 30);
        List<BatteryRecord> records =
        [
            new() { BatteryLevel = 100, RecordedAt = new DateTime(2026, 3, 29, 0, 0, 0, DateTimeKind.Utc) },
            new() { BatteryLevel = 91, RecordedAt = new DateTime(2026, 3, 29, 5, 0, 0, DateTimeKind.Utc) },
            new() { BatteryLevel = 90, RecordedAt = new DateTime(2026, 3, 30, 1, 0, 0, DateTimeKind.Utc) },
            new() { BatteryLevel = 78, RecordedAt = new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc) },
        ];

        var trend = BatteryTrendBuilder.BuildDailyUsageTrend(
            records,
            fromDate,
            toDate,
            Utc,
            todayLocalDateOverride: toDate);

        Assert.Equal(2, trend.Count);
        Assert.Equal("3/29 Sun", trend[0].Label);
        Assert.Equal(9.0, trend[0].Value);
        Assert.Equal("Today", trend[1].Label);
        Assert.Equal(12.0, trend[1].Value);
    }

    [Fact]
    public void GetUtcBoundsForLocalDate_returns_a_full_day_window()
    {
        DateTime selectedDate = new(2026, 3, 30);

        var bounds = BatteryTrendBuilder.GetUtcBoundsForLocalDate(selectedDate, Utc);

        Assert.Equal(new DateTime(2026, 3, 30, 0, 0, 0, DateTimeKind.Utc), bounds.FromUtc);
        Assert.Equal(new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc), bounds.ToUtc);
    }
}
