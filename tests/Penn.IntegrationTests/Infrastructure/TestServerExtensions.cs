namespace Penn.IntegrationTests.Infrastructure;

using System.Net;

public static class TestServerExtensions
{
    public static async Task ShouldReturnSuccessWithContent(this HttpResponseMessage response, string expectedContent)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.OK,
            $"Expected 200 OK but got {(int)response.StatusCode} for {response.RequestMessage?.RequestUri}");

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain(expectedContent,
            customMessage: $"Response from {response.RequestMessage?.RequestUri} did not contain expected content");
    }
}
