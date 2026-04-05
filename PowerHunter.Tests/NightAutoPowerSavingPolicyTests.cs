using PowerHunter.Models;
using PowerHunter.Services;
using Xunit;

namespace PowerHunter.Tests;

public sealed class NightAutoPowerSavingPolicyTests
{
    [Fact]
    public void IsActive_returns_false_when_setting_is_disabled()
    {
        var settings = new UserSettings
        {
            NightAutoPowerSavingEnabled = false,
        };

        var isActive = NightAutoPowerSavingPolicy.IsActive(
            settings,
            new DateTime(2026, 3, 30, 23, 0, 0));

        Assert.False(isActive);
    }

    [Theory]
    [InlineData(22, true)]
    [InlineData(23, true)]
    [InlineData(0, true)]
    [InlineData(6, true)]
    [InlineData(7, false)]
    [InlineData(12, false)]
    [InlineData(21, false)]
    public void IsActive_uses_the_default_overnight_window(int hour, bool expected)
    {
        var settings = new UserSettings
        {
            NightAutoPowerSavingEnabled = true,
        };

        var isActive = NightAutoPowerSavingPolicy.IsActive(
            settings,
            new DateTime(2026, 3, 30, hour, 0, 0));

        Assert.Equal(expected, isActive);
    }

    [Fact]
    public void ResolveMonitoringInterval_returns_power_saving_interval_when_night_mode_is_active()
    {
        var settings = new UserSettings
        {
            NightAutoPowerSavingEnabled = true,
        };

        var interval = NightAutoPowerSavingPolicy.ResolveMonitoringInterval(
            settings,
            defaultInterval: TimeSpan.FromMinutes(5),
            powerSavingInterval: TimeSpan.FromMinutes(15),
            localNow: new DateTime(2026, 3, 30, 23, 0, 0));

        Assert.Equal(TimeSpan.FromMinutes(15), interval);
    }

    [Fact]
    public void ResolveMonitoringInterval_keeps_the_slower_interval_when_default_is_already_lower_frequency()
    {
        var settings = new UserSettings
        {
            NightAutoPowerSavingEnabled = true,
        };

        var interval = NightAutoPowerSavingPolicy.ResolveMonitoringInterval(
            settings,
            defaultInterval: TimeSpan.FromMinutes(20),
            powerSavingInterval: TimeSpan.FromMinutes(15),
            localNow: new DateTime(2026, 3, 30, 23, 0, 0));

        Assert.Equal(TimeSpan.FromMinutes(20), interval);
    }
}
