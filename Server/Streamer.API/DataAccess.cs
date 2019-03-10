using Npgsql;
using Serilog;
using Streamer.API.Entities;
using Streamer.API.Interfaces;
using System;

namespace Streamer.API
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

                using (var cmd = new NpgsqlCommand("INSERT INTO accounts VALUES (@account_id, @google_id, @email, @name, @created)", conn))
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

        private string GetAccountDataFields =>  "account_id, google_id, email, name, created";

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
    }
}
