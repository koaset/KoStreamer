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
            return account;
        }

        public Account GetTestAccount()
        {
            var accountId = "testacc";
            var existingAccount = dataAccess.GetAccountById(accountId);

            if (existingAccount != null)
                return existingAccount;

            var account = new Account
            {
                AccountId = accountId,
                GoogleId = "dummyvalue",
                Name = "test",
                Email = "test",
                CreatedDate = DateTime.UtcNow
            };

            dataAccess.AddNewAccount(account);
            Log.Information($"Added new account. id={account.AccountId}");
            return account;
        }

        public Account GetAccountByAccountId(string accountId)
        {
            return dataAccess.GetAccountById(accountId);
        }

        public Account GetAccountByGoogleId(string googleId)
        {
            return dataAccess.GetAccountByGoogleId(googleId);
        }

        public Account GetAccountBySession()
        {
            var session = sessionService.GetRequestSession();
            return GetAccountByAccountId(session.AccountId);
        }
    }
}
