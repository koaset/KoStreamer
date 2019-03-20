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

        public async Task<UploadSongResult> AddSongAsync(IFormFile file)
        {
            // Check user allowed to add size

            var extension = GetFileExtension(file);

            if (extension == null || !AllowedExtensions.Contains(extension))
            {
                // TODO: Check content type
                return new UploadSongResult { File = file.FileName };
            }

            var userDirectoryPath = UserLibraryPath();
            Directory.CreateDirectory(userDirectoryPath);

            var songId = Guid.NewGuid().ToString();
            var filePath = $"{userDirectoryPath}{Path.DirectorySeparatorChar}{songId}{extension}";

            string md5Hash;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using (var md5 = MD5.Create())
                {
                    md5Hash = GetStringFromMd5Bytes(md5.ComputeHash(memoryStream));
                }

                var songExists = dataAccess.GetSongByMd5HashForUser(md5Hash, userAccount) != null;

                if (songExists)
                {
                    return new UploadSongResult { File = file.FileName };
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(fileStream);
                }
            }

            var song = Song.FromFile(filePath, songId);

            if (song == null)
            {
                File.Delete(filePath);
                return new UploadSongResult { File = file.FileName };
            }

            song.Md5Hash = md5Hash;
            song.SizeBytes = file.Length;

            AddSongToUserLibrary(song);

            return new UploadSongResult { File = file.FileName, Success = true, Id = song.Id };
        }

        private static string GetFileExtension(IFormFile file)
        {
            var split = file.FileName.Split('.');

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
