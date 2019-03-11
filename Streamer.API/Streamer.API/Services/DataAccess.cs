using Npgsql;
using Streamer.API.Entities;
using Streamer.API.Interfaces;
using System;

namespace Streamer.API.Services
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

        public Account GetAccountByUserSecret(string userSecret)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"SELECT {GetAccountDataFields} FROM accounts WHERE user_secret=@user_secret", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("user_secret", userSecret));

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
                    cmd.Parameters.Add(new NpgsqlParameter("user_secret", account.UserSecret));
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
                CreatedDate = ((DateTime)reader["created"]).ToUniversalTime(),
                UserSecret = reader["user_secret"].ToString()
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

        public void AddLibrary(AccountLibrary library)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("INSERT INTO account_libraries VALUES (@library_id, @account_id, @server_address, @added, @last_active)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("library_id", library.LibraryId));
                    cmd.Parameters.Add(new NpgsqlParameter("account_id", library.AccountId));
                    cmd.Parameters.Add(new NpgsqlParameter("server_address", library.ServerAddress));
                    cmd.Parameters.Add(new NpgsqlParameter("added", library.DateAdded));
                    cmd.Parameters.Add(new NpgsqlParameter("last_active", library.LastActive));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public AccountLibrary GetLibrary(string accountId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"SELECT library_id, account_id, server_address, added, last_active FROM account_libraries WHERE account_id=@account_id", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("account_id", accountId));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new AccountLibrary
                            {
                                
                                LibraryId = reader["library_id"].ToString(),
                                AccountId = reader["account_id"].ToString(),
                                ServerAddress = reader["server_address"].ToString(),
                                DateAdded = ((DateTime)reader["added"]).ToUniversalTime(),
                                LastActive = ((DateTime)reader["last_active"]).ToUniversalTime()
                            };
                        }
                        return null;
                    }

                }
            }
        }
    }
}
