using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly IGoogleTokenHelper googleHelper;
        private readonly bool registrationDisabled;

        public SessionController(IAccountService accountService, ISessionService sessionService, IGoogleTokenHelper googleHelper, IConfiguration configration)
        {
            this.accountService = accountService;
            this.sessionService = sessionService;
            this.googleHelper = googleHelper;
            registrationDisabled = configration.GetValue<bool>("RegistrationDisabled");
        }

        [HttpPost("googleAuth")]
        public async Task<ActionResult<LoginResponseModel>> PostGoogleLogin(GoogleLoginRequestModel model)
        {
            if (model.IdToken == "test")
            {
                var testAccount = accountService.GetTestAccount();
                var session = sessionService.CreateNewSession(testAccount);

                return new LoginResponseModel
                {
                    Session = session.SessionId
                };
            }

            var googleData = await googleHelper.ValidateGoogleTokenAndGetUserDataAsync(model.IdToken);

            if (googleData == null)
            {
                return Unauthorized();
            }

            var account = accountService.GetAccountByGoogleId(googleData.Id);

            if (account == null)
            {
                if (registrationDisabled)
                {
                    return NotFound();
                }

                account = accountService.CreateAccount(googleData);
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
