using Microsoft.AspNetCore.Mvc;

namespace Library.Server.Controllers
{
    [Route("ping")]
    [Produces("application/json")]
    [ApiController]
    public class PingController : ControllerBase
    {
        [HttpGet("")]
        public ActionResult<string> GetPing()
        {
            return "Hej";
        }
    }
}
