using Microsoft.Extensions.Configuration;
using Streamer.API.Domain.Entities;
using Streamer.API.Domain.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Streamer.API.Domain
{
    public class LibraryService : ILibraryService
    {
        private readonly string mediaFolder;
        private readonly Account userAccount;
        private readonly IDataAccess dataAccess;

        public LibraryService(IConfiguration configuration, Account userAccount, IDataAccess dataAccess)
        {
            mediaFolder = configuration.GetValue<string>("MediaFolder");
            this.userAccount = userAccount;
            this.dataAccess = dataAccess;
        }

        public string UserLibraryPath() => $"{mediaFolder}{Path.DirectorySeparatorChar}{userAccount.AccountId}";

        public void AddSongToUserLibrary(Song song)
        {
            dataAccess.AddSongForUser(song, userAccount);
        }

        public List<Song> GetUserLibrary()
        {
            return dataAccess.GetSongsForUser(userAccount);
        }
    }
}
