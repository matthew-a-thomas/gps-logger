namespace Common.RemoteStorage.Models
{
    /// <summary>
    /// A location
    /// </summary>
    public class Location
    {
        /// <summary>
        /// The latitude
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// The longitude
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// The number of seconds since Jan 1, 1970 (aka Unix Time)
        /// </summary>
        public long UnixTime { get; set; }
    }
}