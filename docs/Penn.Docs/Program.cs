var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "Penn Docs - coming soon");
await app.RunAsync();
