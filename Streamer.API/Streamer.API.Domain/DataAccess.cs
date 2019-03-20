using Npgsql;
using Streamer.API.Domain.Entities;
using Streamer.API.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Streamer.API.Domain
{
    public class DataAccess : IDataAccess
    {
        private string connectionString;

        public DataAccess()
        {
            var dbEnvVarKey = "DATABASE_CONNECTIONSTRING";
            connectionString = Environment.GetEnvironmentVariable(dbEnvVarKey) ?? Environment.GetEnvironmentVariable(dbEnvVarKey, EnvironmentVariableTarget.Machine);
        }

        public Account GetAccountById(string id)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"SELECT {GetAccountDataFields} FROM accounts WHERE account_id=@account_id", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("account_id", id));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadAccount(reader);
                        }
                        return null;
                    }
                }
            }
        }

        public bool IsAccountIdTaken(string id)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"SELECT 1 FROM accounts WHERE account_id=@account_id", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("account_id", id));

                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read();
                    }
                }
            }
        }

        public bool IsSessionIdTaken(string sessionId, string accountId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"SELECT 1 FROM account_sessions WHERE account_id=@account_id AND session_id=@session_id", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("session_id", accountId));
                    cmd.Parameters.Add(new NpgsqlParameter("account_id", accountId));

                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read();
                    }
                }
            }
        }

        public Account GetAccountByGoogleId(string googleId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"SELECT {GetAccountDataFields} FROM accounts WHERE google_id=@google_id", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("google_id", googleId));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadAccount(reader);
                        }
                        return null;
                    }

                }
            }
        }

        public void AddNewAccount(Account account)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("INSERT INTO accounts VALUES (@account_id, @google_id, @email, @name, @created, @user_secret)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("account_id", account.AccountId));
                    cmd.Parameters.Add(new NpgsqlParameter("google_id", account.GoogleId));
                    cmd.Parameters.Add(new NpgsqlParameter("email", account.Email));
                    cmd.Parameters.Add(new NpgsqlParameter("name", account.Name));
                    cmd.Parameters.Add(new NpgsqlParameter("created", account.CreatedDate));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string GetAccountDataFields =>  "account_id, google_id, email, name, created, user_secret";

        private Account ReadAccount(NpgsqlDataReader reader)
        {
            return new Account
            {
                AccountId = reader["account_id"].ToString(),
                GoogleId = reader["google_id"].ToString(),
                Email = reader["email"].ToString(),
                Name = reader["name"].ToString(),
                CreatedDate = ((DateTime)reader["created"]).ToUniversalTime()
            };
        }

        public Session GetSession(string sessionId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"SELECT session_id, account_id, created, invalidated FROM account_sessions WHERE session_id=@session_id", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("session_id", sessionId));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Session
                            {
                                SessionId = reader["session_id"].ToString(),
                                AccountId = reader["account_id"].ToString(),
                                CreatedDate = ((DateTime)reader["created"]).ToUniversalTime(),
                                Invalidated = (bool)reader["invalidated"]
                            };
                        }
                        return null;
                    }

                }
            }
        }

        public void AddSession(Session session)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("INSERT INTO account_sessions VALUES (@session_id, @account_id, @created, @invalidated)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("session_id", session.SessionId));
                    cmd.Parameters.Add(new NpgsqlParameter("account_id", session.AccountId));
                    cmd.Parameters.Add(new NpgsqlParameter("created", session.CreatedDate));
                    cmd.Parameters.Add(new NpgsqlParameter("invalidated", session.Invalidated));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InvalidateSession(string sessionId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("UPDATE account_sessions SET invalidated=true WHERE session_id = @session_id", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("session_id", sessionId));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Song> GetSongsForUser(Account userAccount)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"SELECT * FROM account_songs WHERE account_id=@account_id", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("account_id", userAccount.AccountId));

                    using (var reader = cmd.ExecuteReader())
                    {
                        var ret = new List<Song>();
                        while (reader.Read())
                        {
                            ret.Add(new Song
                            {
                                Id = reader["song_id"].ToString(),
                                Path = reader["path"].ToString(),
                                DateAdded = ((DateTime)reader["added"]).ToUniversalTime(),
                                Title = reader["title"].ToString(),
                                Artist = reader["artist"].ToString(),
                                Album = reader["album"].ToString(),
                                Genre = reader["genre"].ToString(),
                                TrackNumber = (int?)reader["track_number"],
                                DiscNumber = (int?)reader["disc_number"],
                                Rating = (int?)reader["rating"],
                                DurationMs = (int)reader["duration_ms"],
                                Md5Hash = reader["md5_hash"].ToString(),
                                SizeBytes = (long)reader["size_bytes"]
                            });
                        }
                        return ret;
                    }
                }
            }
        }

        public void AddSongForUser(Song song, Account userAccount)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("INSERT INTO account_songs VALUES " +
                    "(@song_id, @account_id, @path, @added, @title, @artist, @album, @genre, @track_number, @disc_number, @rating, @duration_ms, @md5_hash, @size_bytes)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("song_id", song.Id));
                    cmd.Parameters.Add(new NpgsqlParameter("account_id", userAccount.AccountId));
                    cmd.Parameters.Add(new NpgsqlParameter("path", song.Path));
                    cmd.Parameters.Add(new NpgsqlParameter("added", song.DateAdded));
                    cmd.Parameters.Add(new NpgsqlParameter("title", song.Title));
                    cmd.Parameters.Add(new NpgsqlParameter("artist", song.Artist));
                    cmd.Parameters.Add(new NpgsqlParameter("album", song.Album));
                    cmd.Parameters.Add(new NpgsqlParameter("genre", song.Genre));
                    cmd.Parameters.Add(new NpgsqlParameter("track_number", song.TrackNumber));
                    cmd.Parameters.Add(new NpgsqlParameter("disc_number", song.DiscNumber));
                    cmd.Parameters.Add(new NpgsqlParameter("rating", song.Rating));
                    cmd.Parameters.Add(new NpgsqlParameter("duration_ms", song.DurationMs));
                    cmd.Parameters.Add(new NpgsqlParameter("duration_ms", song.DurationMs));
                    cmd.Parameters.Add(new NpgsqlParameter("md5_hash", song.Md5Hash));
                    cmd.Parameters.Add(new NpgsqlParameter("size_bytes", song.SizeBytes));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Song GetSongByMd5HashForUser(string md5Hash, Account userAccount)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"SELECT * FROM account_songs WHERE account_id=@account_id AND md5_hash=@md5_hash limit 1", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("account_id", userAccount.AccountId));
                    cmd.Parameters.Add(new NpgsqlParameter("md5_hash", md5Hash));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Song
                            {
                                Id = reader["song_id"].ToString(),
                                Path = reader["path"].ToString(),
                                DateAdded = ((DateTime)reader["added"]).ToUniversalTime(),
                                Title = reader["title"].ToString(),
                                Artist = reader["artist"].ToString(),
                                Album = reader["album"].ToString(),
                                Genre = reader["genre"].ToString(),
                                TrackNumber = (int?)reader["track_number"],
                                DiscNumber = (int?)reader["disc_number"],
                                Rating = (int?)reader["rating"],
                                DurationMs = (int)reader["duration_ms"],
                                Md5Hash = reader["md5_hash"].ToString()
                            };
                        }
                        return null;
                    }
                }
            }
        }
    }
}
