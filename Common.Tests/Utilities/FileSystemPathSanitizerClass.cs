using System;
using System.Threading.Tasks;
using Common.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Utilities
{
    [TestClass]
    public class FileSystemPathSanitizerClass
    {
        [TestClass]
        public class SanitizeAsyncMethod
        {
            [TestMethod]
            public async Task DoesNotExceedMaxLength()
            {
                const int maxLength = 100;
                var random = new Random();
                for (var i = 0; i < 1000; ++i)
                {
                    var characters = new char[i];
                    for (var j = 0; j < i; ++j)
                        characters[j] = (char) random.Next(1, 256);
                    var path = new string(characters);

                    var sanitizedPath = await FileSystemPathSanitizer.SanitizeAsync(path, maxLength);
                    Assert.IsTrue(sanitizedPath.Length <= maxLength);
                }
            }
        }
    }
}
