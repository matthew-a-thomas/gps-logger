namespace GPS_Logger.Security
{
    public class Credential<T>
    {
        // ReSharper disable once InconsistentNaming
        public T ID { get; set; }
        public T Secret { get; set; }
    }
}