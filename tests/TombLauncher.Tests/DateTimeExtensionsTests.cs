using System.Globalization;
using TombLauncher.Core.Extensions;

namespace TombLauncher.Tests;

public class DateTimeExtensionsTests
{
    [Fact]
    public void GetDateAtMidnight_StripsTimeInfo()
    {
        var dateTime = new DateTime(2000, 1, 1, 15, 41, 23);
        var atMidnight = dateTime.GetDateAtMidnight();
        Assert.Equal(TimeSpan.Zero, atMidnight.TimeOfDay);
    }

    [Fact]
    public void GetDateAtMidnight_MidnightReturnsSelf()
    {
        var dateTime = new DateTime(2000, 1, 1);
        var atMidnight = dateTime.GetDateAtMidnight();
        Assert.Equal(dateTime, atMidnight);
    }

    [Fact]
    public void IsYesterday_TodayReturnsFalse()
    {
        var now = DateTime.Now;
        var isYesterday = now.IsYesterday();
        Assert.False(isYesterday);
    }
    
    [Fact]
    public void IsYesterday_YesterdayReturnsTrue()
    {
        var yesterday = DateTime.Now.AddDays(-1);
        var isYesterday = yesterday.IsYesterday();
        Assert.True(isYesterday);
    }

    [Fact]
    public void IsYesterday_TwoDaysAgoReturnsFalse()
    {
        var twoDaysAgo = DateTime.Now.AddDays(-2);
        var isYesterday = twoDaysAgo.IsYesterday();
        Assert.False(isYesterday);
    }

    [Fact]
    public void IsYesterday_TomorrowReturnsFalse()
    {
        var tomorrow = DateTime.Now.AddDays(1);
        var isYesterday = tomorrow.IsYesterday();
        Assert.False(isYesterday);
    }

    [Fact]
    public void GetOneSecondToMidnight_ReturnsCorrectTime()
    {
        var oneSecondToMidnight = DateTime.Now.GetOneSecondToMidnight();
        Assert.Equal(new TimeSpan(23, 59, 59), oneSecondToMidnight.TimeOfDay);
    }

    [Fact]
    public void Average_NullReturnsZero()
    {
        List<TimeSpan>? data = null;
        Assert.Equal(TimeSpan.Zero, data.Average());
    }

    [Fact]
    public void Average_EmptyReturnsZero()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        List<TimeSpan> data = [];
        Assert.Equal(TimeSpan.Zero, data.Average());
    }

    [Fact]
    public void Average_OneElementReturnsEquivalent()
    {
        var data = new List<TimeSpan>() { TimeSpan.FromHours(1) };
        Assert.Equal(TimeSpan.FromHours(1), data.Average());
    }

    [Fact]
    public void Average_CorrectlyReturnsFullHours()
    {
        var data = new List<TimeSpan>() { TimeSpan.FromHours(1), TimeSpan.FromHours(3) };
        Assert.Equal(TimeSpan.FromHours(2), data.Average());
    }

    [Fact]
    public void Average_RoundsAwayFromZero()
    {
        var data = new List<TimeSpan>() { TimeSpan.FromTicks(1), TimeSpan.FromTicks(2) };
        Assert.Equal(TimeSpan.FromTicks(2), data.Average());
    }

    [Fact]
    public void Sum_NullReturnsZero()
    {
        IEnumerable<TimeSpan>? data = null;
        Assert.Equal(TimeSpan.Zero, data.Sum());
    }
    
    
    [Fact]
    public void Sum_EmptyReturnsZero()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var data = new List<TimeSpan>();
        Assert.Equal(TimeSpan.Zero, data.Sum());
    }

    [Fact]
    public void Sum_OneElementReturnsFirst()
    {
        var data = new List<TimeSpan>() { TimeSpan.FromTicks(1) };
        Assert.Equal(TimeSpan.FromTicks(1), data.Sum());
    }

    [Fact]
    public void Sum_MoreElementsReturnsSum()
    {
        var data = new List<TimeSpan> { TimeSpan.FromTicks(1), TimeSpan.FromTicks(2) };
        Assert.Equal(TimeSpan.FromTicks(3), data.Sum());
    }

    [Theory]
    [InlineData([DayOfWeek.Sunday, "en-US"])]
    [InlineData([DayOfWeek.Monday, "it-IT"])]
    [InlineData([DayOfWeek.Sunday, "en"])]
    public void GetCultureSensitiveOrder_OrderShouldBeCorrect(DayOfWeek dayOfWeek, string cultureName)
    {
        Assert.Equal(0, dayOfWeek.GetCultureSensitiveOrder(CultureInfo.GetCultureInfo(cultureName)));
    }
}