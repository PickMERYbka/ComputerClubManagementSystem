using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using ComputerClubManagementSystem.Models;

namespace ComputerClubManagementSystem.Managers
{
    public class CustomerManager : BaseManager
    {
        public CustomerManager(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var customers = new List<Customer>();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    "SELECT customer_id, full_name, phone, email, created_at FROM customers ORDER BY full_name",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            customers.Add(new Customer
                            {
                                CustomerId = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                Phone = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CreatedAt = reader.GetDateTime(4)
                            });
                        }
                    }
                }
            }
            return customers;
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    "SELECT customer_id, full_name, phone, email, created_at FROM customers WHERE customer_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Customer
                            {
                                CustomerId = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                Phone = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CreatedAt = reader.GetDateTime(4)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"INSERT INTO customers (full_name, phone, email, created_at)
                      VALUES (@fullName, @phone, @email, @createdAt)
                      RETURNING customer_id",
                    connection))
                {
                    command.Parameters.AddWithValue("@fullName", customer.FullName);
                    command.Parameters.AddWithValue("@phone", (object)customer.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@email", (object)customer.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    customer.CustomerId = Convert.ToInt32(await command.ExecuteScalarAsync());
                }
            }
            return customer;
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"UPDATE customers 
                      SET full_name = @fullName,
                          phone = @phone,
                          email = @email
                      WHERE customer_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", customer.CustomerId);
                    command.Parameters.AddWithValue("@fullName", customer.FullName);
                    command.Parameters.AddWithValue("@phone", (object)customer.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@email", (object)customer.Email ?? DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteCustomerAsync(int id)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    "DELETE FROM customers WHERE customer_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
} 