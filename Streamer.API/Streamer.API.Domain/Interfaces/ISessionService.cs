using Streamer.API.Domain.Entities;

namespace Streamer.API.Domain.Interfaces
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
