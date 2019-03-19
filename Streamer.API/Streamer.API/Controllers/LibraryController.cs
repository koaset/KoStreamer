using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Streamer.API.Domain;
using Streamer.API.Domain.Entities;
using Streamer.API.Domain.Interfaces;
using Streamer.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
        public async Task<IActionResult> PostUploadSong(IFormFile infile)
        {
            var files = new List<IFormFile>() { infile };
            long size = files.Sum(f => f.Length);
            // TODO: Check if can save

            var results = new List<UploadSongResult>();

            foreach (var file in files)
            {
                results.Add(await AddSong(file));
            }

            if (results.All(r => r.Success))
            {
                return Ok(results);
            }
            if (results.All(r => !r.Success))
            {
                return BadRequest(results);
            }
            return StatusCode(207, results);
        }

        private async Task<UploadSongResult> AddSong(IFormFile file)
        {
            var extension = GetFileExtension(file);

            if (extension == null || !Library.EXTENSIONS.Contains(extension))
            {
                // TODO: Check content type
                return new UploadSongResult { File = file.FileName };
            }

            var userDirectoryPath = libraryService.UserLibraryPath();
            Directory.CreateDirectory(userDirectoryPath);

            var songId = Guid.NewGuid().ToString();
            var filePath = $"{userDirectoryPath}{Path.DirectorySeparatorChar}{songId}{extension}";

            string hash;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);

                using (var md5 = MD5.Create())
                {
                    hash = GetStringFromMd5Bytes(md5.ComputeHash(memoryStream));
                }

                // Check hash no already exists

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(fileStream);
                }
            }

            var song = Song.FromFile(filePath, songId);

            song.Md5Hash = hash;

            if (song == null)
            {
                System.IO.File.Delete(filePath);
                return new UploadSongResult { File = file.FileName };
            }

            libraryService.AddSongToUserLibrary(song);

            return new UploadSongResult { File = file.FileName, Success = true, Id = song.Id };
        }

        private string GetStringFromMd5Bytes(byte[] bytes)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
            {
                result.Append(bytes[i].ToString("x2"));
            }

            return result.ToString();
        }

        private string GetFileExtension(IFormFile file)
        {
            var split = file.FileName.Split('.');

            if (split.Count() == 0)
            {
                return null;
            }

            return $".{split.Last().ToLower()}";
        }
    }
}
