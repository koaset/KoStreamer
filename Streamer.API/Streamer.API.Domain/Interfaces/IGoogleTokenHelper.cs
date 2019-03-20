using Streamer.API.Domain.Entities;
using System.Threading.Tasks;

namespace Streamer.API.Domain.Interfaces
{
    public interface IGoogleTokenHelper
    {
        Task<GoogleUserData> ValidateGoogleTokenAndGetUserDataAsync(string token);
    }
}
