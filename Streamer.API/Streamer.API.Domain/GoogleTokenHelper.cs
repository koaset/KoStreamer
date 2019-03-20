using Microsoft.Extensions.Configuration;
using Streamer.API.Domain.Entities;
using Streamer.API.Domain.Interfaces;
using System.Threading.Tasks;

namespace Streamer.API.Domain
{
    public class GoogleTokenHelper : IGoogleTokenHelper
    {
        private readonly string streamerAppId;

        public GoogleTokenHelper(IConfiguration configuration)
        {
            streamerAppId = configuration.GetValue<string>("GoogleAppId");
        }

        public async Task<GoogleUserData> ValidateGoogleTokenAndGetUserDataAsync(string token)
        {
            try
            {
                var validateResult = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(token);

                if (validateResult.Audience.ToString() != streamerAppId)
                {
                    return null;
                }

                return new GoogleUserData
                {
                    Id = validateResult.Subject,
                    Email = validateResult.Email,
                    EmailVerified = validateResult.EmailVerified,
                    Name = validateResult.Name
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
