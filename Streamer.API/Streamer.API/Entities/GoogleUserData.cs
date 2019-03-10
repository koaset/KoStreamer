namespace Streamer.API.Entities
{
    public class GoogleUserData
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public string Email { get; internal set; }
        public bool EmailVerified { get; internal set; }
    }
}
