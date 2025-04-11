using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using ComputerClubManagementSystem.Models;

namespace ComputerClubManagementSystem.Managers
{
    public class SessionManager : BaseManager
    {
        public SessionManager(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<Session>> GetAllSessionsAsync()
        {
            var sessions = new List<Session>();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"SELECT s.session_id, s.computer_id, s.customer_id, s.employee_id, 
                             s.start_time, s.end_time, total_amount, s.status, s.notes, s.created_at,
                             c.station_name, c.computer_type, c.specifications, c.hourly_rate, c.status as computer_status, c.is_active as computer_is_active,
                             cu.first_name, cu.last_name, cu.phone, cu.email,
                             e.first_name || ' ' || e.last_name as employee_name
                      FROM sessions s
                      JOIN computers c ON s.computer_id = c.computer_id
                      JOIN customers cu ON s.customer_id = cu.customer_id
                      JOIN employees e ON s.employee_id = e.employee_id
                      ORDER BY s.start_time DESC",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var session = new Session
                            {
                                SessionId = reader.GetInt32(0),
                                ComputerId = reader.GetInt32(1),
                                CustomerId = reader.GetInt32(2),
                                EmployeeId = reader.GetInt32(3),
                                StartTime = reader.GetDateTime(4),
                                EndTime = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                                TotalAmount = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                                Status = reader.GetString(7),
                                Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                                CreatedAt = reader.GetDateTime(9)
                            };

                            var computer = new Computer
                            {
                                ComputerId = reader.GetInt32(1),
                                Name = reader.GetString(10),
                                ComputerType = reader.GetString(11),
                                Specifications = reader.IsDBNull(12) ? null : reader.GetString(12),
                                HourlyRate = reader.GetDecimal(13),
                                Status = reader.GetString(14),
                                IsActive = reader.GetBoolean(15)
                            };

                            var customer = new Customer
                            {
                                CustomerId = reader.GetInt32(2),
                                FirstName = reader.GetString(16),
                                LastName = reader.GetString(17),
                                Phone = reader.IsDBNull(18) ? null : reader.GetString(18),
                                Email = reader.IsDBNull(19) ? null : reader.GetString(19)
                            };

                            var employee = new User
                            {
                                UserId = reader.GetInt32(3),
                                FullName = reader.GetString(20)
                            };

                            session.Computer = computer;
                            session.Customer = customer;
                            session.Employee = employee;

                            sessions.Add(session);
                        }
                    }
                }
            }
            return sessions;
        }

        public async Task<Session> GetSessionByIdAsync(int id)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"SELECT s.session_id, s.computer_id, s.customer_id, s.employee_id, 
                             s.start_time, s.end_time, total_amount, s.status, s.notes, s.created_at,
                             c.station_name, c.computer_type, c.specifications, c.hourly_rate, c.status as computer_status, c.is_active as computer_is_active,
                             cu.first_name, cu.last_name, cu.phone, cu.email,
                             e.first_name || ' ' || e.last_name as employee_name
                      FROM sessions s
                      JOIN computers c ON s.computer_id = c.computer_id
                      JOIN customers cu ON s.customer_id = cu.customer_id
                      JOIN employees e ON s.employee_id = e.employee_id
                      WHERE s.session_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var session = new Session
                            {
                                SessionId = reader.GetInt32(0),
                                ComputerId = reader.GetInt32(1),
                                CustomerId = reader.GetInt32(2),
                                EmployeeId = reader.GetInt32(3),
                                StartTime = reader.GetDateTime(4),
                                EndTime = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                                TotalAmount = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                                Status = reader.GetString(7),
                                Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                                CreatedAt = reader.GetDateTime(9)
                            };

                            var computer = new Computer
                            {
                                ComputerId = reader.GetInt32(1),
                                Name = reader.GetString(10),
                                ComputerType = reader.GetString(11),
                                Specifications = reader.IsDBNull(12) ? null : reader.GetString(12),
                                HourlyRate = reader.GetDecimal(13),
                                Status = reader.GetString(14),
                                IsActive = reader.GetBoolean(15)
                            };

                            var customer = new Customer
                            {
                                CustomerId = reader.GetInt32(2),
                                FirstName = reader.GetString(16),
                                LastName = reader.GetString(17),
                                Phone = reader.IsDBNull(18) ? null : reader.GetString(18),
                                Email = reader.IsDBNull(19) ? null : reader.GetString(19)
                            };

                            var employee = new User
                            {
                                UserId = reader.GetInt32(3),
                                FullName = reader.GetString(20)
                            };

                            session.Computer = computer;
                            session.Customer = customer;
                            session.Employee = employee;

                            return session;
                        }
                    }
                }
            }
            return null;
        }

        public async Task<Session> CreateSessionAsync(Session session)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"INSERT INTO sessions (computer_id, customer_id, employee_id, start_time, status, notes, created_at)
                      VALUES (@computerId, @customerId, @employeeId, @startTime, @status, @notes, @createdAt)
                      RETURNING session_id",
                    connection))
                {
                    command.Parameters.AddWithValue("@computerId", session.ComputerId);
                    command.Parameters.AddWithValue("@customerId", session.CustomerId);
                    command.Parameters.AddWithValue("@employeeId", session.EmployeeId);
                    command.Parameters.AddWithValue("@startTime", session.StartTime);
                    command.Parameters.AddWithValue("@status", session.Status);
                    command.Parameters.AddWithValue("@notes", (object)session.Notes ?? DBNull.Value);
                    command.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    session.SessionId = Convert.ToInt32(await command.ExecuteScalarAsync());
                }
            }
            return session;
        }

        public async Task<decimal> EndSessionAsync(int id)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"UPDATE sessions 
                      SET end_time = @endTime, 
                          status = 'Completed', 
                          total_amount = @totalAmount
                      WHERE session_id = @id
                      RETURNING total_amount",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@endTime", DateTime.Now);
                    
                    // Получаем информацию о сессии для расчета стоимости
                    var session = await GetSessionByIdAsync(id);
                    if (session != null && session.Computer != null)
                    {
                        var duration = (DateTime.Now - session.StartTime).TotalHours;
                        var totalAmount = (decimal)duration * session.Computer.HourlyRate;
                        command.Parameters.AddWithValue("@totalAmount", totalAmount);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@totalAmount", 0);
                    }

                    return (decimal)await command.ExecuteScalarAsync();
                }
            }
        }

        public async Task<List<Session>> GetActiveSessionsAsync()
        {
            var sessions = new List<Session>();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"SELECT s.session_id, s.computer_id, s.customer_id, s.employee_id, 
                             s.start_time, s.end_time, total_amount, s.status, s.notes, s.created_at,
                             c.station_name, c.computer_type, c.specifications, c.hourly_rate, c.status as computer_status, c.is_active as computer_is_active,
                             cu.first_name, cu.last_name, cu.phone, cu.email,
                             e.first_name || ' ' || e.last_name as employee_name
                      FROM sessions s
                      JOIN computers c ON s.computer_id = c.computer_id
                      JOIN customers cu ON s.customer_id = cu.customer_id
                      JOIN employees e ON s.employee_id = e.employee_id
                      WHERE s.status = 'Active'
                      ORDER BY s.start_time DESC",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var session = new Session
                            {
                                SessionId = reader.GetInt32(0),
                                ComputerId = reader.GetInt32(1),
                                CustomerId = reader.GetInt32(2),
                                EmployeeId = reader.GetInt32(3),
                                StartTime = reader.GetDateTime(4),
                                EndTime = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                                TotalAmount = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                                Status = reader.GetString(7),
                                Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                                CreatedAt = reader.GetDateTime(9)
                            };

                            var computer = new Computer
                            {
                                ComputerId = reader.GetInt32(1),
                                Name = reader.GetString(10),
                                ComputerType = reader.GetString(11),
                                Specifications = reader.IsDBNull(12) ? null : reader.GetString(12),
                                HourlyRate = reader.GetDecimal(13),
                                Status = reader.GetString(14),
                                IsActive = reader.GetBoolean(15)
                            };

                            var customer = new Customer
                            {
                                CustomerId = reader.GetInt32(2),
                                FirstName = reader.GetString(16),
                                LastName = reader.GetString(17),
                                Phone = reader.IsDBNull(18) ? null : reader.GetString(18),
                                Email = reader.IsDBNull(19) ? null : reader.GetString(19)
                            };

                            var employee = new User
                            {
                                UserId = reader.GetInt32(3),
                                FullName = reader.GetString(20)
                            };

                            session.Computer = computer;
                            session.Customer = customer;
                            session.Employee = employee;

                            sessions.Add(session);
                        }
                    }
                }
            }
            return sessions;
        }

        public async Task<int> StartSessionAsync(int customerId, int computerId, int employeeId, string notes = null)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"INSERT INTO sessions (computer_id, customer_id, employee_id, start_time, status, notes, created_at)
                      VALUES (@computerId, @customerId, @employeeId, @startTime, @status, @notes, @createdAt)
                      RETURNING session_id",
                    connection))
                {
                    command.Parameters.AddWithValue("@computerId", computerId);
                    command.Parameters.AddWithValue("@customerId", customerId);
                    command.Parameters.AddWithValue("@employeeId", employeeId);
                    command.Parameters.AddWithValue("@startTime", DateTime.Now);
                    command.Parameters.AddWithValue("@status", "Active");
                    command.Parameters.AddWithValue("@notes", (object)notes ?? DBNull.Value);
                    command.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    return Convert.ToInt32(await command.ExecuteScalarAsync());
                }
            }
        }
    }
} 