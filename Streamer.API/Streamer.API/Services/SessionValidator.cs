using Microsoft.AspNetCore.Http;
using Streamer.API.Entities;
using Streamer.API.Interfaces;
using System;
using System.Linq;

namespace Streamer.API.Services
{
    public class SessionValidator : ISessionValidator
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public SessionValidator(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        private readonly IDataAccess dataAccess = new DataAccess();

        public bool RequestSessionIsValid()
        {
            var sessionEntity = GetRequestSession();
            return SessionIsValid(sessionEntity);
        }

        public bool SessionIsValid(Session sessionEntity)
        {
            var sessionExpiryHours = 24;
            if (sessionEntity == null ||
                sessionEntity.Invalidated ||
                sessionEntity.CreatedDate < DateTime.UtcNow.AddHours(-sessionExpiryHours))
            {
                return false;
            }
            return true;
        }

        public Session GetRequestSession()
        {
            var sessionId = GetSessionFromHeader();

            if (string.IsNullOrEmpty(sessionId))
            {
                return null;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = dataAccess.GetSession(sessionId);
            sw.Stop();
            return result;
        }

        public void DeleteSession()
        {
            var sessionId = GetSessionFromHeader();
            dataAccess.InvalidateSession(sessionId);
        }

        private string GetSessionFromHeader()
        {
            string sessionId = null;
            var request = httpContextAccessor.HttpContext.Request;

            if (request.Headers.ContainsKey("X-Session"))
            {
                sessionId = request.Headers["X-Session"].First();
            }

            return sessionId;
        }
    }
}
