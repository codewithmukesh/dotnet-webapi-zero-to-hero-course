using HttpResilience.FlakyApi;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapFlakyEndpoints();

app.Run();
