using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Streamer.API.Lib;
using Streamer.API.Models;

namespace Streamer.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private Dictionary<string, Song> songDictionary = Library.Default.songDictionary;

        [HttpGet("songs/names")]
        public ActionResult<List<string>> GetSongNames()
        {
            return songDictionary.Keys.ToList();
        }

        [HttpGet("songs")]
        public ActionResult<List<SongModel>> GetSongs()
        {
            return songDictionary.Values.Select(s => SongModel.FromSong(s)).ToList();
        }

        [HttpGet("song/{id}")]
        public ActionResult<SongModel> GetSong(string id)
        {
            if (!songDictionary.ContainsKey(id))
                return NotFound();
            return SongModel.FromSong(songDictionary[id]);
        }
        
        [HttpGet("song/stream/{id}")]
        public IActionResult GetSongStream(string id)
        {
            if (!songDictionary.ContainsKey(id))
                return NotFound();
            var song = songDictionary[id];
            var stream = new StreamReader(song.Path).BaseStream;
            return File(stream, "audio/mpeg3", enableRangeProcessing: true);
        }
    }
}
