using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLDatabase.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
            public async void ReturnsConnectionThatCanSelectFromLocations()
            {
                using (var connection = Helper.CreateConnection())
                using (var transaction = new Transaction(connection))
                    await transaction.GetAsync<int>("select count(*) from locations");
            }

            [TestMethod]
            public async void ReturnsConnectionThatCanSelectFromIdentifiers()
            {
                using (var connection = Helper.CreateConnection())
                using (var transaction = new Transaction(connection))
                    await transaction.GetAsync<int>("select count(*) from identifiers");
            }

            [TestMethod]
            public async void ReturnsConnectionThatCanInsertIdentifier()
            {
                using (var connection = Helper.CreateConnection())
                using (var transaction = new Transaction(connection)) // Is automatically rolled back
                {
                    var numAffected = await transaction.ExecuteAsync("insert into identifiers(hex) values (0x0000)");
                    Assert.AreEqual(1, numAffected);
                }
            }

            [TestMethod]
            public async void ReturnsConnectionThatCanInsertLocation()
            {
                using (var connection = Helper.CreateConnection())
                using (var transaction = new Transaction(connection)) // Is automatically rolled back
                {
                    var id = transaction.GetAsync<int>("insert into identifiers(hex) values (0x0000); select cast(SCOPE_IDENTITY() as int) as id");
                    var numAffected = await transaction.ExecuteAsync("insert into locations(id, latitude, longitude) values (@id, 0, 0)", new SqlParameter("@id", id));
                    Assert.AreEqual(1, numAffected);
                }
            }
        }
    }
}
