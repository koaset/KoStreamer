using Library.Server.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Library.Server.Services
{
    public class StreamerApiAccess : IStreamerApiAccess
    {
        private readonly string userSecret;
        private readonly string streamerApiUrl;

        public StreamerApiAccess(IConfiguration configuration)
        {
            streamerApiUrl = configuration.GetValue<string>("StreamerApiUrl");
            userSecret = configuration.GetValue<string>("UserSecret");
        }

        public async void AuthenticateLibraryAsync()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(streamerApiUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };

            var requestObject = new {
                userSecret
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestObject), Encoding.UTF8, "application/json");
            var result = await client.PostAsync($"library", content);

            if (result.StatusCode != HttpStatusCode.NoContent)
            {
                Log.Error("Unable to authenticate library. Recieved status code: " + result.StatusCode);
            }
        }

        public async Task<bool> IsSessionValidAsync(string session)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(streamerApiUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };

            client.DefaultRequestHeaders.Add("X-Session", session);

            if (string.IsNullOrEmpty(userSecret))
            {
                return false;
            }

            client.DefaultRequestHeaders.Add("X-UserSecret", userSecret);

            var result = await client.GetAsync($"session");

            if (result.StatusCode != HttpStatusCode.NoContent)
            {
                Log.Information("Invalid session!");
                return false;
            }

            return true;
        }
    }
}
