namespace GPS_Logger.Models.Messages
{
    public class Message<T>
    {
        public T Contents { get; set; }
        // ReSharper disable once InconsistentNaming
        public string ID { get; set; }
        public string Salt { get; set; }
        public long UnixTime { get; set; }
    }
}