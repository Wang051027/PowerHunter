using PowerHunter.Models;
using PowerHunter.Services;
using Xunit;

namespace PowerHunter.Tests;

public sealed class AppCategoryResolverTests
{
    [Fact]
    public void Resolve_prefers_primary_os_category_hint_over_secondary_behavior_tags()
    {
        var rawUsage = new RawAppUsage(
            PackageName: "com.example.player",
            AppLabel: "Player",
            ForegroundTimeMs: 5 * 60_000,
            Date: DateTime.UtcNow.Date,
            CategorySignal: new AppCategorySignal(
                PrimaryCategoryHint: "audio",
                BehaviorTags:
                [
                    "launcher-video",
                    "immersive-video",
                ]));

        var category = AppCategoryResolver.Resolve(rawUsage);

        Assert.Equal(AppCategoryResolver.MusicAudio, category);
    }

    [Fact]
    public void Resolve_uses_behavior_tags_when_primary_hint_is_missing()
    {
        var rawUsage = new RawAppUsage(
            PackageName: "com.example.social.client",
            AppLabel: "Moments",
            ForegroundTimeMs: 6 * 60_000,
            Date: DateTime.UtcNow.Date,
            CategorySignal: new AppCategorySignal(
                BehaviorTags:
                [
                    "share-target",
                    "messaging-capability",
                ]));

        var category = AppCategoryResolver.Resolve(rawUsage);

        Assert.Equal(AppCategoryResolver.Social, category);
    }

    [Fact]
    public void Resolve_falls_back_to_known_package_catalog()
    {
        var category = AppCategoryResolver.Resolve("com.spotify.music");

        Assert.Equal(AppCategoryResolver.MusicAudio, category);
    }

    [Fact]
    public void ResolveOriginalCategory_prefers_system_primary_category_name()
    {
        var rawUsage = new RawAppUsage(
            PackageName: "com.example.docs",
            AppLabel: "Docs",
            ForegroundTimeMs: 5 * 60_000,
            Date: DateTime.UtcNow.Date,
            CategorySignal: new AppCategorySignal(
                PrimaryCategoryHint: "productivity"));

        var category = AppCategoryResolver.ResolveOriginalCategory(rawUsage);

        Assert.Equal("Productivity", category);
    }

    [Fact]
    public void GetPreferredCategoryLabel_falls_back_to_legacy_category_when_original_category_is_missing()
    {
        var record = new AppUsageRecord
        {
            Category = AppCategoryResolver.Tools,
        };

        var category = AppCategoryResolver.GetPreferredCategoryLabel(record);

        Assert.Equal(AppCategoryResolver.Tools, category);
    }

    [Theory]
    [InlineData("Entertainment", AppCategoryResolver.Other)]
    [InlineData("Work", AppCategoryResolver.Tools)]
    [InlineData("Music", AppCategoryResolver.MusicAudio)]
    [InlineData("Gaming", AppCategoryResolver.Game)]
    [InlineData("Audio", AppCategoryResolver.MusicAudio)]
    [InlineData("Productivity", AppCategoryResolver.Tools)]
    [InlineData("Maps", AppCategoryResolver.Tools)]
    public void Normalize_maps_legacy_categories_to_new_taxonomy(string input, string expected)
    {
        Assert.Equal(expected, AppCategoryResolver.Normalize(input));
    }
}
