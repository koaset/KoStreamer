using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Streamer.API.Lib
{
    public class Library
    {
        public static Library Default;

        public Dictionary<string, Song> songDictionary { get; set; }

        public Library(IConfiguration configuration)
        {
            var path = configuration.GetValue<string>("Library:SongFolderPath");
            var paths = Directory.EnumerateFiles(path).ToList();
            var songs = paths.Select(p =>
            {
                var song = Song.FromFile(p);
                song.Id = Guid.NewGuid().ToString();
                return song;
            }).ToList();
            songDictionary = songs.ToDictionary(s => s.Id, s => s);
        }

        public void Load()
        {

        }

        public void Save()
        {

        }
    }
}
