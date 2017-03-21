using System;

namespace Common.RemoteStorage.Models
{
    public class IdentifiedLocation
    {
        public byte[] Identifier { get; set; }

        public DateTime Timestamp { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
