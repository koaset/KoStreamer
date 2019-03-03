using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Streamer.API.Lib
{
    public class Library
    {
        public static string[] EXTENSIONS = { ".mp3", ".m4a", ".wma", ".aac", ".flac" };
        public static Library Default;
        public long? libraryLoadMs;
        private readonly List<string> ConfiguredFolders;
        

        public Dictionary<string, Song> songDictionary { get; set; }

        public Library(IConfiguration configuration)
        {
            ConfiguredFolders = configuration.GetSection("Library:Folders").Get<List<string>>() ?? new List<string>();
            
            //Load();

            if (songDictionary == null)
            {
                songDictionary = new Dictionary<string, Song>();
            }

            //Read(ConfiguredFolders);
        }

        internal void Read()
        {
            Read(ConfiguredFolders);
        }

        internal void Read(List<string> folders)
        {
            var paths = new List<string>();
            foreach(var folder in folders)
            {
                paths.AddRange(GetSongsPathsFromFolder(folder));
            }
            TryAddSongs(paths);

            Save();
        }

        private List<string> GetSongsPathsFromFolder(string folderPath)
        {
            var musicFiles = new List<string>();
            foreach (var extension in Library.EXTENSIONS)
            {
                musicFiles.AddRange(Directory.GetFiles(folderPath, "*" + extension, SearchOption.AllDirectories));
            }
            return musicFiles;
        }

        private void TryAddSongs(List<string> musicFiles)
        {
            var songsToAdd = new ConcurrentBag<Song>();
            var currentLibPaths = new HashSet<string>(songDictionary.Values.Select(s => s.Path));

            Parallel.ForEach(musicFiles.Distinct(), new ParallelOptions { MaxDegreeOfParallelism = 8 }, (path) =>
            {
                if (currentLibPaths.Contains(path))
                {
                    return;
                }

                Song newSong = null;
                try
                {
                    newSong = Song.FromFile(path);
                }
                catch
                {
                    Console.WriteLine("Error when reading: " + path);
                }

                songsToAdd.Add(newSong);
            });

            foreach (var song in songsToAdd)
            {
                songDictionary[song.Id] = song;
            }
        }

        public async void Load()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            songDictionary = new Dictionary<string, Song>();

            await Task.Run(() => {
                
                var libraryFilePath = "library.json";
                if (!File.Exists(libraryFilePath))
                {
                    return;
                }

                using (var sr = new StreamReader(File.OpenRead(libraryFilePath)))
                {
                    var lib = JsonConvert.DeserializeObject<List<Song>>(sr.ReadToEnd());
                    foreach (var song in lib)
                    {
                        songDictionary.Add(song.Id, song);
                    }
                }
            });

            sw.Stop();
            Console.WriteLine("read_library_ms=" + sw.ElapsedMilliseconds);
            libraryLoadMs = sw.ElapsedMilliseconds;
        }

        public void Save()
        {
            var libraryFilePath = "library.json";
            var jsonLib = JsonConvert.SerializeObject(songDictionary.Values);
            using (var sw = new StreamWriter(File.OpenWrite(libraryFilePath)))
            {
                sw.Write(jsonLib);
            }
        }
    }
}
