namespace RVM.NearBy.API.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        const string header = "X-Correlation-Id";
        if (!context.Request.Headers.ContainsKey(header))
            context.Request.Headers[header] = Guid.NewGuid().ToString();

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[header] = context.Request.Headers[header].ToString();
            return Task.CompletedTask;
        });

        await next(context);
    }
}
