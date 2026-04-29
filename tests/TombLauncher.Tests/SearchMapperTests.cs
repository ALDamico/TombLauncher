using NSubstitute;
using TombLauncher.Mappers;
using TombLauncher.Services;
using TombLauncher.ViewModels;

namespace TombLauncher.Tests;

public class SearchMapperTests
{
    private readonly SearchMapper _mapper = new();

    [Fact]
    public void ToDto_FromMultiSourceVm_MapsTitlePic()
    {
        var vm = new MultiSourceGameSearchResultMetadataViewModel(
            null!)
        {
            Title = "Into the Realm of Eternal Darkness",
            TitlePic = "https://example.com/titlepic.png"
        };

        var dto = _mapper.ToDto(vm);

        Assert.Equal("https://example.com/titlepic.png", dto.TitlePic);
    }

    [Fact]
    public void ToDto_FromMultiSourceVm_TitlePicNotLost_WhenEmpty()
    {
        var vm = new MultiSourceGameSearchResultMetadataViewModel(
            null!)
        {
            Title = "Some Game",
            TitlePic = ""
        };

        var dto = _mapper.ToDto(vm);

        Assert.Equal("", dto.TitlePic);
    }
}
