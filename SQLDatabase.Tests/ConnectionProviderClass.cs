using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLDatabase.Extensions;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SQLDatabase.Tests
{
    [TestClass]
    public class ConnectionProviderClass
    {
        internal static async Task DoWithConnectionAsync(Func<SqlConnection, Task> action)
        {
            var provider = new ConnectionProvider(Helper.GetConnectionString());
            using (var connection = provider.CreateConnection())
                await action(connection);
        }

        [TestClass]
        public class CreateConnectionMethod
        {
            [TestMethod]
            public async Task ReturnsAnOpenConnection()
            {
                await DoWithConnectionAsync(async connection =>
                {
                    await Task.Run(() => Assert.IsTrue(connection.State == ConnectionState.Open));
                });
            }

            [TestMethod]
            public async Task ReturnsConnectionThatCanSelectFromLocations()
            {
                await DoWithConnectionAsync(async connection =>
                {
                    using (var transaction = new Transaction(connection))
                        await transaction.GetAsync<int>("select count(*) from locations");
                });
            }

            [TestMethod]
            public async Task ReturnsConnectionThatCanSelectFromIdentifiers()
            {
                await DoWithConnectionAsync(async connection =>
                {
                    using (var transaction = new Transaction(connection))
                        await transaction.GetAsync<int>("select count(*) from identifiers");
                });
            }

            [TestMethod]
            public async Task ReturnsConnectionThatCanInsertIdentifier()
            {
                await DoWithConnectionAsync(async connection =>
                {
                    using (var transaction = new Transaction(connection)) // Is automatically rolled back
                    {
                        var numAffected = await transaction.ExecuteAsync("insert into identifiers(hex) values (0x0000)");
                        Assert.AreEqual(1, numAffected);
                    }
                });
            }

            [TestMethod]
            public async Task ReturnsConnectionThatCanInsertLocation()
            {
                await DoWithConnectionAsync(async connection =>
                {
                    using (var transaction = new Transaction(connection)) // Is automatically rolled back
                    {
                        var id = await transaction.GetAsync<int>("insert into identifiers(hex) values (0x0000); select cast(SCOPE_IDENTITY() as int) as id");
                        var numAffected = await transaction.ExecuteAsync("insert into locations(id, latitude, longitude) values (@id, 0, 0)", new SqlParameter("@id", id));
                        Assert.AreEqual(1, numAffected);
                    }
                });
            }
        }
    }
}
