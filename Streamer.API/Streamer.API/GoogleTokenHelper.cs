using Streamer.API.Entities;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Streamer.API
{
    public class GoogleTokenHelper 
    {
        private static readonly string streamerAppId = "900614446703-5p76k96hle7h4ucg4qgdcclcnl4t7njj.apps.googleusercontent.com";

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

        private bool TokenInfoResponseValid(GoogleTokenInfoResponse response)
        {
            if (response.Aud != streamerAppId || response.Iss != "accounts.google.com")
                return false;
            return true;
        }

        private HttpClient GetClient()
        {
            return new HttpClient
            {
                BaseAddress = new Uri("https://oauth2.googleapis.com/"),
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        private class GoogleTokenInfoResponse
        {
            public string Iss { get; set; }
            public string Sub { get; set; }
            public string Aud { get; set; }
            public string Email { get; set; }
            public bool EmailVerified { get; set; }
            public string Name { get; set; }
        }
    }
}
