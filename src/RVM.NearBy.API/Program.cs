using RVM.NearBy.API.Auth;
using RVM.NearBy.API.Health;
using RVM.NearBy.API.Middleware;
using RVM.NearBy.API.Services;
using RVM.NearBy.Infrastructure;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<FeedService>();

builder.Services.AddAuthentication(ApiKeyAuthOptions.Scheme)
    .AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler>(ApiKeyAuthOptions.Scheme, options =>
    {
        options.ApiKey = builder.Configuration["Auth:ApiKey"] ?? "dev-key";
    });
builder.Services.AddAuthorization();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RVM.NearBy.Infrastructure.Data.NearByDbContext>();
    await db.Database.EnsureCreatedAsync();
}

var pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrEmpty(pathBase))
    app.UsePathBase(pathBase);

app.UseStaticFiles();
app.UseAntiforgery();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorComponents<RVM.NearBy.API.Components.App>()
    .AddInteractiveServerRenderMode();
app.MapOpenApi();

app.Run();

public partial class Program { }
