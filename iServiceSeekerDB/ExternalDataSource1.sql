CREATE EXTERNAL DATA SOURCE [ExternalDataSource1] WITH
(  
	TYPE = RDBMS,
	LOCATION = 'myserver.database.windows.net',
	DATABASE_NAME = 'mydb',
	CREDENTIAL = [mycredential]
)