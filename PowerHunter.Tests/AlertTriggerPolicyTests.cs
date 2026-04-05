using PowerHunter.Models;
using PowerHunter.Services;
using Xunit;

namespace PowerHunter.Tests;

public sealed class AlertTriggerPolicyTests
{
    [Fact]
    public void FindTriggeredApp_returns_null_when_smart_alerts_are_disabled()
    {
        var alert = CreateAlert(thresholdPercent: 25);
        List<AppUsageRecord> usageRecords =
        [
            CreateUsageRecord("Video", 30),
        ];

        var triggeredApp = AlertTriggerPolicy.FindTriggeredApp(
            alert,
            usageRecords,
            smartAlertsEnabled: false,
            notificationsEnabled: true,
            canSendLocalNotification: true,
            nowUtc: DateTime.UtcNow);

        Assert.Null(triggeredApp);
    }

    [Fact]
    public void FindTriggeredApp_returns_null_when_no_app_exceeds_threshold()
    {
        var alert = CreateAlert(thresholdPercent: 20);
        List<AppUsageRecord> usageRecords =
        [
            CreateUsageRecord("Maps", 11.5),
            CreateUsageRecord("Music", 18.7),
        ];

        var triggeredApp = AlertTriggerPolicy.FindTriggeredApp(
            alert,
            usageRecords,
            smartAlertsEnabled: true,
            notificationsEnabled: true,
            canSendLocalNotification: true,
            nowUtc: DateTime.UtcNow);

        Assert.Null(triggeredApp);
    }

    [Fact]
    public void FindTriggeredApp_returns_null_within_30_second_cooldown_window()
    {
        var now = DateTime.UtcNow;
        var alert = CreateAlert(
            thresholdPercent: 20,
            lastTriggeredAt: now.AddSeconds(-15));
        List<AppUsageRecord> usageRecords =
        [
            CreateUsageRecord("Chrome", 31.2),
        ];

        var triggeredApp = AlertTriggerPolicy.FindTriggeredApp(
            alert,
            usageRecords,
            smartAlertsEnabled: true,
            notificationsEnabled: true,
            canSendLocalNotification: true,
            nowUtc: now);

        Assert.Null(triggeredApp);
    }

    [Fact]
    public void FindTriggeredApp_returns_app_again_after_30_second_cooldown_elapses()
    {
        var now = DateTime.UtcNow;
        var alert = CreateAlert(
            thresholdPercent: 20,
            lastTriggeredAt: now.AddSeconds(-31));
        List<AppUsageRecord> usageRecords =
        [
            CreateUsageRecord("Chrome", 31.2),
        ];

        var triggeredApp = AlertTriggerPolicy.FindTriggeredApp(
            alert,
            usageRecords,
            smartAlertsEnabled: true,
            notificationsEnabled: true,
            canSendLocalNotification: true,
            nowUtc: now);

        Assert.NotNull(triggeredApp);
        Assert.Equal("Chrome", triggeredApp.AppName);
    }

    [Fact]
    public void FindTriggeredApp_returns_highest_usage_app_when_threshold_is_breached()
    {
        var alert = CreateAlert(thresholdPercent: 20);
        List<AppUsageRecord> usageRecords =
        [
            CreateUsageRecord("Chat", 24.5),
            CreateUsageRecord("Video", 41.3),
            CreateUsageRecord("Maps", 12.1),
        ];

        var triggeredApp = AlertTriggerPolicy.FindTriggeredApp(
            alert,
            usageRecords,
            smartAlertsEnabled: true,
            notificationsEnabled: true,
            canSendLocalNotification: true,
            nowUtc: DateTime.UtcNow);

        Assert.NotNull(triggeredApp);
        Assert.Equal("Video", triggeredApp.AppName);
        Assert.Equal(41.3, triggeredApp.UsagePercentage);
    }

    private static BatteryAlert CreateAlert(double thresholdPercent, DateTime? lastTriggeredAt = null)
    {
        return new BatteryAlert
        {
            Id = 1,
            Title = "Low Battery",
            ThresholdPercent = thresholdPercent,
            IsEnabled = true,
            LastTriggeredAt = lastTriggeredAt,
        };
    }

    private static AppUsageRecord CreateUsageRecord(string appName, double usagePercentage)
    {
        return new AppUsageRecord
        {
            AppId = $"com.powerhunter.{appName.ToLowerInvariant()}",
            AppName = appName,
            UsagePercentage = usagePercentage,
            IsOfficialPowerData = true,
        };
    }
}
