namespace GPS_Logger.Serialization
{
    public interface ISerializer<in T>
    {
        byte[] Serialize(T thing);
    }
}