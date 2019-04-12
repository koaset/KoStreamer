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
            
            var checkedFolders = new List<string>();

            foreach(var id in userIds)
            {
                var songs = dataAccess.GetSongsForUser(id);
                var libraryPath = libraryService.UserLibraryPath(id);

                if (!Directory.Exists(libraryPath))
                {
                    continue;
                }

                var libraryFolderFiles = Directory.GetFiles(libraryPath);

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

                checkedFolders.Add(libraryPath);
            }

            foreach (var directory in Directory.GetDirectories(libraryService.MediaFolder()))
            {
                if (!checkedFolders.Contains(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}
