using System.ComponentModel;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using IconPacks.Avalonia.RemixIcon;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization;
using TombLauncher.ValueConverters;
using TombLauncher.ViewModels;
using TombLauncher.Tests.Fixtures;

namespace TombLauncher.Tests;

[Collection("Localization")]
public class ValueConverterTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    static ValueConverterTests()
    {
        var locManager = Substitute.For<ILocalizationManager>();
        locManager.GetLocalizedString(Arg.Any<string>(), Arg.Any<object[]>())
                  .Returns(x => x.ArgAt<string>(0));
        locManager[Arg.Any<string>()].Returns(x => x.ArgAt<string>(0));
        locManager.CurrentCulture.Returns(CultureInfo.InvariantCulture);
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddSingleton(locManager)
                .BuildServiceProvider());
    }

    // --- GreaterThanZeroToBoolConverter ---

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(0.5, true)]
    [InlineData(0.0, false)]
    [InlineData(-0.1, false)]
    [InlineData((long)100, true)]
    [InlineData((long)0, false)]
    [InlineData((byte)1, true)]
    [InlineData((byte)0, false)]
    public void GreaterThanZero_ReturnsExpected(object value, bool expected)
    {
        var converter = new GreaterThanZeroToBoolConverter();
        Assert.Equal(expected, converter.Convert(value, typeof(bool), null, Culture));
    }

    [Fact]
    public void GreaterThanZero_NullReturnsFalse()
    {
        var converter = new GreaterThanZeroToBoolConverter();
        Assert.Equal(false, converter.Convert(null, typeof(bool), null, Culture));
    }

    [Fact]
    public void GreaterThanZero_NonNumericReturnsFalse()
    {
        var converter = new GreaterThanZeroToBoolConverter();
        Assert.Equal(false, converter.Convert(new object(), typeof(bool), null, Culture));
    }

    // --- CollectionToVisibilityConverter ---

    [Fact]
    public void CollectionToVisibility_EmptyCollection_ReturnsTrue()
    {
        var converter = new CollectionToVisibilityConverter();
        Assert.Equal(true, converter.Convert(Array.Empty<int>(), typeof(bool), null, Culture));
    }

    [Fact]
    public void CollectionToVisibility_NonEmptyCollection_ReturnsFalse()
    {
        var converter = new CollectionToVisibilityConverter();
        Assert.Equal(false, converter.Convert(new[] { 1, 2 }, typeof(bool), null, Culture));
    }

    [Fact]
    public void CollectionToVisibility_Inverted_EmptyReturns_False()
    {
        var converter = new CollectionToVisibilityConverter { Invert = true };
        Assert.Equal(false, converter.Convert(Array.Empty<int>(), typeof(bool), null, Culture));
    }

    [Fact]
    public void CollectionToVisibility_Inverted_NonEmptyReturns_True()
    {
        var converter = new CollectionToVisibilityConverter { Invert = true };
        Assert.Equal(true, converter.Convert(new[] { 1 }, typeof(bool), null, Culture));
    }

    [Fact]
    public void CollectionToVisibility_NullReturnsFalse()
    {
        var converter = new CollectionToVisibilityConverter();
        Assert.Equal(false, converter.Convert(null, typeof(bool), null, Culture));
    }

    // --- IsSmallerOrEqualConverter ---

    [Fact]
    public void IsSmallerOrEqual_SmallerReturnsTrue()
    {
        var converter = new IsSmallerOrEqualConverter();
        var result = converter.Convert(new List<object?> { 2, 5 }, typeof(bool), null, Culture);
        Assert.Equal(true, result);
    }

    [Fact]
    public void IsSmallerOrEqual_EqualReturnsTrue()
    {
        var converter = new IsSmallerOrEqualConverter();
        var result = converter.Convert(new List<object?> { 3, 3 }, typeof(bool), null, Culture);
        Assert.Equal(true, result);
    }

    [Fact]
    public void IsSmallerOrEqual_LargerReturnsFalse()
    {
        var converter = new IsSmallerOrEqualConverter();
        var result = converter.Convert(new List<object?> { 5, 2 }, typeof(bool), null, Culture);
        Assert.Equal(false, result);
    }

    [Fact]
    public void IsSmallerOrEqual_WrongCountThrows()
    {
        var converter = new IsSmallerOrEqualConverter();
        Assert.Throws<ArgumentException>(() =>
            converter.Convert(new List<object?> { 1 }, typeof(bool), null, Culture));
    }

    // --- FileSizeConverter ---

    [Theory]
    [InlineData(0L, 0.0, "bytes")]
    [InlineData(512L, 512.0, "bytes")]
    [InlineData(1024L, 1024.0, "bytes")]
    [InlineData(1536L, 1.5, "kB")]
    [InlineData(1048576L, 1024.0, "kB")]
    [InlineData(1572864L, 1.5, "MB")]
    public void FileSizeConverter_FormatsCorrectly(long bytes, double expectedValue, string expectedUnit)
    {
        var converter = new FileSizeConverter();
        var result = (string?)converter.Convert(bytes, typeof(string), null, Culture);
        Assert.Equal($"{expectedValue:F2} {expectedUnit}", result);
    }

    [Fact]
    public void FileSizeConverter_NullReturnsEmpty()
    {
        var converter = new FileSizeConverter();
        Assert.Equal(string.Empty, converter.Convert(null, typeof(string), null, Culture));
    }

    // --- TransferSpeedConverter ---

    [Theory]
    [InlineData(512.0, 512.0, "B/s")]
    [InlineData(1024.0, 1024.0, "B/s")]
    [InlineData(1536.0, 1.5, "kB/s")]
    [InlineData(1048576.0, 1024.0, "kB/s")]
    [InlineData(1572864.0, 1.5, "MB/s")]
    public void TransferSpeedConverter_FormatsCorrectly(double speed, double expectedValue, string expectedUnit)
    {
        var converter = new TransferSpeedConverter();
        var result = (string?)converter.Convert(speed, typeof(string), null, Culture);
        Assert.Equal($"{expectedValue:F2} {expectedUnit}", result);
    }

    [Fact]
    public void TransferSpeedConverter_NullReturnsNull()
    {
        var converter = new TransferSpeedConverter();
        Assert.Null(converter.Convert(null, typeof(string), null, Culture));
    }

    private const PackIconRemixIconKind TrueIcon = PackIconRemixIconKind.CheckLine;
    private const PackIconRemixIconKind FalseIcon = PackIconRemixIconKind.DeleteBackFill;

    [Theory]
    [InlineData(true, TrueIcon)]
    [InlineData(false, FalseIcon)]
    [InlineData(";P", null)]
    public void BooleanToIconConverter_Convert_ConvertsCorrectly(object value, PackIconRemixIconKind? expectedValue)
    {
        var converter = new BooleanToIconConverter(){TrueValue = TrueIcon, FalseValue = FalseIcon};
        Assert.Equal(expectedValue, converter.Convert(value, typeof(PackIconRemixIconKind), null, Culture));
    }
    
    [Theory]
    [InlineData(TrueIcon, true)]
    [InlineData( FalseIcon, false)]
    [InlineData(null, false)]
    public void BooleanToIconConverter_ConvertBack_ConvertsCorrectly(PackIconRemixIconKind? value, bool expectedValue)
    {
        var converter = new BooleanToIconConverter(){TrueValue = TrueIcon, FalseValue = FalseIcon};
        Assert.Equal(expectedValue, converter.ConvertBack(value, typeof(bool), null, Culture));
    }

    private const string TrueString = "TRUE!!!";
    private const string FalseString = "NOPE!!!";
    
    [Theory]
    [InlineData(true, TrueString)]
    [InlineData(false, FalseString)]
    [InlineData(1, null)]
    [InlineData(null, null)]
    public void BooleanToStringConverter_Convert_ConvertsCorrectly(object? value, string? expectedValue)
    {
        var converter = new BooleanToStringConverter() { TrueValue = TrueString, FalseValue = FalseString };
        Assert.Equal(expectedValue, converter.Convert(value, typeof(string), null, Culture));
    }

    [Fact]
    public void BooleanToStringConverter_ConvertBack_Throws()
    {
        var converter = new BooleanToStringConverter();
        Assert.Throws<NotSupportedException>(() =>
            converter.ConvertBack("anything", typeof(bool), null, Culture));
    }

    // --- DateTimeFormatConverter ---

    private static readonly DateTime FixedDate = new DateTime(2000, 6, 15, 12, 30, 0);

    [Fact]
    public void DateTimeFormatConverter_DefaultDateReturnsEmpty()
    {
        var converter = new DateTimeFormatConverter();
        Assert.Equal(string.Empty, converter.Convert(default(DateTime), typeof(string), null, Culture));
    }

    [Fact]
    public void DateTimeFormatConverter_NonDateTimeReturnsNull()
    {
        var converter = new DateTimeFormatConverter();
        Assert.Null(converter.Convert("not a date", typeof(string), null, Culture));
    }

    [Fact]
    public void DateTimeFormatConverter_NoDesiredFormat_UsesCurrentCulture()
    {
        var converter = new DateTimeFormatConverter();
        var expected = FixedDate.ToString(CultureInfo.InvariantCulture.DateTimeFormat);
        Assert.Equal(expected, converter.Convert(FixedDate, typeof(string), null, Culture));
    }

    [Fact]
    public void DateTimeFormatConverter_WithDesiredFormat_UsesFormat()
    {
        var converter = new DateTimeFormatConverter { DesiredFormat = "yyyy-MM-dd" };
        Assert.Equal("2000-06-15", converter.Convert(FixedDate, typeof(string), null, Culture));
    }

    [Fact]
    public void DateTimeFormatConverter_ConvertBack_ParsesDate()
    {
        var converter = new DateTimeFormatConverter();
        var formatted = FixedDate.ToString(CultureInfo.InvariantCulture);
        var result = converter.ConvertBack(formatted, typeof(DateTime), null, Culture);
        Assert.Equal(FixedDate, result);
    }

    // --- DateTimeToStringConverter ---

    [Fact]
    public void DateTimeToStringConverter_NullReturnsNever()
    {
        var converter = new DateTimeToStringConverter();
        Assert.Equal("NEVER", converter.Convert(null, typeof(string), null, Culture));
    }

    [Fact]
    public void DateTimeToStringConverter_TodayReturnsToday()
    {
        var converter = new DateTimeToStringConverter();
        Assert.Equal("TODAY", converter.Convert(DateTime.Today, typeof(string), null, Culture));
    }

    [Fact]
    public void DateTimeToStringConverter_YesterdayReturnsYesterday()
    {
        var converter = new DateTimeToStringConverter();
        Assert.Equal("YESTERDAY", converter.Convert(DateTime.Today.AddDays(-1), typeof(string), null, Culture));
    }

    [Fact]
    public void DateTimeToStringConverter_FewDaysAgoReturnsDaysAgo()
    {
        var converter = new DateTimeToStringConverter();
        Assert.Equal("DAYS_AGO", converter.Convert(DateTime.Today.AddDays(-3), typeof(string), null, Culture));
    }

    [Fact]
    public void DateTimeToStringConverter_OneWeekAgoReturnsLastWeek()
    {
        var converter = new DateTimeToStringConverter();
        Assert.Equal("LAST_WEEK", converter.Convert(DateTime.Today.AddDays(-7), typeof(string), null, Culture));
    }

    [Fact]
    public void DateTimeToStringConverter_ThreeWeeksAgoReturnsWeeksAgo()
    {
        var converter = new DateTimeToStringConverter();
        Assert.Equal("WEEKS_AGO", converter.Convert(DateTime.Today.AddDays(-21), typeof(string), null, Culture));
    }

    [Fact]
    public void DateTimeToStringConverter_ThirtyDaysAgoReturnsLastMonth()
    {
        var converter = new DateTimeToStringConverter();
        Assert.Equal("LAST_MONTH", converter.Convert(DateTime.Today.AddDays(-30), typeof(string), null, Culture));
    }

    [Fact]
    public void DateTimeToStringConverter_SixMonthsAgoReturnsMonthsAgo()
    {
        var converter = new DateTimeToStringConverter();
        Assert.Equal("MONTHS_AGO", converter.Convert(DateTime.Today.AddDays(-180), typeof(string), null, Culture));
    }

    [Fact]
    public void DateTimeToStringConverter_LastYearReturnsLastYear()
    {
        var converter = new DateTimeToStringConverter();
        var lastYear = new DateTime(DateTime.Today.Year - 1, 1, 1);
        Assert.Equal("LAST_YEAR", converter.Convert(lastYear, typeof(string), null, Culture));
    }

    [Fact]
    public void DateTimeToStringConverter_TwoYearsAgoReturnsYearsAgo()
    {
        var converter = new DateTimeToStringConverter();
        var twoYearsAgo = new DateTime(DateTime.Today.Year - 2, 1, 1);
        Assert.Equal("YEARS_AGO", converter.Convert(twoYearsAgo, typeof(string), null, Culture));
    }

    // --- EnumIsNotDefaultConverter ---

    public enum TestEnum { Zero = 0, One = 1 }

    [Theory]
    [InlineData(TestEnum.Zero, false)]
    [InlineData(TestEnum.One, true)]
    public void EnumIsNotDefaultConverter_ReturnsExpected(TestEnum value, bool expected)
    {
        var converter = new EnumIsNotDefaultConverter();
        Assert.Equal(expected, converter.Convert(value, typeof(bool), null, Culture));
    }

    [Fact]
    public void EnumIsNotDefaultConverter_NullReturnsFalse()
    {
        var converter = new EnumIsNotDefaultConverter();
        Assert.Equal(false, converter.Convert(null, typeof(bool), null, Culture));
    }

    // --- EnumToDescriptionConverter ---

    private enum DescribedEnum
    {
        [Description("A described value")]
        Described,
        Undescribed
    }

    [Fact]
    public void EnumToDescriptionConverter_ReturnsDescriptionAttribute()
    {
        var converter = new EnumToDescriptionConverter();
        Assert.Equal("A described value", converter.Convert(DescribedEnum.Described, typeof(string), null, Culture));
    }

    [Fact]
    public void EnumToDescriptionConverter_FallsBackToEnumName()
    {
        var converter = new EnumToDescriptionConverter();
        Assert.Equal("Undescribed", converter.Convert(DescribedEnum.Undescribed, typeof(string), null, Culture));
    }

    [Fact]
    public void EnumToDescriptionConverter_NullReturnsEmpty()
    {
        var converter = new EnumToDescriptionConverter();
        Assert.Equal(string.Empty, converter.Convert(null, typeof(string), null, Culture));
    }

    // --- FilenameConverter ---

    [Theory]
    [InlineData("/path/to/file.exe", "file.exe")]
    [InlineData("file.exe", "file.exe")]
    public void FilenameConverter_ReturnsFilename(string path, string expected)
    {
        var converter = new FilenameConverter();
        Assert.Equal(expected, converter.Convert(path, typeof(string), null, Culture));
    }

    [Fact]
    public void FilenameConverter_NonStringPassesThrough()
    {
        var obj = new object();
        var converter = new FilenameConverter();
        Assert.Same(obj, converter.Convert(obj, typeof(string), null, Culture));
    }

    // --- FullScreenToIconConverter ---

    [Theory]
    [InlineData(WindowState.FullScreen, PackIconRemixIconKind.FullscreenExitLine)]
    [InlineData(WindowState.Normal, PackIconRemixIconKind.FullscreenLine)]
    [InlineData(WindowState.Maximized, PackIconRemixIconKind.FullscreenLine)]
    public void FullScreenToIconConverter_ReturnsExpectedIcon(WindowState state, PackIconRemixIconKind expected)
    {
        var converter = new FullScreenToIconConverter();
        Assert.Equal(expected, converter.Convert(state, typeof(PackIconRemixIconKind), null, Culture));
    }

    // --- IconKindToNullableIconConverter ---

    [Fact]
    public void IconKindToNullableIconConverter_NoneReturnsNull()
    {
        var converter = new IconKindToNullableIconConverter();
        Assert.Null(converter.Convert(PackIconRemixIconKind.None, typeof(object), null, Culture));
    }

    // --- IndexMatchToBrushConverter ---

    [Fact]
    public void IndexMatchToBrushConverter_MatchReturnsActiveValue()
    {
        var converter = new IndexMatchToBrushConverter();
        Assert.Equal("active", converter.Convert([0, 0, "active", "inactive"], typeof(object), null, Culture));
    }

    [Fact]
    public void IndexMatchToBrushConverter_NoMatchReturnsInactiveValue()
    {
        var converter = new IndexMatchToBrushConverter();
        Assert.Equal("inactive", converter.Convert([0, 1, "active", "inactive"], typeof(object), null, Culture));
    }

    [Fact]
    public void IndexMatchToBrushConverter_InsufficientValuesReturnsNull()
    {
        var converter = new IndexMatchToBrushConverter();
        Assert.Null(converter.Convert([0, 0], typeof(object), null, Culture));
    }

    // --- IndexMatchToDoubleConverter ---

    [Theory]
    [InlineData(0, 0, "16|8", 16.0)]
    [InlineData(0, 1, "16|8", 8.0)]
    [InlineData(0, 0, null, 8.0)]
    public void IndexMatchToDoubleConverter_ReturnsExpected(int item, int selected, string? param, double expected)
    {
        var converter = new IndexMatchToDoubleConverter();
        Assert.Equal(expected, converter.Convert([item, selected], typeof(double), param, Culture));
    }

    // --- LocalizedFallbackValueConverter ---

    [Fact]
    public void LocalizedFallbackValueConverter_NullValueReturnsParameter()
    {
        var converter = new LocalizedFallbackValueConverter();
        Assert.Equal("fallback", converter.Convert(null, typeof(string), "fallback", Culture));
    }

    [Fact]
    public void LocalizedFallbackValueConverter_NullValueNullParamReturnsEmpty()
    {
        var converter = new LocalizedFallbackValueConverter();
        Assert.Equal(string.Empty, converter.Convert(null, typeof(string), null, Culture));
    }

    [Fact]
    public void LocalizedFallbackValueConverter_NonNullValuePassesThrough()
    {
        var converter = new LocalizedFallbackValueConverter();
        Assert.Equal("value", converter.Convert("value", typeof(string), "fallback", Culture));
    }

    // --- LocalizedStringConverter ---

    [Fact]
    public void LocalizedStringConverter_ReturnsLocalizedParameter()
    {
        var converter = new LocalizedStringConverter();
        Assert.Equal("MY_KEY", converter.Convert("some_value", typeof(string), "MY_KEY", Culture));
    }

    // --- MultiSourceGameSearchResultMetadataCanDownloadToBooleanConverter ---

    [Fact]
    public void CanDownload_NonVmReturnsFalse()
    {
        var converter = new MultiSourceGameSearchResultMetadataCanDownloadToBooleanConverter();
        Assert.Equal(false, converter.Convert(null, typeof(bool), null, Culture));
    }

    [Fact]
    public void CanDownload_WithDownloadLinkReturnsTrue()
    {
        var converter = new MultiSourceGameSearchResultMetadataCanDownloadToBooleanConverter();
        var vm = new MultiSourceGameSearchResultMetadataViewModel(null!) { DownloadLink = "http://example.com/game.zip" };
        Assert.Equal(true, converter.Convert(vm, typeof(bool), null, Culture));
    }

    [Fact]
    public void CanDownload_EmptyLinkNoInstalledGameReturnsFalse()
    {
        var converter = new MultiSourceGameSearchResultMetadataCanDownloadToBooleanConverter();
        var vm = new MultiSourceGameSearchResultMetadataViewModel(null!) { DownloadLink = string.Empty };
        Assert.Equal(false, converter.Convert(vm, typeof(bool), null, Culture));
    }

    // --- NotificationTypeToColorConverter (fallback only — requires no Avalonia app) ---

    [Fact]
    public void NotificationTypeToColorConverter_NonNotificationTypeReturnsGray()
    {
        var converter = new NotificationTypeToColorConverter();
        Assert.Equal(Brushes.Gray, converter.Convert(null, typeof(IBrush), null, Culture));
    }

    // --- ObjectNotNullToBrushConverter ---

    [Fact]
    public void ObjectNotNullToBrushConverter_NullReturnsFalseValue()
    {
        var trueBrush = Substitute.For<IBrush>();
        var falseBrush = Substitute.For<IBrush>();
        var converter = new ObjectNotNullToBrushConverter { TrueValue = trueBrush, FalseValue = falseBrush };
        Assert.Same(falseBrush, converter.Convert(null, typeof(IBrush), null, Culture));
    }

    [Fact]
    public void ObjectNotNullToBrushConverter_NonNullReturnsTrueValue()
    {
        var trueBrush = Substitute.For<IBrush>();
        var falseBrush = Substitute.For<IBrush>();
        var converter = new ObjectNotNullToBrushConverter { TrueValue = trueBrush, FalseValue = falseBrush };
        Assert.Same(trueBrush, converter.Convert("something", typeof(IBrush), null, Culture));
    }

    // --- ObjectNotNullToIconConverter ---

    [Fact]
    public void ObjectNotNullToIconConverter_NullReturnsFalseValue()
    {
        var converter = new ObjectNotNullToIconConverter { TrueValue = TrueIcon, FalseValue = FalseIcon };
        Assert.Equal(FalseIcon, converter.Convert(null, typeof(PackIconRemixIconKind), null, Culture));
    }

    [Fact]
    public void ObjectNotNullToIconConverter_NonNullReturnsTrueValue()
    {
        var converter = new ObjectNotNullToIconConverter { TrueValue = TrueIcon, FalseValue = FalseIcon };
        Assert.Equal(TrueIcon, converter.Convert("something", typeof(PackIconRemixIconKind), null, Culture));
    }

    // --- StringEmptyToLocalizedStringConverter ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void StringEmptyToLocalizedStringConverter_NullOrEmptyReturnsEmptyKey(object? value)
    {
        var converter = new StringEmptyToLocalizedStringConverter { EmptyValue = "EMPTY_KEY", NotEmptyValue = "NOT_EMPTY_KEY" };
        Assert.Equal("EMPTY_KEY", converter.Convert(value, typeof(string), null, Culture));
    }

    [Fact]
    public void StringEmptyToLocalizedStringConverter_NonEmptyReturnsNotEmptyKey()
    {
        var converter = new StringEmptyToLocalizedStringConverter { EmptyValue = "EMPTY_KEY", NotEmptyValue = "NOT_EMPTY_KEY" };
        Assert.Equal("NOT_EMPTY_KEY", converter.Convert("actual content", typeof(string), null, Culture));
    }

    // --- StringNullToNullBitmapConverter ---
    // Application.Current is null in unit tests → theme is always treated as light

    [Fact]
    public void StringNullToNullBitmapConverter_NullReturnsLightValue()
    {
        var converter = new StringNullToNullBitmapConverter { LightThemeVariantValue = "LIGHT", DarkThemeVariantValue = "DARK" };
        Assert.Equal("LIGHT", converter.Convert(null, typeof(string), null, Culture));
    }

    [Fact]
    public void StringNullToNullBitmapConverter_EmptyStringReturnsLightValue()
    {
        var converter = new StringNullToNullBitmapConverter { LightThemeVariantValue = "LIGHT", DarkThemeVariantValue = "DARK" };
        Assert.Equal("LIGHT", converter.Convert("", typeof(string), null, Culture));
    }

    [Fact]
    public void StringNullToNullBitmapConverter_NonEmptyPassesThrough()
    {
        var converter = new StringNullToNullBitmapConverter { LightThemeVariantValue = "LIGHT", DarkThemeVariantValue = "DARK" };
        Assert.Equal("image.png", converter.Convert("image.png", typeof(string), null, Culture));
    }

    [Theory]
    [InlineData("LIGHT", "")]
    [InlineData("DARK", "")]
    [InlineData("image.png", "image.png")]
    public void StringNullToNullBitmapConverter_ConvertBack_ReturnsExpected(string value, string expected)
    {
        var converter = new StringNullToNullBitmapConverter { LightThemeVariantValue = "LIGHT", DarkThemeVariantValue = "DARK" };
        Assert.Equal(expected, converter.ConvertBack(value, typeof(string), null, Culture));
    }

    // --- TimeSpanToHumanReadableStringConverter ---

    [Fact]
    public void TimeSpanConverter_NullReturnsNull()
    {
        var converter = new TimeSpanToHumanReadableStringConverter();
        Assert.Null(converter.Convert(null, typeof(string), null, Culture));
    }

    [Fact]
    public void TimeSpanConverter_ZeroReturnsNa()
    {
        var converter = new TimeSpanToHumanReadableStringConverter();
        Assert.Equal("NA", converter.Convert(TimeSpan.Zero, typeof(string), null, Culture));
    }

    [Theory]
    [InlineData(1, "1_SECOND")]
    [InlineData(30, "SECONDS_FORMATTABLE")]
    public void TimeSpanConverter_SecondsReturnsExpectedKey(int seconds, string expected)
    {
        var converter = new TimeSpanToHumanReadableStringConverter();
        Assert.Equal(expected, converter.Convert(TimeSpan.FromSeconds(seconds), typeof(string), null, Culture));
    }

    [Theory]
    [InlineData(1, "1_MINUTE")]
    [InlineData(30, "MINUTES_FORMATTABLE")]
    public void TimeSpanConverter_MinutesReturnsExpectedKey(int minutes, string expected)
    {
        var converter = new TimeSpanToHumanReadableStringConverter();
        Assert.Equal(expected, converter.Convert(TimeSpan.FromMinutes(minutes), typeof(string), null, Culture));
    }

    [Theory]
    [InlineData(1, "1_HOUR")]
    [InlineData(3, "HOURS_FORMATTABLE")]
    public void TimeSpanConverter_HoursReturnsExpectedKey(int hours, string expected)
    {
        var converter = new TimeSpanToHumanReadableStringConverter();
        Assert.Equal(expected, converter.Convert(TimeSpan.FromHours(hours), typeof(string), null, Culture));
    }

    [Theory]
    [InlineData(1, "1_DAY")]
    [InlineData(3, "DAYS_FORMATTABLE")]
    public void TimeSpanConverter_DaysReturnsExpectedKey(int days, string expected)
    {
        var converter = new TimeSpanToHumanReadableStringConverter();
        Assert.Equal(expected, converter.Convert(TimeSpan.FromDays(days), typeof(string), null, Culture));
    }

    [Fact]
    public void TimeSpanConverter_MultiPartContainsAndFormattable()
    {
        var converter = new TimeSpanToHumanReadableStringConverter();
        var result = (string?)converter.Convert(new TimeSpan(1, 30, 0), typeof(string), null, Culture);
        Assert.NotNull(result);
        Assert.Contains("1_HOUR", result);
        Assert.Contains("AND_FORMATTABLE", result);
    }

    // --- UppercaseConverter ---

    [Theory]
    [InlineData("hello", "HELLO")]
    [InlineData("cafè", "CAFE")]
    [InlineData("", "")]
    public void UppercaseConverter_ConvertsToUppercase(string input, string expected)
    {
        var converter = new UppercaseConverter();
        Assert.Equal(expected, converter.Convert(input, typeof(string), null, Culture));
    }

    [Fact]
    public void UppercaseConverter_NonStringPassesThrough()
    {
        var converter = new UppercaseConverter();
        Assert.Equal(42, converter.Convert(42, typeof(string), null, Culture));
    }
}
