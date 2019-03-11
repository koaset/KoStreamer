using Streamer.API.Entities;

namespace Streamer.API.Interfaces
{
    public interface ISessionValidator
    {
        bool RequestSessionIsValid();
        Session GetRequestSession();
        bool SessionIsValid(Session sessionEntity);
        void DeleteSession();
    }
}
