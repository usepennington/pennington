namespace Pennington.Tests.Cli;

using Pennington.Cli;
using Shouldly;
using Xunit;

public class PenningtonCliTests
{
    [Fact]
    public void No_args_is_serve()
    {
        var cli = new PenningtonCli([]);
        cli.Mode.ShouldBe(PenningtonRunMode.Serve);
        cli.IsHeadlessOneShot.ShouldBeFalse();
        cli.WritesOutput.ShouldBeFalse();
    }

    [Fact]
    public void Build_verb_is_build()
    {
        var cli = new PenningtonCli(["build"]);
        cli.Mode.ShouldBe(PenningtonRunMode.Build);
        cli.WritesOutput.ShouldBeTrue();
        cli.IsHeadlessOneShot.ShouldBeTrue();
    }

    [Fact]
    public void Build_verb_is_case_insensitive()
    {
        new PenningtonCli(["BUILD"]).Mode.ShouldBe(PenningtonRunMode.Build);
        new PenningtonCli(["Build"]).Mode.ShouldBe(PenningtonRunMode.Build);
    }

    [Fact]
    public void Build_with_options_and_positionals_is_build()
    {
        new PenningtonCli(["build", "/sub", "dist"]).Mode.ShouldBe(PenningtonRunMode.Build);
        new PenningtonCli(["build", "--base-url=/x", "--output", "dist"]).Mode.ShouldBe(PenningtonRunMode.Build);
    }

    [Fact]
    public void Diag_verb_is_diag()
    {
        var cli = new PenningtonCli(["diag", "toc"]);
        cli.Mode.ShouldBe(PenningtonRunMode.Diag);
        cli.IsHeadlessOneShot.ShouldBeTrue();
        cli.WritesOutput.ShouldBeFalse();
    }

    [Fact]
    public void Unknown_verb_is_serve()
    {
        new PenningtonCli(["frobnicate"]).Mode.ShouldBe(PenningtonRunMode.Serve);
    }

    [Fact]
    public void Host_and_test_args_are_serve()
    {
        // Stray host args (dotnet run --urls ...) and test-runner args must not be misread as a verb.
        new PenningtonCli(["--urls", "http://localhost:5000"]).Mode.ShouldBe(PenningtonRunMode.Serve);
        new PenningtonCli(["serve"]).Mode.ShouldBe(PenningtonRunMode.Serve);
    }
}
