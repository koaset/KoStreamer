using Serilog;
using System;
using System.IO;

namespace Streamer.API.Domain.Entities
{
    public class Song
    {
        public const string LengthFormat = "%m\\:ss";

        public string Path { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

        public string Genre { get; set; }

        public int? TrackNumber { get; set; }

        public int? DiscNumber { get; set; }

        public int? Rating { get; set; }

        public int PlayCount { get; set; }

        public TimeSpan Length { get; set; }

        public int DurationMs
        {
            get { return (int)this.Length.TotalMilliseconds; }
            set { this.Length = new TimeSpan(0, 0, 0, 0, value); }
        }

        public string LengthString { get { return Length.ToString(LengthFormat); } }

        public DateTime DateAdded { get; set; }

        public DateTime LastPlayed { get; set; }
        public string Id { get; set; }
        public string Format { get; internal set; }
        public string AccountId { get; internal set; }
        public string Md5Hash { get; set; }
        public long SizeBytes { get; set; }

        public Song()
        {
            Path = "empty";
            Title = "";
            Artist = "";
            Album = "";
            Genre = "";
            TrackNumber = -1;
            DiscNumber = -1;
            Rating = -1;
            PlayCount = -1;
            Length = TimeSpan.MinValue;
            DateAdded = DateTime.Now;
        }

        /// <summary>
        /// Creates new object with tags read from file
        /// </summary>
        /// <param name="path"></param>
        public Song(string path) : this()
        {
            Path = path;
        }

        public static Song FromFile(string filePath, string id = null)
        {
            var song = new Song
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Path = filePath
            };

            try
            {
                using (var track = TagLib.File.Create(filePath))
                {
                    song.ReadTags(track);
                }
                return song;
            }
            catch (TagLib.UnsupportedFormatException)
            {
                Log.Error("{message}{reason}", "Error when reading song.", "Unsupported format");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{message}", "Error when reading song.");
            }
            return null;
        }

        private void ReadTags(TagLib.File track)
        {
            Title = track.Tag.Title;

            // Set title to path if it is nothing
            if (string.IsNullOrEmpty(Title))
                Title = System.IO.Path.GetFileNameWithoutExtension(this.Path);

            if (Artist != null && track.Tag.Performers.Length > 0)
                Artist = track.Tag.Performers[0];
            else
                Artist = "";

            Album = track.Tag.Album;
            if (Album == null)
                Album = "";

            Genre = track.Tag.FirstGenre;
            if (Genre == null)
                Genre = "";

            TrackNumber = (int)track.Tag.Track;
            DiscNumber = (int)track.Tag.Disc;
            Length = track.Properties.Duration;

            if (Length.TotalMilliseconds == 0)
            {
                throw new Exception("Invalid format");
            }
        }

        public ImageData GetImageData()
        {
            using (var track = TagLib.File.Create(Path))
            {
                if (track.Tag.Pictures.Length > 0)
                {
                    return new ImageData
                    {
                        Bytes = track.Tag.Pictures[0].Data.Data,
                        MimeType = track.Tag.Pictures[0].MimeType
                    };
                }
            }

            return null;
        }
    }
}
