namespace Streamer.API.Domain.Entities
{
    public class UploadSongResult
    {
        public string File { get; set; }
        public bool Success { get; set; }
        public Song Song { get; set; }
    }
}
