using Microsoft.AspNetCore.Http;
using Pennington.Generation;
using Pennington.Infrastructure;
using Pennington.Routing;

namespace Pennington.Tests.Infrastructure;

public class BaseUrlCssResponseProcessorTests
{
    private static OutputOptions WithBase(string baseUrl) => new()
    {
        OutputDirectory = new FilePath("output"),
        BaseUrl = new UrlPath(baseUrl),
    };

    private static DefaultHttpContext CssContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "text/css";
        return ctx;
    }

    [Fact]
    public async Task RewritesUnquotedRootRelativeFontUrl()
    {
        var processor = new BaseUrlCssResponseProcessor(WithBase("/pennington/"));
        var input = "@font-face { src: url(/fonts/lexend.woff2) format(\"woff2\"); }";

        var result = await processor.ProcessAsync(input, CssContext());

        result.ShouldContain("url(/pennington/fonts/lexend.woff2)");
        result.ShouldNotContain("url(/fonts/");
    }

    [Fact]
    public async Task PreservesQuotingStyle()
    {
        var processor = new BaseUrlCssResponseProcessor(WithBase("/sub"));
        var input = """
            a { background: url('/img/a.png'); }
            b { background: url("/img/b.png"); }
            c { background: url(/img/c.png); }
            """;

        var result = await processor.ProcessAsync(input, CssContext());

        result.ShouldContain("url('/sub/img/a.png')");
        result.ShouldContain("url(\"/sub/img/b.png\")");
        result.ShouldContain("url(/sub/img/c.png)");
    }

    [Fact]
    public async Task LeavesAbsoluteAndProtocolRelativeAlone()
    {
        var processor = new BaseUrlCssResponseProcessor(WithBase("/sub"));
        var input = """
            a { src: url(https://cdn.example/font.woff2); }
            b { src: url(//cdn.example/font.woff2); }
            c { src: url(./relative.woff2); }
            d { src: url(data:font/woff2;base64,abc==); }
            """;

        var result = await processor.ProcessAsync(input, CssContext());

        result.ShouldContain("url(https://cdn.example/font.woff2)");
        result.ShouldContain("url(//cdn.example/font.woff2)");
        result.ShouldContain("url(./relative.woff2)");
        result.ShouldContain("url(data:font/woff2;base64,abc==)");
    }

    [Fact]
    public void ShouldProcess_FalseWhenBaseUrlIsRoot()
    {
        var processor = new BaseUrlCssResponseProcessor(WithBase("/"));
        processor.ShouldProcess(CssContext()).ShouldBeFalse();
    }

    [Fact]
    public void ShouldProcess_FalseForHtmlContentType()
    {
        var processor = new BaseUrlCssResponseProcessor(WithBase("/sub"));
        var ctx = new DefaultHttpContext();
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "text/html";

        processor.ShouldProcess(ctx).ShouldBeFalse();
    }

    [Fact]
    public void ShouldProcess_TrueForCssContentTypeWithCharset()
    {
        var processor = new BaseUrlCssResponseProcessor(WithBase("/sub"));
        var ctx = new DefaultHttpContext();
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "text/css; charset=utf-8";

        processor.ShouldProcess(ctx).ShouldBeTrue();
    }
}