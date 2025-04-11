using System;
using Npgsql;

namespace ComputerClubManagementSystem.Managers
{
    public abstract class BaseManager
    {
        protected readonly string _connectionString;

        protected BaseManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        protected async Task<NpgsqlConnection> GetConnectionAsync()
        {
            var connection = GetConnection();
            await connection.OpenAsync();
            return connection;
        }
    }
} 