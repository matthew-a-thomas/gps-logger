﻿using SQLDatabase.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace SQLDatabase.RemoteStorage.Command
{
    internal class IdentifierPoster
    {
        /// <summary>
        /// Creates a new record in the identifiers table and returns the generated ID,
        /// or selects the ID that already exists
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public async Task<int> PostOrGetIdentifierAsync(Transaction transaction, byte[] identifier)
        {
            return await transaction.GetAsync<int>(@"
-- Insert the given @hex into [identifiers] if it isn't already there
insert
	identifiers (hex)
select
	data.hex
from
	( select @hex as hex ) as data
	left join identifiers on
		identifiers.hex = data.hex
where
	identifiers.id is null

if @@rowcount = 1
begin
    -- Return the generated ID, if a record was inserted
	select scope_identity()
end
else
begin
    -- else return the ID that matches the given @hex
	select
		id
	from
		identifiers
	where
		identifiers.hex = @hex
end

-- Note that while it would be faster to select to see if an ID already exists, I can't think of a quick way to do that without introducing a race condition
",
            new SqlParameter("@hex", identifier));
        }
    }
}