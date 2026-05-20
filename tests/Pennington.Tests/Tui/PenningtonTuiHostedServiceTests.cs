namespace Pennington.Tests.Tui;

using Pennington.Tui;
using Shouldly;
using Xunit;

public class PenningtonTuiHostedServiceTests
{
    // Mirrors the gate in PenningtonExtensions.RunOrBuildAsync so the TUI and
    // the build entry point agree on what "build mode" looks like. If this test
    // drifts from the real check, the TUI will fire up during `dotnet run -- build`
    // and fight Kestrel for the console.

    [Fact]
    public void IsBuildMode_true_for_build_arg()
    {
        PenningtonTuiHostedService.IsBuildMode(["Host.exe", "build"]).ShouldBeTrue();
    }

    [Fact]
    public void IsBuildMode_case_insensitive()
    {
        PenningtonTuiHostedService.IsBuildMode(["Host.exe", "Build"]).ShouldBeTrue();
        PenningtonTuiHostedService.IsBuildMode(["Host.exe", "BUILD"]).ShouldBeTrue();
    }

    [Fact]
    public void IsBuildMode_false_for_dev_run()
    {
        PenningtonTuiHostedService.IsBuildMode(["Host.exe"]).ShouldBeFalse();
        PenningtonTuiHostedService.IsBuildMode(["Host.exe", "serve"]).ShouldBeFalse();
        PenningtonTuiHostedService.IsBuildMode(["Host.exe", "--urls", "http://localhost:5000"]).ShouldBeFalse();
    }

    [Fact]
    public void IsBuildMode_false_for_empty_args()
    {
        PenningtonTuiHostedService.IsBuildMode([]).ShouldBeFalse();
    }
}