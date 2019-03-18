using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
            var ip = Request.HttpContext.GetRemoteIPAddress(true);
            var original = Request.Headers["X-Original-Forwarded-For"].FirstOrDefault();
            return $"Hejdu... ip_with_true={ip} original={original}";
        }
    }
}
