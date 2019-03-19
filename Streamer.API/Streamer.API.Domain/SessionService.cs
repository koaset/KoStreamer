using Microsoft.AspNetCore.Http;
using Streamer.API.Domain.Entities;
using Streamer.API.Domain.Extensions;
using Streamer.API.Domain.Interfaces;
using System;

namespace Streamer.API.Domain
{
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private Session RequestSessionCache;

        public SessionService(IHttpContextAccessor httpContextAccessor)
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
            if (RequestSessionCache != null)
            {
                return RequestSessionCache;
            }

            var sessionId = httpContextAccessor.HttpContext.Request.GetSessionId();

            if (string.IsNullOrEmpty(sessionId))
            {
                return null;
            }

            var dbValue = dataAccess.GetSession(sessionId);
            RequestSessionCache = dbValue;
            return dbValue;
        }

        public void DeleteSession()
        {
            var sessionId = httpContextAccessor.HttpContext.Request.GetSessionId();

            if (string.IsNullOrEmpty(sessionId))
            {
                return;
            }

            dataAccess.InvalidateSession(sessionId);
        }

        public Session CreateNewSession(Account account)
        {
            var newSessionId = IdGenerationHelper.GetNewId((sessionId) => dataAccess.GetSession(sessionId) == null);

            var entity = new Session
            {
                AccountId = account.AccountId,
                SessionId = newSessionId,
                CreatedDate = DateTime.UtcNow,
                Invalidated = false
            };

            dataAccess.AddSession(entity);

            return entity;
        }
    }
}
