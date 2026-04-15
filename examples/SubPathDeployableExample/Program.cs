using Pennington.DocSite;
using SubPathDeployableExample;

var builder = WebApplication.CreateBuilder(args);

// Deliberately tiny DocSite host. The teaching surface for how-to §2.4 isn't
// this code — it's the sibling deployment fixtures (`.github/workflows/deploy.yml`,
// `staticwebapp.config.json`, `netlify.toml`, `nginx.conf`, `web.config`).
// Keeping the host minimal makes `BaseUrlHtmlRewriter` behaviour observable
// without noise when the site is built with a sub-path `baseUrl`.
builder.Services.AddDocSite(ServiceConfiguration.BuildDocSiteOptions);

var app = builder.Build();

app.UseDocSite();

// `RunDocSiteAsync` delegates to `RunOrBuildAsync`, so the same host serves
// live (`dotnet run`) and writes static HTML (`dotnet run -- build [baseUrl]`).
// The first positional arg after `build` is the base URL; pass `/my-sub-path`
// to see BaseUrlHtmlRewriter prefix every anchor, asset, and script.
await app.RunDocSiteAsync(args);