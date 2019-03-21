using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Streamer.API.Domain.Entities;
using Streamer.API.Domain.Interfaces;
using Streamer.API.Models;
using System;
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
        public async Task<ActionResult> PostSongUploadChunk(IFormFile qqfile, [FromForm]string qquuid, [FromForm]string qqfilename, [FromForm]int qqpartindex,
            [FromForm]long qqpartbyteoffset, [FromForm]long qqtotalfilesize, [FromForm] int qqtotalparts, [FromForm] long qqchunksize)
        {
            var inCache = uploadChunkCache.TryGetValue(qquuid, out MemoryStream stream);

            if (!inCache && qqpartindex == 0)
            {
                stream = new MemoryStream();
            }
            else if (inCache && qqpartindex == 0 || !inCache && qqpartindex != 0)
            {
                return BadRequest();
            }

            await qqfile.CopyToAsync(stream);
            uploadChunkCache.Set(qquuid, stream, new MemoryCacheEntryOptions { Size = stream.Length, SlidingExpiration = TimeSpan.FromSeconds(10) });

            if (qqtotalparts == 0)
            {
                return await PostCompleteSongUpload(qquuid, qqfilename);
            }
            else
            {
                return Ok(new { success = true });
            }
        }

        [HttpPost("song/upload/complete")]
        public async Task<ActionResult> PostCompleteSongUpload([FromForm]string qquuid, [FromForm]string qqfilename)
        {
            if (!uploadChunkCache.TryGetValue(qquuid, out MemoryStream stream))
            {
                return BadRequest();
            }
            
            var addSongResult = await libraryService.AddSongAsync(stream, qqfilename);

            if (!addSongResult.Success)
            {
                return BadRequest();
            }

            return Ok(new { success = true, song = SongModel.FromSong(addSongResult.Song) });
        }

        private static readonly MemoryCache uploadChunkCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 1000000000 });
    }
}
