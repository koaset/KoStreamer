using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Streamer.API.Domain.Interfaces;
using Streamer.API.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Streamer.API.Controllers
{
    [Route("library")]
    [Produces("application/json")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly ILibraryService libraryService;

        public LibraryController(ILibraryService libraryService)
        {
            this.libraryService = libraryService;
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
            return libraryService.GetUserLibrary().Skip((page - 1) * size).Take(size).Select(s => SongModel.FromSong(s)).ToList();
        }

        [HttpGet("song/{id}")]
        public ActionResult<SongModel> GetSong(string id)
        {
            var lib = libraryService.GetUserLibrary();
            if (!lib.Any(s => s.Id == id))
                return NotFound();
            return SongModel.FromSong(lib.First(s => s.Id == id));
        }

        [HttpGet("song/play")]
        public IActionResult GetSongStream(string id, string sessionId = "")
        {
            var lib = libraryService.GetUserLibrary();
            if (!lib.Any(s => s.Id == id))
                return NotFound(); ;
            var song = lib.First(s => s.Id == id);
            var stream = new StreamReader(song.Path).BaseStream;
            return File(stream, "audio/mpeg3", enableRangeProcessing: true);
        }

        [HttpPost("song/upload")]
        public async Task<IActionResult> PostUploadSong(IFormFile qqfile)
        {
            var addSongResult = await libraryService.AddSongAsync(qqfile);

            if (!addSongResult.Success)
            {
                return BadRequest(addSongResult);
            }
            return Ok(addSongResult);
        }
    }
}
