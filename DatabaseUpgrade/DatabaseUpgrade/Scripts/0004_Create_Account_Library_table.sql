create table account_libraries (
	library_id varchar(100) PRIMARY KEY,
	account_id varchar(100) REFERENCES accounts(account_id),
	server_address varchar(100) NOT NULL,
	added timestamp NOT NULL,
	last_active timestamp NOT NULL
)