using System;

namespace Streamer.API.Lib
{
    public class Song : IComparable
    {
        public const string LengthFormat = "%m\\:ss";

        public string Path { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

        public string Genre { get; set; }

        public int TrackNumber { get; set; }

        public int DiscNumber { get; set; }

        public int Rating { get; set; }

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

        /// <summary>
        /// Sortorder according to disc and track numbers
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            Song otherSong = obj as Song;
            if (otherSong != null)
            {
                if (this.Album.CompareTo(otherSong.Album) == 0)
                {
                    if (this.DiscNumber.CompareTo(otherSong.DiscNumber) != 0)
                        return this.DiscNumber.CompareTo(otherSong.DiscNumber);
                    else
                        return this.TrackNumber.CompareTo(otherSong.TrackNumber);
                }
                return this.Album.CompareTo(otherSong.Album);
            }
            else
                throw new ArgumentException("Object is not a Song");
        }

        public static Song FromFile(string filePath)
        {
            var song = new Song { Path = filePath };
            using (var track = TagLib.File.Create(filePath))
            {
                song.ReadTags(track);
            }
            return song;
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
        }
    }
}
