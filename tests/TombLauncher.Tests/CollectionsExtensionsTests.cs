using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;

namespace TombLauncher.Tests;

public class CollectionsExtensionsTests
{
    [Fact]
    public void PickOneAtRandom_ThrowsOnEmptyCollection()
    {
        var empty = new List<int>();
        Assert.Throws<InvalidOperationException>(() => empty.PickOneAtRandom());
    }

    [Fact]
    public void PickOneAtRandom_ReturnsElementFromCollection()
    {
        var list = new List<string> { "a", "b", "c" };

        for (var i = 0; i < 50; i++)
        {
            var result = list.PickOneAtRandom();
            Assert.Contains(result, list);
        }
    }

    [Fact]
    public void PickOneAtRandom_SingleElement_ReturnsThatElement()
    {
        var list = new List<int> { 42 };
        Assert.Equal(42, list.PickOneAtRandom());
    }

    [Fact]
    public void PickOneAtRandom_CanReturnLastElement()
    {
        // With only 2 elements, after enough iterations the last one must appear
        var list = new List<int> { 1, 2 };
        var pickedValues = Enumerable.Range(0, 100).Select(_ => list.PickOneAtRandom()).ToHashSet();
        Assert.Contains(2, pickedValues);
    }

    // --- MergeWithOverrides ---

    [Fact]
    public void MergeWithOverrides_EmptyOverrides_ReturnsDefaults()
    {
        var defaults = new List<CheckableItem<string>>
        {
            new() { Value = "a", IsChecked = true },
            new() { Value = "b", IsChecked = false }
        };

        var result = defaults.MergeWithOverrides(new List<CheckableItem<string>>());

        Assert.Equal(2, result.Count);
        Assert.Equal("a", result[0].Value);
        Assert.Equal("b", result[1].Value);
    }

    [Fact]
    public void MergeWithOverrides_DisjointSets_ReturnsCombined()
    {
        var defaults = new List<CheckableItem<string>> { new() { Value = "a" } };
        var overrides = new List<CheckableItem<string>> { new() { Value = "b" } };

        var result = defaults.MergeWithOverrides(overrides);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, i => i.Value == "a");
        Assert.Contains(result, i => i.Value == "b");
    }

    [Fact]
    public void MergeWithOverrides_OverlappingItems_OverrideWins()
    {
        var defaults = new List<CheckableItem<string>>
        {
            new() { Value = "a", IsChecked = false },
            new() { Value = "b", IsChecked = true }
        };
        var overrides = new List<CheckableItem<string>>
        {
            new() { Value = "a", IsChecked = true }
        };

        var result = defaults.MergeWithOverrides(overrides);

        Assert.Equal(2, result.Count);
        var itemA = result.Single(i => i.Value == "a");
        Assert.True(itemA.IsChecked); // override value wins
    }

    [Fact]
    public void MergeWithOverrides_EmptyDefaults_ReturnsOverrides()
    {
        var defaults = new List<CheckableItem<string>>();
        var overrides = new List<CheckableItem<string>> { new() { Value = "x" } };

        var result = defaults.MergeWithOverrides(overrides);

        Assert.Single(result);
        Assert.Equal("x", result[0].Value);
    }
}
