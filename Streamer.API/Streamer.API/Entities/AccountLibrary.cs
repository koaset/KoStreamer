using System;

namespace Streamer.API.Entities
{
    public class AccountLibrary
    {
        public string LibraryId { get; set; }
        public string AccountId { get; set; }
        public string ServerAddress { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime LastActive { get; set; }
    }
}
