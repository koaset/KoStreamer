﻿using Microsoft.AspNetCore.Http;
using Streamer.API.Entities;
using Streamer.API.Helpers;
using Streamer.API.Interfaces;
using System;

namespace Streamer.API.Services
{
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

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
            var sessionId = GetSessionFromHeader();

            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = GetSessionFromQuery();
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                return null;
            }

            return dataAccess.GetSession(sessionId);
        }

        public void DeleteSession()
        {
            var sessionId = GetSessionFromHeader();
            dataAccess.InvalidateSession(sessionId);
        }

        private string GetSessionFromHeader()
        {
            var request = httpContextAccessor.HttpContext.Request;
            return request.Headers.TryGetValue("X-Session", out var sessionId) ? (string)sessionId : null;
        }

        private string GetSessionFromQuery()
        {
            var request = httpContextAccessor.HttpContext.Request;
            return request.Query.TryGetValue("sessionId", out var sessionId) ? (string)sessionId : null;
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