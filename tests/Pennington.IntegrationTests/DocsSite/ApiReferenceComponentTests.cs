namespace Pennington.IntegrationTests.DocsSite;

using Infrastructure;

public class ApiReferenceComponentTests : IClassFixture<DocsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiReferenceComponentTests(DocsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(Skip = "reference/options/roslyn-options page is in draft; un-skip when it's published")]
    public async Task RoslynOptionsPage_ApiMemberTable_Renders_Both_Properties()
    {
        var response = await _client.GetAsync("/reference/options/roslyn-options/", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldNotContain("Page not found");
        content.ShouldNotContain("diag-error");
        content.ShouldContain("SolutionPath");
        content.ShouldContain("ProjectFilter");
    }

    [Fact(Skip = "reference/options/roslyn-options page is in draft; un-skip when it's published")]
    public async Task RoslynOptionsPage_ApiMemberTable_Pulls_Description_From_XmlDoc()
    {
        var response = await _client.GetAsync("/reference/options/roslyn-options/", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        content.ShouldContain("Path to .sln or .slnx file");
    }

    [Fact(Skip = "reference/options/roslyn-options page is in draft; un-skip when it's published")]
    public async Task RoslynOptionsPage_ApiMemberTable_Uses_Table_Headers()
    {
        var response = await _client.GetAsync("/reference/options/roslyn-options/", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        content.ShouldContain("<th>Name</th>");
        content.ShouldContain("<th>Type</th>");
        content.ShouldContain("<th>Default</th>");
        content.ShouldContain("<th>Description</th>");
    }

    [Fact(Skip = "reference/options/roslyn-options page is in draft; un-skip when it's published")]
    public async Task RoslynOptionsPage_Renders_Nullable_Type_Symbol()
    {
        var response = await _client.GetAsync("/reference/options/roslyn-options/", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        content.ShouldContain("string?");
    }
}