using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Common.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SQLDatabase.Tests
{
    [TestClass]
    public class TransactionClass
    {
        [TestClass]
        public class Constructor
        {
            [TestMethod]
            [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
            public void HandlesNullParameters()
            {
                using (new Transaction(null, null)) { }
                using (new Transaction(null, new ConnectionOptions())) { }
                using (new Transaction(new Factory<ConnectionOptions, SqlConnection>(_ => null), null)) { }
                using (new Transaction(new Factory<ConnectionOptions, SqlConnection>(_ => null), new ConnectionOptions())) { }
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
    }
}
