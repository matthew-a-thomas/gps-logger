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

            [TestMethod]
            public void ReturnsConnectionThatCanInsertIdentifier()
            {
                using (var connection = Helper.CreateConnection())
                {
                    using (var transaction = new Transaction(connection)) // Is automatically rolled back
                    using (var command = transaction.CreateCommand())
                    {
                        command.CommandText = "insert into identifiers(hex) values (0x0000)";
                        var numAffected = command.ExecuteNonQuery();
                        Assert.AreEqual(1, numAffected);
                    }
                }
            }

            [TestMethod]
            public void ReturnsConnectionThatCanInsertLocation()
            {
                using (var connection = Helper.CreateConnection())
                {
                    using (var transaction = new Transaction(connection)) // Is automatically rolled back
                    {
                        int id;
                        using (var command = transaction.CreateCommand())
                        {
                            command.CommandText = "insert into identifiers(hex) values (0x0000); select cast(SCOPE_IDENTITY() as int) as id";
                            id = (int)command.ExecuteScalar();
                        }
                        using (var command = transaction.CreateCommand())
                        {
                            command.CommandText = "insert into locations(id, latitude, longitude) values (@id, 0, 0)";
                            command.Parameters.AddWithValue("@id", id);
                            var numAffected = command.ExecuteNonQuery();
                            Assert.AreEqual(1, numAffected);
                        }
                    }
                }
            }
        }
    }
}
