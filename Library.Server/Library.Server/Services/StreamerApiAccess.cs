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
            var client = GetClient();

            var requestObject = new {
                userSecret
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestObject), Encoding.UTF8, "application/json");

            try
            {
                var result = await client.PostAsync($"library", content);

                if (result.StatusCode != HttpStatusCode.NoContent)
                {
                    Log.Error("Unable to authenticate library. Recieved status code: " + result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{message}", "Exception when registering API");
            }
        }

        public async Task<bool> IsSessionValidAsync(string session)
        {
            var client = GetClient();

            client.DefaultRequestHeaders.Add("X-Session", session);

            if (string.IsNullOrEmpty(userSecret))
            {
                return false;
            }
            

            client.DefaultRequestHeaders.Add("X-UserSecret", userSecret);

            var result = await client.GetAsync("api/session");

            if (result.StatusCode != HttpStatusCode.NoContent)
            {
                Log.Information("Invalid session!");
                return false;
            }

            return true;
        }

        private HttpClient GetClient()
        {
            var client = new HttpClient(
                new HttpClientHandler
                {
                    UseProxy = false
                })
            {
                Timeout = TimeSpan.FromSeconds(5),
                BaseAddress = new Uri(streamerApiUrl)
            };
            return client;
        }
    }
}
