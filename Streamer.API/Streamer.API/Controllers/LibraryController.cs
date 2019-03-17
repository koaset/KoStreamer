using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Serilog;
using Streamer.API.Entities;
using Streamer.API.Interfaces;
using Streamer.API.Models;
using System;

namespace Streamer.API.Controllers
{
    [Route("library")]
    [Produces("application/json")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly IDataAccess dataAccess;
        private readonly ISessionValidator sessionValidator;

        public LibraryController(IDataAccess dataAccess, ISessionValidator sessionValidator)
        {
            this.dataAccess = dataAccess;
            this.sessionValidator = sessionValidator;
        }

        [HttpPost("")]
        public ActionResult PostAuthenticateLibraryServer(LibraryAuthRequestModel model)
        {
            var account = dataAccess.GetAccountByUserSecret(model.UserSecret);

            if (account == null)
            {
                return Unauthorized();
            }

            var serverAddress = Request.HttpContext.GetRemoteIPAddress(true).ToString();

            if (serverAddress == "::1")
            {
                serverAddress = "localhost";
            }

            Log.Information("{user_ip}", serverAddress);

            serverAddress = $"https://{serverAddress}:44362";

            var lib = new AccountLibrary
            {
                LibraryId = Guid.NewGuid().ToString().Replace("-", string.Empty),
                AccountId = account.AccountId,
                ServerAddress = serverAddress,
                DateAdded = DateTime.UtcNow,
                LastActive = DateTime.UtcNow
            };

            dataAccess.AddLibrary(lib);

            return NoContent();
        }

        [HttpGet("")]
        public ActionResult<AccountLibraryModel> GetLibrary()
        {
            var sessionEntity = sessionValidator.GetRequestSession();
            if (!sessionValidator.SessionIsValid(sessionEntity))
            {
                return Unauthorized();
            }

            var library = dataAccess.GetLibrary(sessionEntity.AccountId);

            return new AccountLibraryModel
            {
                LibraryId = library.LibraryId,
                ServerAddress = library.ServerAddress
            };
        }
    }
}
