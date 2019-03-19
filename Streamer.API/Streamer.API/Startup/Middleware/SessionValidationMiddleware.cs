using Microsoft.AspNetCore.Http;
using Streamer.API.Domain.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Streamer.API.Startup.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISessionService sessionValidator;

        public SessionValidationMiddleware(RequestDelegate next, ISessionService sessionValidator)
        {
            _next = next;
            this.sessionValidator = sessionValidator;
        }

        public async Task Invoke(HttpContext context)
        {
            var routesWithNoValidation = new[] { "/ping", "/session" };
            if (routesWithNoValidation.Any(route => context.Request.Path.Value.StartsWith(route)))
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
