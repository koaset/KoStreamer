using Microsoft.AspNetCore.Mvc;

namespace Streamer.API.Controllers
{
    [Route("ping")]
    [Produces("application/json")]
    [ApiController]
    public class PingController : ControllerBase
    {
        [HttpGet("")]
        public ActionResult<string> GetPing()
        {
            return $"Hejdu...";
        }
    }
}
