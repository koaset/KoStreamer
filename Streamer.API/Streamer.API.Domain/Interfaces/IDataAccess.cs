using Streamer.API.Domain.Entities;

namespace Streamer.API.Domain.Interfaces
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
