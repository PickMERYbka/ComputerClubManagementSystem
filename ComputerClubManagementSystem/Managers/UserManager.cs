using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using ComputerClubManagementSystem.Models;

namespace ComputerClubManagementSystem.Managers
{
    public class UserManager : BaseManager
    {
        public UserManager(string connectionString) : base(connectionString) { }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(
                    "SELECT employee_id, login_username, first_name || ' ' || last_name as full_name, position as role, hire_date as created_at, is_active FROM employees ORDER BY login_username",
                    connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        FullName = reader.GetString(2),
                        Role = reader.GetString(3),
                        CreatedAt = reader.GetDateTime(4),
                        IsActive = reader.GetBoolean(5)
                    });
                }
            }

            return users;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(
                "SELECT employee_id, login_username, first_name || ' ' || last_name as full_name, position as role, hire_date as created_at, is_active FROM employees WHERE employee_id = @id",
                connection);

            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    FullName = reader.GetString(2),
                    Role = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    IsActive = reader.GetBoolean(5)
                };
            }

            return null;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(
                @"INSERT INTO employees (login_username, first_name, last_name, position, hire_date, is_active, login_password_hash)
                VALUES (@username, @firstName, @lastName, @role, @createdAt, @isActive, @passwordHash)
                RETURNING employee_id",
                connection);

            var nameParts = user.FullName.Split(' ');
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@firstName", firstName);
            command.Parameters.AddWithValue("@lastName", lastName);
            command.Parameters.AddWithValue("@role", user.Role);
            command.Parameters.AddWithValue("@createdAt", user.CreatedAt);
            command.Parameters.AddWithValue("@isActive", user.IsActive);
            command.Parameters.AddWithValue("@passwordHash", "default_password"); // Здесь нужно добавить правильное хеширование пароля

            user.Id = (int)await command.ExecuteScalarAsync();
            return user;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(
                @"UPDATE employees
                SET login_username = @username,
                    first_name = @firstName,
                    last_name = @lastName,
                    position = @role,
                    is_active = @isActive
                WHERE employee_id = @id",
                connection);

            var nameParts = user.FullName.Split(' ');
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@firstName", firstName);
            command.Parameters.AddWithValue("@lastName", lastName);
            command.Parameters.AddWithValue("@role", user.Role);
            command.Parameters.AddWithValue("@isActive", user.IsActive);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(
                "DELETE FROM employees WHERE employee_id = @id",
                connection);

            command.Parameters.AddWithValue("@id", id);
            return await command.ExecuteNonQueryAsync() > 0;
        }
    }
} 