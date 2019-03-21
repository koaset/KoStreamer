using Microsoft.AspNetCore.Http;
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

        public async Task<UploadSongResult> AddSongAsync(MemoryStream stream, string fileName)
        {
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

                var songExists = dataAccess.GetSongByMd5HashForUser(md5Hash, userAccount) != null;

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
