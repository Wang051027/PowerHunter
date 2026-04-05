using PowerHunter.Models;
using PowerHunter.Services;
using Xunit;

namespace PowerHunter.Tests;

public sealed class AppUsageAlertEvaluatorTests
{
    [Fact]
    public async Task EvaluateQuietlyAsync_forwards_records_to_alert_service()
    {
        var alertService = new FakeAlertService();
        var evaluator = new AppUsageAlertEvaluator(alertService);
        List<AppUsageRecord> usageRecords =
        [
            CreateUsageRecord("Video", 45.9),
            CreateUsageRecord("Browser", 40.2),
        ];

        await evaluator.EvaluateQuietlyAsync(usageRecords);

        Assert.Equal(1, alertService.EvaluateCallCount);
        Assert.Equal(usageRecords, alertService.LastEvaluatedRecords);
    }

    [Fact]
    public async Task EvaluateQuietlyAsync_skips_empty_usage_sets()
    {
        var alertService = new FakeAlertService();
        var evaluator = new AppUsageAlertEvaluator(alertService);

        await evaluator.EvaluateQuietlyAsync([]);

        Assert.Equal(0, alertService.EvaluateCallCount);
    }

    [Fact]
    public async Task EvaluateQuietlyAsync_swallows_alert_service_exceptions()
    {
        var alertService = new FakeAlertService { ThrowOnEvaluate = true };
        var evaluator = new AppUsageAlertEvaluator(alertService);

        var exception = await Record.ExceptionAsync(() =>
            evaluator.EvaluateQuietlyAsync([CreateUsageRecord("Maps", 22.5)]));

        Assert.Null(exception);
        Assert.Equal(1, alertService.EvaluateCallCount);
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

    private sealed class FakeAlertService : IAlertService
    {
        public int EvaluateCallCount { get; private set; }
        public List<AppUsageRecord>? LastEvaluatedRecords { get; private set; }
        public bool ThrowOnEvaluate { get; init; }

        public Task<BatteryAlert> CreateAlertAsync(string title, string description, double thresholdPercent)
        {
            throw new NotSupportedException();
        }

        public Task ToggleAlertAsync(int alertId, bool isEnabled)
        {
            throw new NotSupportedException();
        }

        public Task DeleteAlertAsync(int alertId)
        {
            throw new NotSupportedException();
        }

        public Task<List<BatteryAlert>> GetAlertsAsync()
        {
            throw new NotSupportedException();
        }

        public Task EvaluateAlertsAsync(IEnumerable<AppUsageRecord> usageRecords)
        {
            EvaluateCallCount++;
            LastEvaluatedRecords = usageRecords.ToList();

            if (ThrowOnEvaluate)
                throw new InvalidOperationException("boom");

            return Task.CompletedTask;
        }
    }
}
