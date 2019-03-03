using Microsoft.AspNetCore.Mvc;

namespace Streamer.API.Controllers
{
    [Route("ping")]
    [ApiController]
    public class PingController : ControllerBase
    {
        [HttpGet("")]
        public ActionResult<string> GetSongNames()
        {
            return "Alive and well!";
        }
    }
}
