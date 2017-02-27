namespace GPS_Logger.Security.Messages
{
    public class Message<T>
    {
        public T Contents { get; set; }
        public byte[] ID { get; set; }
        public byte[] Salt { get; set; }
        public long UnixTime { get; set; }
    }
}