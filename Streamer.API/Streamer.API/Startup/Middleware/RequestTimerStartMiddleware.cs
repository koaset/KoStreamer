﻿using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Streamer.API.Startup.Middleware
{
    public class RequestTimerStartMiddleware
    {
        internal static readonly string RequestTimeContextKey = "request_timer";

        private readonly RequestDelegate _next;
        
        public RequestTimerStartMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Items[RequestTimeContextKey] = System.Diagnostics.Stopwatch.StartNew();
            await _next(context);
        }
    }
}
