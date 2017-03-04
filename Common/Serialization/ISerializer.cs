namespace Common.Serialization
{
    public interface ISerializer<in T>
    {
        byte[] Serialize(T thing);
    }
}