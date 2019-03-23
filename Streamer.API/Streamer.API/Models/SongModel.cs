using Streamer.API.Domain.Entities;
using System;

namespace Streamer.API.Models
{
    public class SongModel
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

        public string Genre { get; set; }

        public int? TrackNumber { get; set; }

        public int? DiscNumber { get; set; }

        public int? Rating { get; set; }
        public string LengthString { get; set; }
        public int DurationMs { get; set; }
        public string Art { get; set; }
        public string ArtMimeType { get; set; }

        public static SongModel FromSong(Song song)
        {
            var imageData = song.GetImageData();

            return new SongModel
            {
                Id = song.Id,
                Title = song.Title,
                Artist = song.Artist,
                Album = song.Album,
                Genre = song.Genre,
                TrackNumber = song.TrackNumber,
                DiscNumber = song.DiscNumber,
                Rating = song.Rating,
                LengthString = song.LengthString,
                DurationMs = song.DurationMs,
                Art = imageData != null ? Convert.ToBase64String(imageData.Bytes) : null,
                ArtMimeType = imageData?.MimeType
            };
        }
    }
}
