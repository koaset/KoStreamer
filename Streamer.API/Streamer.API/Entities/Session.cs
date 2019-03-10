using System;

namespace Streamer.API.Entities
{
    public class Session
    {
        public string SessionId { get; set; }
        public string AccountId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Invalidated { get; set; }
    }
}
