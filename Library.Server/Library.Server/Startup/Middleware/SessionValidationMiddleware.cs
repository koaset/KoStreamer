using Library.Server.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Library.Server.Startup.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private IStreamerApiAccess streamerApi;


        public SessionValidationMiddleware(RequestDelegate next, IStreamerApiAccess streamerApi)
        {
            _next = next;
            this.streamerApi = streamerApi;
        }

        public async Task Invoke(HttpContext context)
        {
            var session = context.Request.Headers["X-Session"];

            if (string.IsNullOrEmpty(session) && context.Request.Query.ContainsKey("sessionId"))
            {
                context.Request.Query.TryGetValue("sessionId", out session);
            }

            var secret = context.Request.Headers["X-UserSecret"];

            var isSessionValid = streamerApi.IsSessionValidAsync(session).Result;

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
