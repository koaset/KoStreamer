using System;
using System.Collections.Generic;
using System.Text;

namespace Streamer.API.Domain.Entities
{
    public class ImageData
    {
        public byte[] Bytes { get; set; }
        public string MimeType { get; set; }
    }
}
