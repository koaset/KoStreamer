using Streamer.API.Entities;

namespace Streamer.API.Interfaces
{
    public interface IDataAccess
    {
        Account GetAccountById(string id);
        Account GetAccountByGoogleId(string googleId);
        void AddNewAccount(Account account);
        Session GetSession(string session);
        void AddSession(Session sessionEntity);
        void InvalidateSession(string sessionId);
    }
}
