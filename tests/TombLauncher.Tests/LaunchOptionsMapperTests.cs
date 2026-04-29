using NSubstitute;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Mappers;
using TombLauncher.Services;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Tests;

public class LaunchOptionsMapperTests
{
    private readonly LaunchOptionsMapper _mapper = new();

    // ── compatibility fields ──────────────────────────────────────────────────

    [Fact]
    public void ToDto_MapsCompatibilityPrefixPath()
    {
        var vm = MakeVm(v => v.CompatibilityPrefixPath = "/home/user/.wine-tr1");
        Assert.Equal("/home/user/.wine-tr1", _mapper.ToDto(vm).CompatibilityPrefixPath);
    }

    [Fact]
    public void ToDto_MapsCompatibilityTool()
    {
        var vm = MakeVm(v => v.CompatibilityTool = CompatibilityTool.Proton);
        Assert.Equal(CompatibilityTool.Proton, _mapper.ToDto(vm).CompatibilityTool);
    }

    [Fact]
    public void ToDto_MapsCompatibilityToolPath()
    {
        var vm = MakeVm(v => v.CompatibilityToolPath = "/opt/proton/proton");
        Assert.Equal("/opt/proton/proton", _mapper.ToDto(vm).CompatibilityToolPath);
    }

    [Fact]
    public void ToDto_MapsExtraEnvVars()
    {
        var envVars = new List<EnvironmentVariableDto>
        {
            new() { GameId = 1, VariableName = "DXVK_HUD", VariableValue = "fps" }
        };
        var vm = MakeVm(v => v.ExtraEnvVars = envVars);

        var dto = _mapper.ToDto(vm);

        Assert.Single(dto.ExtraEnvVars);
        Assert.Equal("DXVK_HUD", dto.ExtraEnvVars[0].VariableName);
    }

    // ── executable fields ─────────────────────────────────────────────────────

    [Fact]
    public void ToDto_SetupExecutable_NullWhenNotSupported()
    {
        var vm = MakeVm(v =>
        {
            v.SupportsSetup = false;
            v.SetupExecutable = "setup.exe";
        });
        Assert.Null(_mapper.ToDto(vm).SetupExecutable);
    }

    [Fact]
    public void ToDto_SetupExecutable_PresentWhenSupported()
    {
        var vm = MakeVm(v =>
        {
            v.SupportsSetup = true;
            v.SetupExecutable = "setup.exe";
        });
        Assert.NotNull(_mapper.ToDto(vm).SetupExecutable);
        Assert.Equal("setup.exe", _mapper.ToDto(vm).SetupExecutable!.FileName);
    }

    [Fact]
    public void ToDto_CommunitySetupExecutable_NullWhenNotSupported()
    {
        var vm = MakeVm(v =>
        {
            v.SupportsCustomSetup = false;
            v.CustomSetupExecutable = "community.exe";
        });
        Assert.Null(_mapper.ToDto(vm).CommunitySetupExecutable);
    }

    // ── helper ────────────────────────────────────────────────────────────────

    private static LaunchOptionsViewModel MakeVm(Action<LaunchOptionsViewModel>? configure = null)
    {
        var vm = new LaunchOptionsViewModel(null!, Substitute.For<IPlatformSpecificFeatures>());
        vm.GameExecutable = "game.exe";
        configure?.Invoke(vm);
        return vm;
    }
}
