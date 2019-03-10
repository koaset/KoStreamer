using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Library.Server.Startup.Middleware
{
    public class SessionValidatorMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidatorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var session = context.Request.Headers["X-Session"];

            if (!IsValid(session))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid session");
                return;
            }

            await _next(context);
        }

        public bool IsValid(string session)
        {
            var client = new HttpClient
            {
                //BaseAddress = new Uri("https://dev.player.koaset.com/api"),
                BaseAddress = new Uri("https://localhost:44361/")
            };

            client.DefaultRequestHeaders.Add("X-Session", session);

            var result = client.GetAsync($"session").Result;

            if (result.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                Log.Information("Invalid session!");
                return false;
            }

            return true;
        }
    }
}
