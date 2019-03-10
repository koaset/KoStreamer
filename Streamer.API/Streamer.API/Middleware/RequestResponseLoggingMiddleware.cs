using Microsoft.AspNetCore.Http;
using Serilog;
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
            var logLine = $"timestamp={DateTime.UtcNow:yyyy-MM-dd:hh:mm:ss} method={request.Method} " +
                $"url={request.Scheme}://{request.Host}{request.Path} query={request.QueryString} " +
                $"status={responseStatus} response_time={GetRequestMs(context)}";
            Log.Information(logLine);
        }

        private string GetRequestMs(HttpContext context)
        {
            var sw = (System.Diagnostics.Stopwatch)context.Items[RequestTimerStartMiddleware.RequestTimeContextKey];
            sw.Stop();
            return sw.ElapsedMilliseconds.ToString();
        }
    }
}