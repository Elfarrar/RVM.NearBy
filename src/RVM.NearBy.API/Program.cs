using RVM.NearBy.API.Auth;
using RVM.NearBy.API.Health;
using RVM.NearBy.API.Middleware;
using RVM.NearBy.API.Services;
using RVM.NearBy.Infrastructure;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateBootstrapLogger();

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new RenderedCompactJsonFormatter());

    var seqUrl = context.Configuration["Seq:ServerUrl"];
    if (!string.IsNullOrEmpty(seqUrl))
        loggerConfiguration.WriteTo.Seq(seqUrl);
});

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

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "font-src 'self'; " +
        "img-src 'self' data:; " +
        "connect-src 'self' wss:; " +
        "frame-ancestors 'none';";
    await next();
});

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
