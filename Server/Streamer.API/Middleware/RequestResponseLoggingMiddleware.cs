using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Streamer.API.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            await _next(context);
            var responseStatus = context.Response.StatusCode;
            var logLine = $"{DateTime.UtcNow:yyyy-MM-dd:hh:mm:ss} {request.Method} {request.Scheme} {request.Host}{request.Path} {request.QueryString} {responseStatus}";
            Console.WriteLine(logLine);
        }
    }
}