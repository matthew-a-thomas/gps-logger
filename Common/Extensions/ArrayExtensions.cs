namespace Common.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Same as .Clone, but with strong type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T[] CreateClone<T>(this T[] array) => array.Clone() as T[];
    }
}