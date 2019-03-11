using System.Threading.Tasks;

namespace Library.Server.Interfaces
{
    public interface IStreamerApiAccess
    {
        void AuthenticateLibraryAsync();
        Task<bool> IsSessionValidAsync(string session);
    }
}
