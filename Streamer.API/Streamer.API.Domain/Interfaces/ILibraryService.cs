using Streamer.API.Domain.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Streamer.API.Domain.Interfaces
{
    public interface ILibraryService
    {
        string UserLibraryPath();
        string UserLibraryPath(string accountId);
        string MediaFolder();
        void AddSongToUserLibrary(Song song);
        List<Song> GetUserLibrary();
        Task<UploadSongResult> AddSongAsync(MemoryStream file, string fileName);
    }
}
