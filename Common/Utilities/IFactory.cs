namespace Common.Utilities
{
    public interface IFactory<in TOptions, out TInstance>
    {
        TInstance Create(TOptions options);
    }
}
