using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace SQLDatabase.Tests
{
    [TestClass]
    public class HelperClass
    {
        [TestClass]
        public class GetConnectionStringMethod
        {
            [TestMethod]
            public void ReturnsSomething()
            {
                var connectionString = Helper.GetConnectionString();
                Assert.IsNotNull(connectionString);
            }

            [TestMethod]
            public void ReturnsSyntacticallyCorrectString()
            {
                var connectionString = Helper.GetConnectionString();
                // ReSharper disable once UnusedVariable
                var a = new SqlConnectionStringBuilder(connectionString);
            }
        }
    }
}
