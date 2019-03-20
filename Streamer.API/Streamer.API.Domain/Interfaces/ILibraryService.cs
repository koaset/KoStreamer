using Microsoft.AspNetCore.Http;
using Streamer.API.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Streamer.API.Domain.Interfaces
{
    public interface ILibraryService
    {
        string UserLibraryPath();
        void AddSongToUserLibrary(Song song);
        List<Song> GetUserLibrary();
        Task<UploadSongResult> AddSongAsync(IFormFile file);
    }
}
