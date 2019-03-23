using Streamer.API.Domain.Entities;
using System.Collections.Generic;

namespace Streamer.API.Domain.Interfaces
{
    public interface IDataAccess
    {
        List<string> GetAllUserIds();
        Account GetAccountById(string id);
        Account GetAccountByGoogleId(string googleId);
        void AddNewAccount(Account account);
        Session GetSession(string session);
        void AddSession(Session sessionEntity);
        void InvalidateSession(string sessionId);
        List<Song> GetSongsForUser(string accountId);
        void DeleteSongForUser(Song song);
        void AddSongForUser(Song song, Account userAccount);
        Song GetSongByMd5HashForUser(string md5Hash, Account userAccount);
    }
}
