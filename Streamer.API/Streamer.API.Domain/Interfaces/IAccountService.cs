using Streamer.API.Domain.Entities;

namespace Streamer.API.Domain.Interfaces
{
    public interface IAccountService
    {
        Account GetAccountByAccountId(string accountId);
        Account GetAccountByGoogleId(string stringGoogleId);
        Account CreateAccount(GoogleUserData googleData);
    }
}
