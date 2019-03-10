create table account_sessions (
	session_id varchar(100) PRIMARY KEY,
	account_id varchar(100) REFERENCES accounts(account_id),
	created timestamp NOT NULL,
	invalidated boolean NOT NULL
)