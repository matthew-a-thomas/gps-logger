namespace Common.Security
{
    public class KeySizeProvider : IKeySizeProvider
    {
        public KeySizeProvider(int keySize)
        {
            KeySize = keySize;
        }

        public int KeySize { get; }
    }
}
