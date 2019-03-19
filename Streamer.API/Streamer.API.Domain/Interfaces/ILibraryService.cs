using Streamer.API.Domain.Entities;
using System.Collections.Generic;

namespace Streamer.API.Domain.Interfaces
{
    public interface ILibraryService
    {
        string UserLibraryPath();
        void AddSongToUserLibrary(Song song);
        List<Song> GetUserLibrary();
    }
}
