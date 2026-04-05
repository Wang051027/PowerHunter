using PowerHunter.Models;
using PowerHunter.Services;
using Xunit;

namespace PowerHunter.Tests;

public sealed class AlertPollingPolicyTests
{
    [Fact]
    public void ShouldPoll_returns_false_when_usage_collection_is_unavailable()
    {
        var settings = new UserSettings { NotificationsEnabled = true };
        List<BatteryAlert> alerts = [CreateAlert(isEnabled: true)];

        var shouldPoll = AlertPollingPolicy.ShouldPoll(
            settings,
            alerts,
            isUsageCollectionAvailable: false);

        Assert.False(shouldPoll);
    }

    [Fact]
    public void ShouldPoll_returns_false_when_notifications_are_disabled()
    {
        var settings = new UserSettings { NotificationsEnabled = false };
        List<BatteryAlert> alerts = [CreateAlert(isEnabled: true)];

        var shouldPoll = AlertPollingPolicy.ShouldPoll(
            settings,
            alerts,
            isUsageCollectionAvailable: true);

        Assert.False(shouldPoll);
    }

    [Fact]
    public void ShouldPoll_returns_false_when_no_enabled_alerts_exist()
    {
        var settings = new UserSettings { NotificationsEnabled = true };
        List<BatteryAlert> alerts =
        [
            CreateAlert(isEnabled: false),
            CreateAlert(isEnabled: false),
        ];

        var shouldPoll = AlertPollingPolicy.ShouldPoll(
            settings,
            alerts,
            isUsageCollectionAvailable: true);

        Assert.False(shouldPoll);
    }

    [Fact]
    public void ShouldPoll_returns_true_when_notifications_and_enabled_alert_exist()
    {
        var settings = new UserSettings { NotificationsEnabled = true };
        List<BatteryAlert> alerts =
        [
            CreateAlert(isEnabled: false),
            CreateAlert(isEnabled: true),
        ];

        var shouldPoll = AlertPollingPolicy.ShouldPoll(
            settings,
            alerts,
            isUsageCollectionAvailable: true);

        Assert.True(shouldPoll);
    }

    [Fact]
    public void AlertCheckInterval_is_30_seconds()
    {
        Assert.Equal(TimeSpan.FromSeconds(30), BatteryRefreshDefaults.AlertCheckInterval);
        Assert.Equal(TimeSpan.FromSeconds(30), BatteryRefreshDefaults.UsageSyncInterval);
    }

    private static BatteryAlert CreateAlert(bool isEnabled)
    {
        return new BatteryAlert
        {
            Id = 1,
            Title = "High Usage",
            ThresholdPercent = 40,
            IsEnabled = isEnabled,
        };
    }
}
