using Microsoft.Extensions.Configuration;
using Streamer.API.Domain.Entities;
using Streamer.API.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Streamer.API.Domain
{
    public class LibraryService : ILibraryService
    {
        public static string[] AllowedExtensions = { ".mp3", ".m4a", ".wma", ".aac", ".flac" };
        private readonly string mediaFolder;
        private readonly IAccountService accountService;
        private readonly IDataAccess dataAccess;

        public LibraryService(IConfiguration configuration, IAccountService accountService, IDataAccess dataAccess)
        {
            mediaFolder = configuration.GetValue<string>("MediaFolder");
            Directory.CreateDirectory(MediaFolder());
            this.accountService = accountService;
            this.dataAccess = dataAccess;
        }

        public string MediaFolder() => mediaFolder;
        public string UserLibraryPath() => $"{mediaFolder}{Path.DirectorySeparatorChar}{accountService.GetAccountBySession().AccountId}";
        public string UserLibraryPath(string accountId) => $"{mediaFolder}{Path.DirectorySeparatorChar}{accountId}";

        public void AddSongToUserLibrary(Song song)
        {
            dataAccess.AddSongForUser(song, accountService.GetAccountBySession());
        }

        public List<Song> GetUserLibrary()
        {
            return dataAccess.GetSongsForUser(accountService.GetAccountBySession().AccountId);
        }

        public async Task<UploadSongResult> AddSongAsync(MemoryStream stream, string fileName)
        {
            Directory.CreateDirectory(UserLibraryPath());
            // Check user allowed to add size

            var extension = GetFileExtension(fileName);

            if (extension == null || !AllowedExtensions.Contains(extension))
            {
                return new UploadSongResult { File = fileName };
            }

            var userDirectoryPath = UserLibraryPath();
            Directory.CreateDirectory(userDirectoryPath);

            var songId = Guid.NewGuid().ToString();
            var filePath = $"{userDirectoryPath}{Path.DirectorySeparatorChar}{songId}{extension}";

            string md5Hash;
            long streamLength = stream.Length;

            using (stream)
            {

                stream.Position = 0;

                using (var md5 = MD5.Create())
                {
                    md5Hash = GetStringFromMd5Bytes(md5.ComputeHash(stream));
                }

                var songExists = dataAccess.GetSongByMd5HashForUser(md5Hash, accountService.GetAccountBySession()) != null;

                if (songExists)
                {
                    return new UploadSongResult { File = fileName };
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.Position = 0;
                    await stream.CopyToAsync(fileStream);
                }
            }

            var song = Song.FromFile(filePath, songId);

            if (song == null)
            {
                File.Delete(filePath);
                return new UploadSongResult { File = fileName };
            }

            song.Md5Hash = md5Hash;
            song.SizeBytes = streamLength;

            AddSongToUserLibrary(song);

            return new UploadSongResult { File = fileName, Success = true, Song = song };
        }

        private static string GetFileExtension(string fileName)
        {
            var split = fileName.Split('.');

            if (split.Count() == 0)
            {
                return null;
            }

            return $".{split.Last().ToLower()}";
        }

        private string GetStringFromMd5Bytes(byte[] bytes)
        {
            var result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
            {
                result.Append(bytes[i].ToString("x2"));
            }

            return result.ToString();
        }
    }
}
