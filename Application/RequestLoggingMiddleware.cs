using Microsoft.AspNetCore.Http;

namespace Domain;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // log request info
        Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");

        // invoke next middleware
        await _next(context);

        // log response info
        Console.WriteLine($"Status Code: {context.Response.StatusCode}");
    }
}