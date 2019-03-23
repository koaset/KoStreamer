using Streamer.API.Domain.Entities;
using Streamer.API.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Streamer.API.Domain.Setup
{
    public static class StartupHelper
    {
        public static void DoUserFolderCleanup(IDataAccess dataAccess, ILibraryService libraryService)
        {
            var userIds = dataAccess.GetAllUserIds();

            foreach(var id in userIds)
            {
                var songs = dataAccess.GetSongsForUser(id);
                var libraryFolderFiles = Directory.GetFiles(libraryService.UserLibraryPath(id));

                foreach (var song in songs)
                {
                    if (!libraryFolderFiles.Contains(song.Path))
                    {
                        dataAccess.DeleteSongForUser(song);
                    }
                }

                foreach (var file in libraryFolderFiles)
                {
                    if (!songs.Any(s => s.Path == file))
                    {
                        File.Delete(file);
                    }
                }
            }
        }
    }
}
