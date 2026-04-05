using PowerHunter.Services;
using Xunit;

namespace PowerHunter.Tests;

public sealed class BatteryGuardianDeliveryPolicyTests
{
    [Fact]
    public void Decide_returns_in_app_dialog_when_app_is_foreground()
    {
        var mode = BatteryGuardianDeliveryPolicy.Decide(
            isAppInForeground: true,
            notificationsEnabled: false,
            canSendLocalNotification: false);

        Assert.Equal(BatteryGuardianDeliveryMode.InAppDialog, mode);
    }

    [Fact]
    public void Decide_returns_local_notification_when_background_and_notifications_available()
    {
        var mode = BatteryGuardianDeliveryPolicy.Decide(
            isAppInForeground: false,
            notificationsEnabled: true,
            canSendLocalNotification: true);

        Assert.Equal(BatteryGuardianDeliveryMode.LocalNotification, mode);
    }

    [Fact]
    public void Decide_returns_none_when_background_notifications_are_disabled()
    {
        var mode = BatteryGuardianDeliveryPolicy.Decide(
            isAppInForeground: false,
            notificationsEnabled: false,
            canSendLocalNotification: true);

        Assert.Equal(BatteryGuardianDeliveryMode.None, mode);
    }
}
