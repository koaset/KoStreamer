using Streamer.API.Entities;
using System.Threading.Tasks;

namespace Streamer.API
{
    public class GoogleTokenHelper 
    {
        private static string streamerAppId;

        public static void Configure(string appId)
        {
            streamerAppId = appId;
        }

        public static async Task<GoogleUserData> ValidateGoogleTokenAndGetUserData(string token)
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
