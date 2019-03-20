using Streamer.API.Domain.Entities;
using System.Collections.Generic;

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
        List<Song> GetSongsForUser(Account userAccount);
        void AddSongForUser(Song song, Account userAccount);
        Song GetSongByMd5HashForUser(string md5Hash, Account userAccount);
    }
}
