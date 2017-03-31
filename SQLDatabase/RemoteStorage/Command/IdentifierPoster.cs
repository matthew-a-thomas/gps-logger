using System.Collections.Generic;
using System.Threading.Tasks;
using SQLDatabase.Extensions;

namespace SQLDatabase.RemoteStorage.Command
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class IdentifierPoster : IIdentifierPoster
    {
        /// <summary>
        /// Creates a new record in the identifiers table and returns the generated ID,
        /// or selects the ID that already exists
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public async Task<int> PostOrGetIdentifierAsync(ITransaction transaction, byte[] identifier)
        {
            return await transaction.GetScalarAsync<int>(Commands.Command.Create(@"
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

declare @id int

if @@rowcount = 1
begin
    -- Return the generated ID, if a record was inserted
	set @id = scope_identity()
end
else
begin
    -- else return the ID that matches the given @hex
	select
		@id = id
	from
		identifiers
	where
		identifiers.hex = @hex
end

select @id

-- Note that while it would be faster to select to see if an ID already exists, I can't think of a quick way to do that without introducing a race condition
",
                new KeyValuePair<string, object>("@hex", identifier)));
        }
    }
}
