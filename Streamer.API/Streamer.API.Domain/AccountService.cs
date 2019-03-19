using Serilog;
using Streamer.API.Domain.Entities;
using Streamer.API.Domain.Interfaces;
using System;

namespace Streamer.API.Domain
{
    public class AccountService : IAccountService
    {
        private readonly IDataAccess dataAccess;

        public AccountService(IDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
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

        public Account GetAccountByAccountId(string accountId)
        {
            return dataAccess.GetAccountById(accountId);
        }

        public Account GetAccountByGoogleId(string googleId)
        {
            return dataAccess.GetAccountByGoogleId(googleId);
        }
    }
}
