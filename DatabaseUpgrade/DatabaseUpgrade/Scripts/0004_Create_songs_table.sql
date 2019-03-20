create table account_songs (
	song_id varchar(100) PRIMARY KEY,
	account_id varchar(100) REFERENCES accounts(account_id),
	path varchar(100) NOT NULL,
	added timestamp NOT NULL,
	title varchar(100),
	artist varchar(100),
	album varchar(100),
	genre varchar(100),
	track_number int,
	disc_number int,
	rating int,
	duration_ms int,
	md5_hash varchar(100),
    size_bytes bigint
)