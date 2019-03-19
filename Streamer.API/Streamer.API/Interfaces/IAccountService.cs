using Streamer.API.Entities;

namespace Streamer.API.Interfaces
{
    public interface IAccountService
    {
        Account GetAccountByAccountId(string accountId);
        Account GetAccountByGoogleId(string stringGoogleId);
        Account CreateAccount(GoogleUserData googleData);
    }
}
