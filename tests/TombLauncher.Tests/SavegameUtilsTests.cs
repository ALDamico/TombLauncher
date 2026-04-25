using TombLauncher.Core.Utils;

namespace TombLauncher.Tests;

public class SavegameUtilsTests
{
    // --- GetSlotNumber ---

    [Theory]
    [InlineData("savegame.0", 1)]
    [InlineData("savegame.4", 5)]
    [InlineData("savegame.15", 16)]
    public void GetSlotNumber_NumericExtension_ReturnsSlotPlusOne(string filename, int expected)
    {
        Assert.Equal(expected, SavegameUtils.GetSlotNumber(filename));
    }

    [Theory]
    [InlineData("savegame.sav")]
    [InlineData("savegame")]
    [InlineData("savegame.")]
    public void GetSlotNumber_NonNumericExtension_ReturnsMinusOne(string filename)
    {
        Assert.Equal(-1, SavegameUtils.GetSlotNumber(filename));
    }

    // --- GetTr1xSlotNumber ---

    [Theory]
    [InlineData("save_1.dat", 1)]
    [InlineData("save_10.dat", 10)]
    public void GetTr1xSlotNumber_ValidFormat_ReturnsSlotNumber(string filename, int expected)
    {
        Assert.Equal(expected, SavegameUtils.GetTr1xSlotNumber(filename));
    }

    [Theory]
    [InlineData("noseparator.dat")]
    [InlineData("save_abc.dat")]
    public void GetTr1xSlotNumber_InvalidFormat_ReturnsMinusOne(string filename)
    {
        Assert.Equal(-1, SavegameUtils.GetTr1xSlotNumber(filename));
    }
}
