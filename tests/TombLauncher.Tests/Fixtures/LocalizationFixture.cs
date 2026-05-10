using System.Globalization;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TombLauncher.Contracts.Localization;

namespace TombLauncher.Tests.Fixtures;

[CollectionDefinition("Localization")]
public class LocalizationCollection : ICollectionFixture<LocalizationFixture> { }

public class LocalizationFixture
{
    public LocalizationFixture()
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
}
