using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLDatabase.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

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
            public async Task ReturnsConnectionThatCanSelectFromLocations()
            {
                using (var connection = Helper.CreateConnection())
                using (var transaction = new Transaction(connection))
                    await transaction.GetAsync<int>("select count(*) from locations");
            }

            [TestMethod]
            public async Task ReturnsConnectionThatCanSelectFromIdentifiers()
            {
                using (var connection = Helper.CreateConnection())
                using (var transaction = new Transaction(connection))
                    await transaction.GetAsync<int>("select count(*) from identifiers");
            }

            [TestMethod]
            public async Task ReturnsConnectionThatCanInsertIdentifier()
            {
                using (var connection = Helper.CreateConnection())
                using (var transaction = new Transaction(connection)) // Is automatically rolled back
                {
                    var numAffected = await transaction.ExecuteAsync("insert into identifiers(hex) values (0x0000)");
                    Assert.AreEqual(1, numAffected);
                }
            }

            [TestMethod]
            public async Task ReturnsConnectionThatCanInsertLocation()
            {
                using (var connection = Helper.CreateConnection())
                using (var transaction = new Transaction(connection)) // Is automatically rolled back
                {
                    var id = await transaction.GetAsync<int>("insert into identifiers(hex) values (0x0000); select cast(SCOPE_IDENTITY() as int) as id");
                    var numAffected = await transaction.ExecuteAsync("insert into locations(id, latitude, longitude) values (@id, 0, 0)", new SqlParameter("@id", id));
                    Assert.AreEqual(1, numAffected);
                }
            }
        }
    }
}
