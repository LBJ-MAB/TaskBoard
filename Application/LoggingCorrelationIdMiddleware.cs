using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Context;

namespace Application;

public class LoggingCorrelationIdMiddleware
{
    private RequestDelegate _next;
    private HttpContext _context;
    
    public LoggingCorrelationIdMiddleware(RequestDelegate next, HttpContext context)
    {
        _next = next;
        _context = context;
    }

    public async Task InvokeAsync()
    {
        _context.Request.Headers.TryGetValue("Correlation-Id-Header", out StringValues correlationIds);
        var correlationId = correlationIds.FirstOrDefault() ?? Guid.NewGuid().ToString();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(_context);
        }
    }
}