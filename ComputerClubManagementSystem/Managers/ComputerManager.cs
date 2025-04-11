using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using ComputerClubManagementSystem.Models;

namespace ComputerClubManagementSystem.Managers
{
    public class ComputerManager : BaseManager
    {
        public ComputerManager(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<Computer>> GetAllComputersAsync()
        {
            var computers = new List<Computer>();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    "SELECT computer_id, station_name, computer_type, specifications, hourly_rate, status, last_maintenance_date, purchase_date, is_active FROM computers ORDER BY station_name",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            computers.Add(new Computer
                            {
                                ComputerId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                ComputerType = reader.GetString(2),
                                Specifications = reader.IsDBNull(3) ? null : reader.GetString(3),
                                HourlyRate = reader.GetDecimal(4),
                                Status = reader.GetString(5),
                                LastMaintenanceDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                PurchaseDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                                IsActive = reader.GetBoolean(8)
                            });
                        }
                    }
                }
            }
            return computers;
        }

        public async Task<Computer> GetComputerByIdAsync(int id)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    "SELECT computer_id, station_name, computer_type, specifications, hourly_rate, status, last_maintenance_date, purchase_date, is_active FROM computers WHERE computer_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Computer
                            {
                                ComputerId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                ComputerType = reader.GetString(2),
                                Specifications = reader.IsDBNull(3) ? null : reader.GetString(3),
                                HourlyRate = reader.GetDecimal(4),
                                Status = reader.GetString(5),
                                LastMaintenanceDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                PurchaseDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                                IsActive = reader.GetBoolean(8)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<Computer> CreateComputerAsync(Computer computer)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"INSERT INTO computers (station_name, computer_type, specifications, hourly_rate, status, last_maintenance_date, purchase_date, is_active) 
                      VALUES (@stationName, @computerType, @specifications, @hourlyRate, @status, @lastMaintenanceDate, @purchaseDate, @isActive) 
                      RETURNING computer_id",
                    connection))
                {
                    command.Parameters.AddWithValue("@stationName", computer.Name);
                    command.Parameters.AddWithValue("@computerType", computer.ComputerType);
                    command.Parameters.AddWithValue("@specifications", (object)computer.Specifications ?? DBNull.Value);
                    command.Parameters.AddWithValue("@hourlyRate", computer.HourlyRate);
                    command.Parameters.AddWithValue("@status", computer.Status);
                    command.Parameters.AddWithValue("@lastMaintenanceDate", (object)computer.LastMaintenanceDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@purchaseDate", (object)computer.PurchaseDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@isActive", true);

                    computer.ComputerId = Convert.ToInt32(await command.ExecuteScalarAsync());
                }
            }
            return computer;
        }

        public async Task UpdateComputerAsync(Computer computer)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"UPDATE computers 
                      SET station_name = @stationName, 
                          computer_type = @computerType, 
                          specifications = @specifications, 
                          hourly_rate = @hourlyRate, 
                          status = @status, 
                          last_maintenance_date = @lastMaintenanceDate, 
                          purchase_date = @purchaseDate, 
                          is_active = @isActive 
                      WHERE computer_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", computer.ComputerId);
                    command.Parameters.AddWithValue("@stationName", computer.Name);
                    command.Parameters.AddWithValue("@computerType", computer.ComputerType);
                    command.Parameters.AddWithValue("@specifications", (object)computer.Specifications ?? DBNull.Value);
                    command.Parameters.AddWithValue("@hourlyRate", computer.HourlyRate);
                    command.Parameters.AddWithValue("@status", computer.Status);
                    command.Parameters.AddWithValue("@lastMaintenanceDate", (object)computer.LastMaintenanceDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@purchaseDate", (object)computer.PurchaseDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@isActive", computer.IsActive);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteComputerAsync(int id)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    "DELETE FROM computers WHERE computer_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task MarkForMaintenanceAsync(int id)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"UPDATE computers 
                      SET status = 'Maintenance', 
                          last_maintenance_date = @maintenanceDate 
                      WHERE computer_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@maintenanceDate", DateTime.Now);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Computer>> GetAvailableComputersAsync()
        {
            var computers = new List<Computer>();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    "SELECT computer_id, station_name, computer_type, specifications, hourly_rate, status, last_maintenance_date, purchase_date, is_active FROM computers WHERE status = 'Available' AND is_active = true ORDER BY station_name",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            computers.Add(new Computer
                            {
                                ComputerId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                ComputerType = reader.GetString(2),
                                Specifications = reader.IsDBNull(3) ? null : reader.GetString(3),
                                HourlyRate = reader.GetDecimal(4),
                                Status = reader.GetString(5),
                                LastMaintenanceDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                PurchaseDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                                IsActive = reader.GetBoolean(8)
                            });
                        }
                    }
                }
            }
            return computers;
        }
    }
} 