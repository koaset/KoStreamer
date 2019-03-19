using System;

namespace Streamer.API.Domain.Entities
{
    public class Account
    {
        public string AccountId { get; set; }
        public string GoogleId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Email { get; set; }
    }
}
