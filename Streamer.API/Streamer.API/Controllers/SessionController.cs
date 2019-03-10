using Microsoft.AspNetCore.Mvc;
using Serilog;
using Streamer.API.Entities;
using Streamer.API.Interfaces;
using Streamer.API.Models;
using System;
using System.Threading.Tasks;

namespace Streamer.API.Controllers
{
    [Route("session")]
    [Produces("application/json")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly IDataAccess dataAccess = new DataAccess();

        [HttpPost("googleAuth")]
        public async Task<ActionResult<LoginResponseModel>> PostGoogleLogin(GoogleLoginRequestModel model)
        {
            var googleData = await GoogleTokenHelper.ValidateGoogleTokenAndGetUserData(model.IdToken);

            if (googleData == null)
            {
                return Unauthorized();
            }

            var account = dataAccess.GetAccountByGoogleId(googleData.Id);

            if (account == null)
            {
                account = CreateAccount(model, googleData);
            }

            var sessionEntity = CreateNewSession(account);
            dataAccess.AddSession(sessionEntity);

            return new LoginResponseModel { Session = sessionEntity.SessionId };
        }

        private Session CreateNewSession(Account account)
        {
            return new Session
            {
                AccountId = account.AccountId,
                SessionId = GetValidNewSessionId(),
                CreatedDate = DateTime.UtcNow,
                Invalidated = false
            };
        }

        private string GetValidNewSessionId()
        {
            return GetNewId(idIsValid: (sessionId) => dataAccess.GetSession(sessionId) == null);
        }

        private Account CreateAccount(GoogleLoginRequestModel model, GoogleUserData googleData)
        {
            var account = new Account
            {
                AccountId = GetValidNewAccountId(),
                GoogleId = googleData.Id,
                Name = googleData.Name,
                Email = googleData.Email,
                CreatedDate = DateTime.UtcNow
            };

            dataAccess.AddNewAccount(account);
            Log.Information($"Added new account. id={account.AccountId}");
            return account;
        }

        private string GetValidNewAccountId()
        {
            return GetNewId(idIsValid: (accountId) => dataAccess.GetAccountById(accountId) == null);
        }

        private string GetNewId(Func<string, bool> idIsValid)
        {
            string newId;
            var maxTries = 10;
            var i = 0;
            do
            {
                newId = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 10);
                if (++i >= maxTries)
                {
                    throw new Exception();
                }
                    
            } while (!idIsValid(newId));

            return newId;
        }

        [HttpGet("")]
        public ActionResult ValidateSession(string sessionId)
        {
            var sessionExpiryHours = 2;

            var sessionEntity = dataAccess.GetSession(sessionId);

            if (sessionEntity == null || 
                sessionEntity.Invalidated ||
                sessionEntity.CreatedDate < DateTime.UtcNow.AddHours(-sessionExpiryHours))
            {
                return Unauthorized();
            }

            return NoContent();
        }

        [HttpDelete("")]
        public ActionResult DeleteSession(string sessionId)
        {
            dataAccess.InvalidateSession(sessionId);
            return NoContent();
        }
    }
}
