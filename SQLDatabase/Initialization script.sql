begin transaction

create table dbo.identifiers (
	id int identity(1,1) primary key,
	hex varbinary(256) not null unique
)

create table dbo.locations (
	id int not null foreign key references identifiers(id),
	[timestamp] datetime not null default GETUTCDATE(),
	latitude float not null,
	longitude float not null
)

CREATE LOGIN test
	WITH PASSWORD = <Password here>

CREATE USER test
	FOR LOGIN test
	WITH DEFAULT_SCHEMA = dbo

CREATE ROLE dbo_read_write AUTHORIZATION [dbo]

-- Add user to the database owner role
EXEC sp_addrolemember N'dbo_read_write', N'test'

-- Grant insert,select on dbo.* to dbo_read_write
GRANT
	INSERT,
	SELECT
ON SCHEMA::dbo
	TO dbo_read_write

rollback -- Change to commit