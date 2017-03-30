using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Common.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SQLDatabase.Tests
{
    [TestClass]
    public class TransactionClass
    {
        private static void TestWithNullConstructorParameters(Action<Transaction> test)
        {
            using (var t = new Transaction(null, null))
            {
                test(t);
            }
            using (var t = new Transaction(null, new ConnectionOptions()))
            {
                test(t);
            }
            using (var t = new Transaction(new Factory<ConnectionOptions, SqlConnection>(_ => null), null))
            {
                test(t);
            }
            using (var t = new Transaction(new Factory<ConnectionOptions, SqlConnection>(_ => null), new ConnectionOptions()))
            {
                test(t);
            }
        }

        [TestClass]
        public class Constructor
        {
            [TestMethod]
            [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
            public void HandlesNullParameters()
            {
                TestWithNullConstructorParameters(_ => { });
            }

            [TestMethod]
            public void HandlesNopInterfaces()
            {
                // ReSharper disable once ObjectCreationAsStatement
                using (new Transaction(
                    new Mock<IFactory<ConnectionOptions, SqlConnection>>().Object,
                    new ConnectionOptions())) { }
            }
        }

        [TestClass]
        public class CommitMethod
        {
            [TestMethod]
            public void CanBeCalledWithNullConstructorParameters()
            {
                TestWithNullConstructorParameters(transaction => transaction.Commit());
            }
        }
    }
}
