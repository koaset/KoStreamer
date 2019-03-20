create table accounts (
	account_id varchar(100) primary key,
	google_id varchar(100) unique,
	email varchar(100) NOT NULL,
	name varchar(100) NOT NULL,
	created timestamp NOT NULL
)