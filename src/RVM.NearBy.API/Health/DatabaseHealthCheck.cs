using Microsoft.Extensions.Diagnostics.HealthChecks;
using RVM.NearBy.Infrastructure.Data;

namespace RVM.NearBy.API.Health;

public class DatabaseHealthCheck(NearByDbContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext ctx, CancellationToken ct = default)
    {
        try
        {
            await context.Database.CanConnectAsync(ct);
            return HealthCheckResult.Healthy("Database connection OK");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}
