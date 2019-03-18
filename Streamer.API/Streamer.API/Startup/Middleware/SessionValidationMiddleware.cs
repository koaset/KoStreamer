using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Streamer.API.Interfaces;
using System.Threading.Tasks;

namespace Streamer.API.Startup.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISessionValidator sessionValidator;

        public SessionValidationMiddleware(RequestDelegate next, ISessionValidator sessionValidator)
        {
            _next = next;
            this.sessionValidator = sessionValidator;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value.StartsWith("/ping") || context.Request.Path.Value.Contains("/session"))
            {
                await _next(context);
                return;
            }

            var isSessionValid = sessionValidator.RequestSessionIsValid();

            if (!isSessionValid)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid session");
                return;
            }

            await _next(context);
        }
    }
}
