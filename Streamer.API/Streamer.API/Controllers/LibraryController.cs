﻿using Microsoft.AspNetCore.Mvc;
using Streamer.API.Interfaces;
using Streamer.API.Library;
using Streamer.API.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Streamer.API.Controllers
{
    [Route("library")]
    [Produces("application/json")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly IDataAccess dataAccess;
        private readonly ISessionValidator sessionValidator;
        private Dictionary<string, Song> songDictionary = SongLibrary.Default.songDictionary;

        public LibraryController(IDataAccess dataAccess, ISessionValidator sessionValidator)
        {
            this.dataAccess = dataAccess;
            this.sessionValidator = sessionValidator;
        }
        
        [HttpGet("status")]
        public ActionResult<StatusModel> GetStatus()
        {
            return new StatusModel
            {
                NumSongs = songDictionary.Count,
                LibrayLoadTime = SongLibrary.Default.libraryLoadMs
            };
        }

        [HttpGet("songs")]
        public ActionResult<List<SongModel>> GetSongs(int page = 1, int size = 100)
        {
            if (page < 1)
            {
                page = 1;
            }
            if (size < 1 || size > 1000)
            {
                size = 100;
            }

            return songDictionary.Skip((page - 1) * size).Take(size).Select(p => SongModel.FromSong(p.Value)).ToList();
        }

        [HttpGet("song/{id}")]
        public ActionResult<SongModel> GetSong(string id)
        {
            if (!songDictionary.ContainsKey(id))
                return NotFound();
            return SongModel.FromSong(songDictionary[id]);
        }

        [HttpGet("song/play")]
        public IActionResult GetSongStream(string id, string sessionId = "")
        {
            if (!songDictionary.ContainsKey(id))
                return NotFound();
            var song = songDictionary[id];
            var stream = new StreamReader(song.Path).BaseStream;
            return File(stream, "audio/mpeg3", enableRangeProcessing: true);
        }
    }
}
