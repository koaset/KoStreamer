using Streamer.API.Entities;

namespace Streamer.API.Interfaces
{
    public interface ISessionService
    {
        bool RequestSessionIsValid();
        Session GetRequestSession();
        bool SessionIsValid(Session sessionEntity);
        Session CreateNewSession(Account account);
        void DeleteSession();
    }
}
