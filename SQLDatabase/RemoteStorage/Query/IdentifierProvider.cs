using SQLDatabase.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace SQLDatabase.RemoteStorage.Query
{
    internal class IdentifierProvider
    {
        public async Task<int> GetIDFor(Transaction transaction, byte[] identifier)
        {
            return await transaction.GetAsync<int>("select id from identifiers where hex = @hex", new SqlParameter("@hex", identifier));
        }
    }
}
