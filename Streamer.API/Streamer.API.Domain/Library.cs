using Newtonsoft.Json;
using Streamer.API.Domain.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Streamer.API.Domain
{
    public class Library
    {
        public static string[] EXTENSIONS = { ".mp3", ".m4a", ".wma", ".aac", ".flac" };
        public static Library Default;
        public long? libraryLoadMs;
        private readonly List<string> ConfiguredFolders;
        public Dictionary<string, Song> songDictionary { get; set; }

        public Library(List<string> libraryFolders)
        {
            ConfiguredFolders = libraryFolders ?? new List<string>();
            if (songDictionary == null)
            {
                songDictionary = new Dictionary<string, Song>();
            }

            //Load();
            ReadFolders();
        }

        public Library() : this(new List<string>())
        {

        }

        private void ScanLibrary()
        {

        }

        internal void ReadFolders()
        {
            ReadFolders(ConfiguredFolders);
        }

        internal void ReadFolders(List<string> folders)
        {
            var paths = new List<string>();
            foreach (var folder in folders)
            {
                paths.AddRange(GetSongsPathsFromFolder(folder));
            }
            TryAddSongs(paths);
        }

        private List<string> GetSongsPathsFromFolder(string folderPath)
        {
            var musicFiles = new List<string>();
            foreach (var extension in EXTENSIONS)
            {
                musicFiles.AddRange(Directory.GetFiles(folderPath, "*" + extension, SearchOption.AllDirectories));
            }
            return musicFiles;
        }

        private void TryAddSongs(List<string> musicFiles)
        {
            var songsToAdd = new ConcurrentBag<Song>();
            var currentLibPaths = new HashSet<string>(songDictionary.Values.Select(s => s.Path));

            foreach (var path in musicFiles)
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
            }

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
                    var fileText = sr.ReadToEnd();
                    var lib = JsonConvert.DeserializeObject<List<Song>>(fileText);
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
            var jsonLib = JsonConvert.SerializeObject(songDictionary.Values.ToList());
            using (var sw = new StreamWriter(File.OpenWrite(libraryFilePath)))
            {
                sw.Write(jsonLib);
            }
        }
    }
}
