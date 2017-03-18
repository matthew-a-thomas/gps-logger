using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLDatabase.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace SQLDatabase.Tests
{
    [TestClass]
    public class TransactionClass
    {
        internal static async Task DoWithTransactionAsync(Func<Transaction, Task> action, bool allowCommits = true)
        {
            await ConnectionProviderClass.DoWithConnectionAsync(async connection =>
            {
                using (var transaction = new Transaction(connection, allowCommits))
                    await action(transaction);
            });
        }

        [TestClass]
        public class CommitMethod
        {
            [TestMethod]
            public async Task NotCommittedUntilCalled()
            {
                await DoWithTransactionAsync(async transaction =>
                {
                    var identifier = Guid.NewGuid().ToByteArray();
                    using (var command = transaction.CreateCommand())
                    {
                        command.CommandText = "insert into identifiers(hex) values (@hex)";
                        command.Parameters.AddWithValue("@hex", identifier);
                        var numAffected = await command.ExecuteNonQueryAsync();
                        Assert.AreEqual(1, numAffected);

                        // The transaction has not yet been committed
                        await DoWithTransactionAsync(async transaction2 =>
                        {
                            var count = await transaction2.GetAsync<int>("select count(*) from identifiers with (readpast) where hex = @hex", new SqlParameter("@hex", identifier));
                            Assert.AreEqual(0, count);
                        });
                    }
                });
            }

            [TestMethod]
            public async Task DoesNotCommitWhenAllowCommitsIsFalse()
            {
                await DoWithTransactionAsync(
                    action: async transaction =>
                    {
                        var identifier = Guid.NewGuid().ToByteArray();
                        using (var command = transaction.CreateCommand())
                        {
                            command.CommandText = "insert into identifiers(hex) values (@hex)";
                            command.Parameters.AddWithValue("@hex", identifier);
                            var numAffected = await command.ExecuteNonQueryAsync();
                            Assert.AreEqual(1, numAffected);

                            // Tell the transaction to commit (which shouldn't happen since we've set allowCommits to false
                            transaction.Commit();

                            await DoWithTransactionAsync(async transaction2 =>
                            {
                                var count = await transaction2.GetAsync<int>("select count(*) from identifiers with (readpast) where hex = @hex", new SqlParameter("@hex", identifier));
                                Assert.AreEqual(0, count);
                            });
                        }
                    },
                    allowCommits: false
                    );
            }
        }

        [TestClass]
        public class CreateCommandMethod
        {
            [TestMethod]
            public async Task CreatesCommand()
            {
                await DoWithTransactionAsync(async transaction =>
                {
                    await Task.Run(() =>
                    {
                        using (var command = transaction.CreateCommand())
                            Assert.IsNotNull(command);
                    });
                });
            }

            [TestMethod]
            public async Task CreatesCommandThatIsInATransaction()
            {
                await DoWithTransactionAsync(async transaction =>
                {
                    await Task.Run(() =>
                    {
                        using (var command = transaction.CreateCommand())
                            Assert.IsNotNull(command?.Transaction);
                    });
                });
            }
        }
    }
}
