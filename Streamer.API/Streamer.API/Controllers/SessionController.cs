using Microsoft.AspNetCore.Mvc;
using Streamer.API.Domain;
using Streamer.API.Domain.Interfaces;
using Streamer.API.Models;
using System.Threading.Tasks;

namespace Streamer.API.Controllers
{
    [Route("session")]
    [Produces("application/json")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly IAccountService accountService;
        private readonly ISessionService sessionService;

        public SessionController(IAccountService accountService, ISessionService sessionService)
        {
            this.accountService = accountService;
            this.sessionService = sessionService;
        }

        [HttpPost("googleAuth")]
        public async Task<ActionResult<LoginResponseModel>> PostGoogleLogin(GoogleLoginRequestModel model)
        {
            var googleData = await GoogleTokenHelper.ValidateGoogleTokenAndGetUserData(model.IdToken);

            if (googleData == null)
            {
                return Unauthorized();
            }

            var account = accountService.GetAccountByGoogleId(googleData.Id);

            if (account == null)
            {
                return NotFound(); // New accounts disabled for now.
                //account = accountService.CreateAccount(googleData);
            }

            var sessionEntity = sessionService.CreateNewSession(account);

            return new LoginResponseModel {
                Session = sessionEntity.SessionId
            };
        }

        [HttpGet("")]
        public ActionResult ValidateSession()
        {
            return sessionService.RequestSessionIsValid() ? (ActionResult)NoContent() : Unauthorized();
        }

        [HttpDelete("")]
        public ActionResult DeleteSession()
        {
            sessionService.DeleteSession();
            return NoContent();
        }
    }
}
