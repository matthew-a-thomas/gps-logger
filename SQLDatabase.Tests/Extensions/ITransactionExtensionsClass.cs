using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SQLDatabase.Extensions;

namespace SQLDatabase.Tests.Extensions
{
    [TestClass]
    public class ITransactionExtensionsClass
    {
        [TestClass]
        public class GetScalarAsyncMethod
        {
            [TestMethod]
            public async Task HandlesNopInterfaces()
            {
                var mockedTransaction = new Mock<ITransaction>();
                // ReSharper disable once InvokeAsExtensionMethod
                await ITransactionExtensions.GetScalarAsync<object>(mockedTransaction.Object, Commands.Command.Create(""));
            }

            [TestMethod]
            public async Task HandlesNullParameters()
            {
                await ITransactionExtensions.GetScalarAsync<object>(null, null);
            }
        }
    }
}
