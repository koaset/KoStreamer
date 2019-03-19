using Microsoft.AspNetCore.Http;

namespace Streamer.API.Domain.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string GetSessionId(this HttpRequest request)
        {
            var sessionId = request.Headers.TryGetValue("X-Session", out var headerResult) ? (string)headerResult : null;
            
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = request.Query.TryGetValue("sessionId", out var queryResult) ? (string)queryResult : null;
            }

            return sessionId;
        }
    }
}
