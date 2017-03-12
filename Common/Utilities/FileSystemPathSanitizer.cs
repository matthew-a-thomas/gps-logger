using Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Common.Utilities
{
    public static class FileSystemPathSanitizer
    {
        /// <summary>
        /// All the characters that must be escaped
        /// </summary>
        private static readonly HashSet<char> InvalidCharacters =
            new HashSet<char>(
                Path.GetInvalidFileNameChars().Concat(
                Path.GetInvalidPathChars()).Concat(
                new char[] { Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar })
                );

        /// <summary>
        /// The escape character to use when escaping other characters
        /// </summary>
        private const char EscapeCharacter = '_';

        /// <summary>
        /// Escapes the given character if it's in the set of invalid characters.
        /// Otherwise returns it as a string.
        /// An escaped string will be the escape character followed by a four digit hex number indicating its ASCII code
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static string Escape(char c) => InvalidCharacters.Contains(c) ? string.Format("{0}{1:x2}", EscapeCharacter, (byte)c) : c.ToString();

        /// <summary>
        /// Sanitizes the given string so that it's safe for file system use
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void Sanitize(ref string path)
        {
            if (path == null)
                return;

            // Escape any escape characters
            path = path.Replace(EscapeCharacter.ToString(), Escape(EscapeCharacter));

            // Escape any invalid characters
            foreach (var c in InvalidCharacters)
                path = path.Replace(c.ToString(), Escape(c));

            // Shorten by truncating and appending with part of the SHA1 hash of the whole thing
            const int maxLength = 40;
            if (path.Length > maxLength)
            {
                using (var hasher = SHA1.Create())
                {
                    var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(path));
                    var hashString = hash.ToHexString();
                    var partialHashString = new string(hashString.Take(6).ToArray()); // Take the first six hex characters
                    path = string.Format("{0}__{1}", path.Substring(0, maxLength - partialHashString.Length - 2), partialHashString);
                }
            }
        }
    }
}
