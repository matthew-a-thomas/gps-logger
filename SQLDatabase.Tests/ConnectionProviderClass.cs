using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Common.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SQLDatabase.Tests
{
    [TestClass]
    public class ConnectionProviderClass
    {
        [TestClass]
        public class Constructor
        {
            [TestMethod]
            public void HandlesNopInterfaces()
            {
                // ReSharper disable once ObjectCreationAsStatement
                new ConnectionProvider(
                    new ConnectionOptions {Database = null, Password = null, Server = null, User = null},
                    new Mock<IFactory<ConnectionOptions, string>>().Object,
                    new Mock<IFactory<string, SqlConnection>>().Object
                );
            }

            [TestMethod]
            public void HandlesNullParameters()
            {
                // ReSharper disable once ObjectCreationAsStatement
                new ConnectionProvider(null, null, null);
                // ReSharper disable once ObjectCreationAsStatement
                new ConnectionProvider(
                    new ConnectionOptions {Database = null, Password = null, Server = null, User = null},
                    new Factory<ConnectionOptions, string>(_ => null),
                    new Factory<string, SqlConnection>(_ => null)
                );
            }
        }

        [TestClass]
        public class CreateConnectionClass
        {
            [TestMethod]
            public void ReturnsWhatFactoryProvides()
            {
                var connection = new SqlConnection();
                var createdConnection = new ConnectionProvider(
                        new ConnectionOptions(),
                        new Factory<ConnectionOptions, string>(_ => ""),
                        new Factory<string, SqlConnection>(_ => connection)
                    )
                    .CreateConnection();
                Assert.AreEqual(connection, createdConnection);
            }
        }
    }
}
