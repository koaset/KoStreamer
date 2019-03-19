using Serilog;
using Streamer.API.Domain.Entities;
using Streamer.API.Domain.Interfaces;
using System;

namespace Streamer.API.Domain
{
    public class AccountService : IAccountService
    {
        private readonly IDataAccess dataAccess;
        private readonly ISessionService sessionService;
        private Account CachedAccount;

        public AccountService(IDataAccess dataAccess, ISessionService sessionService)
        {
            this.dataAccess = dataAccess;
            this.sessionService = sessionService;
        }

        public Account CreateAccount(GoogleUserData googleData)
        {
            var newAccountId = IdGenerationHelper.GetNewId((accountId) => dataAccess.GetAccountById(accountId) == null);

            var account = new Account
            {
                AccountId = newAccountId,
                GoogleId = googleData.Id,
                Name = googleData.Name,
                Email = googleData.Email,
                CreatedDate = DateTime.UtcNow
            };

            dataAccess.AddNewAccount(account);
            Log.Information($"Added new account. id={account.AccountId}");
            CachedAccount = account;
            return account;
        }

        public Account GetAccountByAccountId(string accountId)
        {
            if (CachedAccount?.AccountId == accountId)
            {
                return CachedAccount;
            }

            return dataAccess.GetAccountById(accountId);
        }

        public Account GetAccountByGoogleId(string googleId)
        {
            if (CachedAccount?.GoogleId == googleId)
            {
                return CachedAccount;
            }

            return dataAccess.GetAccountByGoogleId(googleId);
        }

        public Account GetAccountBySession()
        {
            var session = sessionService.GetRequestSession();
            CachedAccount = GetAccountByAccountId(session.AccountId);
            return CachedAccount;
        }
    }
}
