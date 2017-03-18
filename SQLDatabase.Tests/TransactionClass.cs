using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLDatabase.Tests
{
    [TestClass]
    public class TransactionClass
    {
        [TestClass]
        public class CreateCommandMethod
        {
            [TestMethod]
            public void CreatesCommand()
            {
                using (var connection = Helper.CreateConnection())
                using (var transaction = new Transaction(connection))
                using (var command = transaction.CreateCommand())
                    Assert.IsNotNull(command);
            }

            [TestMethod]
            public void CreatesCommandThatIsInATransaction()
            {
                using (var connection = Helper.CreateConnection())
                using (var transaction = new Transaction(connection))
                using (var command = transaction.CreateCommand())
                    Assert.IsNotNull(command?.Transaction);
            }
        }
    }
}
