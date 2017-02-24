using System;
using System.Linq;

namespace GPS_Logger.Extensions
{
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Creates a byte array from the given hex string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <remarks>Inspired by http://stackoverflow.com/a/18021603/3063273</remarks>
        public static byte[] FromHexString(string s) =>
            s?
                .Zip(s.Skip(1), (one, two) => new string(new[] { one, two })) // From ABCD make {AB, BC, CD}
                .Where((tuple, index) => index % 2 == 0) // Choose only {AB, CD}
                .Select(tuple => Convert.ToByte(tuple, 16)) // Turn to byte e.g. AB => 171
                .ToArray()
            ?? new byte[0];

        /// <summary>
        /// Creates a lowercased hex string from the byte array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] array) => BitConverter.ToString(array).Replace("-", "").ToLower();
    }
}