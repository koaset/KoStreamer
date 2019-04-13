using Serilog;
using Streamer.API.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Streamer.API.Domain
{
    public class CleanupHelper
    {
        private IDataAccess dataAccess;
        private ILibraryService libraryService;

        public CleanupHelper(IDataAccess dataAccess, ILibraryService libraryService)
        {
            this.libraryService = libraryService;
            this.dataAccess = dataAccess;
        }

        public void DoUserFolderCleanup()
        {
            Log.Information("{message}", "Starting cleanup.");
            List<string> userIds;
            try
            {
                userIds = dataAccess.GetAllUserIds();
            }
            catch
            {
                Log.Error("{message}", "Error when connecting to database.");
                return;
            }
            
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

            CleanDirectories(checkedFolders);

            Log.Information("{message}", "Cleanup succesfully completed.");
        }

        private void CleanDirectories(List<string> checkedFolders)
        {
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
