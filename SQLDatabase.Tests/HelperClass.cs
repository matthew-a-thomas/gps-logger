using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SQLDatabase.Tests
{
    [TestClass]
    public class HelperClass
    {
        [TestClass]
        public class CreateConnectionMethod
        {
            [TestMethod]
            public void ReturnsAnOpenConnection()
            {
                using (var connection = Helper.CreateConnection())
                {
                    Assert.IsTrue(connection.State == ConnectionState.Open);
                }
            }

            [TestMethod]
            public void ReturnsConnectionThatCanSelectFromLocations()
            {
                using (var connection = Helper.CreateConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "select count(*) from locations";
                        command.ExecuteScalar();
                    }
                }
            }

            [TestMethod]
            public void ReturnsConnectionThatCanSelectFromIdentifiers()
            {
                using (var connection = Helper.CreateConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "select count(*) from identifiers";
                        command.ExecuteScalar();
                    }
                }
            }
        }
    }
}
