using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using ComputerClubManagementSystem.Models;

namespace ComputerClubManagementSystem.Managers
{
    public class ReservationManager : BaseManager
    {
        public ReservationManager(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<Reservation>> GetAllReservationsAsync()
        {
            var reservations = new List<Reservation>();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"SELECT r.reservation_id, r.customer_id, r.computer_id, r.reservation_date, 
                             r.start_time, r.end_time, r.status, r.notes, r.employee_id, r.created_at,
                             c.first_name, c.last_name, comp.name, e.first_name || ' ' || e.last_name as employee_name
                      FROM reservations r
                      LEFT JOIN customers c ON r.customer_id = c.customer_id
                      LEFT JOIN computers comp ON r.computer_id = comp.computer_id
                      LEFT JOIN employees e ON r.created_by = e.employee_id
                      ORDER BY r.reservation_date DESC, r.start_time",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var reservation = new Reservation
                            {
                                ReservationId = reader.GetInt32(0),
                                CustomerId = reader.GetInt32(1),
                                ComputerId = reader.GetInt32(2),
                                ReservationDate = reader.GetDateTime(3),
                                StartTime = reader.GetDateTime(3).Date.Add(reader.GetTimeSpan(4)),
                                EndTime = reader.GetDateTime(3).Date.Add(reader.GetTimeSpan(5)),
                                Status = reader.GetString(6),
                                Notes = reader.IsDBNull(7) ? null : reader.GetString(7),
                                EmployeeId = reader.GetInt32(8),
                                CreatedAt = reader.GetDateTime(9)
                            };

                            // Добавляем навигационные свойства
                            reservation.Customer = new Customer
                            {
                                CustomerId = reservation.CustomerId,
                                FirstName = reader.GetString(10),
                                LastName = reader.GetString(11)
                            };

                            reservation.Computer = new Computer
                            {
                                ComputerId = reservation.ComputerId,
                                Name = reader.GetString(12)
                            };

                            reservation.Employee = new User
                            {
                                Id = reservation.EmployeeId,
                                FullName = reader.GetString(13)
                            };

                            reservations.Add(reservation);
                        }
                    }
                }
            }
            return reservations;
        }

        public async Task<Reservation> GetReservationByIdAsync(int id)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"SELECT r.reservation_id, r.customer_id, r.computer_id, r.reservation_date, 
                             r.start_time, r.end_time, r.status, r.notes, r.employee_id, r.created_at,
                             c.first_name, c.last_name, comp.name, e.first_name || ' ' || e.last_name as employee_name
                      FROM reservations r
                      LEFT JOIN customers c ON r.customer_id = c.customer_id
                      LEFT JOIN computers comp ON r.computer_id = comp.computer_id
                      LEFT JOIN employees e ON r.created_by = e.employee_id
                      WHERE r.reservation_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var reservation = new Reservation
                            {
                                ReservationId = reader.GetInt32(0),
                                CustomerId = reader.GetInt32(1),
                                ComputerId = reader.GetInt32(2),
                                ReservationDate = reader.GetDateTime(3),
                                StartTime = reader.GetDateTime(3).Date.Add(reader.GetTimeSpan(4)),
                                EndTime = reader.GetDateTime(3).Date.Add(reader.GetTimeSpan(5)),
                                Status = reader.GetString(6),
                                Notes = reader.IsDBNull(7) ? null : reader.GetString(7),
                                EmployeeId = reader.GetInt32(8),
                                CreatedAt = reader.GetDateTime(9)
                            };

                            // Добавляем навигационные свойства
                            reservation.Customer = new Customer
                            {
                                CustomerId = reservation.CustomerId,
                                FirstName = reader.GetString(10),
                                LastName = reader.GetString(11)
                            };

                            reservation.Computer = new Computer
                            {
                                ComputerId = reservation.ComputerId,
                                Name = reader.GetString(12)
                            };

                            reservation.Employee = new User
                            {
                                Id = reservation.EmployeeId,
                                FullName = reader.GetString(13)
                            };

                            return reservation;
                        }
                    }
                }
            }
            return null;
        }

        public async Task<int> CreateReservationAsync(
            int customerId,
            int computerId,
            DateTime reservationDate,
            TimeSpan startTime,
            TimeSpan endTime,
            string notes,
            int employeeId)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"INSERT INTO reservations (
                        customer_id, computer_id, reservation_date, 
                        start_time, end_time, notes, employee_id, 
                        status, created_at
                    ) VALUES (
                        @customerId, @computerId, @reservationDate,
                        @startTime, @endTime, @notes, @employeeId,
                        'Pending', CURRENT_TIMESTAMP
                    ) RETURNING reservation_id",
                    connection))
                {
                    command.Parameters.AddWithValue("@customerId", customerId);
                    command.Parameters.AddWithValue("@computerId", computerId);
                    command.Parameters.AddWithValue("@reservationDate", reservationDate.Date);
                    command.Parameters.AddWithValue("@startTime", startTime);
                    command.Parameters.AddWithValue("@endTime", endTime);
                    command.Parameters.AddWithValue("@notes", (object)notes ?? DBNull.Value);
                    command.Parameters.AddWithValue("@employeeId", employeeId);

                    return Convert.ToInt32(await command.ExecuteScalarAsync());
                }
            }
        }

        public async Task UpdateReservationAsync(Reservation reservation)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"UPDATE reservations 
                      SET customer_id = @customerId, 
                          computer_id = @computerId, 
                          reservation_date = @reservationDate, 
                          start_time = @startTime, 
                          end_time = @endTime, 
                          status = @status, 
                          notes = @notes 
                      WHERE reservation_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", reservation.ReservationId);
                    command.Parameters.AddWithValue("@customerId", reservation.CustomerId);
                    command.Parameters.AddWithValue("@computerId", reservation.ComputerId);
                    command.Parameters.AddWithValue("@reservationDate", reservation.ReservationDate);
                    command.Parameters.AddWithValue("@startTime", reservation.StartTime);
                    command.Parameters.AddWithValue("@endTime", reservation.EndTime);
                    command.Parameters.AddWithValue("@status", reservation.Status);
                    command.Parameters.AddWithValue("@notes", (object)reservation.Notes ?? DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task CancelReservationAsync(int id)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    "UPDATE reservations SET status = 'Cancelled' WHERE reservation_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Reservation>> GetUpcomingReservationsAsync()
        {
            var reservations = new List<Reservation>();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"SELECT r.reservation_id, r.customer_id, r.computer_id, r.reservation_date, 
                             r.start_time, r.end_time, r.status, r.notes, r.employee_id, r.created_at,
                             c.first_name, c.last_name, comp.name, e.first_name || ' ' || e.last_name as employee_name
                      FROM reservations r
                      LEFT JOIN customers c ON r.customer_id = c.customer_id
                      LEFT JOIN computers comp ON r.computer_id = comp.computer_id
                      LEFT JOIN employees e ON r.created_by = e.employee_id
                      WHERE r.reservation_date >= @today AND r.status = 'Pending'
                      ORDER BY r.reservation_date, r.start_time",
                    connection))
                {
                    command.Parameters.AddWithValue("@today", DateTime.Today);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var reservation = new Reservation
                            {
                                ReservationId = reader.GetInt32(0),
                                CustomerId = reader.GetInt32(1),
                                ComputerId = reader.GetInt32(2),
                                ReservationDate = reader.GetDateTime(3),
                                StartTime = reader.GetDateTime(3).Date.Add(reader.GetTimeSpan(4)),
                                EndTime = reader.GetDateTime(3).Date.Add(reader.GetTimeSpan(5)),
                                Status = reader.GetString(6),
                                Notes = reader.IsDBNull(7) ? null : reader.GetString(7),
                                EmployeeId = reader.GetInt32(8),
                                CreatedAt = reader.GetDateTime(9)
                            };

                            // Добавляем навигационные свойства
                            reservation.Customer = new Customer
                            {
                                CustomerId = reservation.CustomerId,
                                FirstName = reader.GetString(10),
                                LastName = reader.GetString(11)
                            };

                            reservation.Computer = new Computer
                            {
                                ComputerId = reservation.ComputerId,
                                Name = reader.GetString(12)
                            };

                            reservation.Employee = new User
                            {
                                Id = reservation.EmployeeId,
                                FullName = reader.GetString(13)
                            };

                            reservations.Add(reservation);
                        }
                    }
                }
            }
            return reservations;
        }

        public async Task<List<Reservation>> GetReservationsByDateAsync(DateTime date)
        {
            var reservations = new List<Reservation>();
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = await GetConnectionAsync();
                cmd.CommandText = @"
                    SELECT r.reservation_id, r.computer_id, r.customer_id, r.start_time, r.end_time, 
                           r.status, r.notes, r.created_at,
                           c.station_name, c.status as computer_status, c.is_active as computer_is_active,
                           cu.first_name, cu.last_name, cu.phone, cu.email
                    FROM reservations r
                    JOIN computers c ON r.computer_id = c.computer_id
                    JOIN customers cu ON r.customer_id = cu.customer_id
                    WHERE DATE(r.start_time) = @date
                    ORDER BY r.start_time";

                cmd.Parameters.AddWithValue("@date", date.Date);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var reservation = new Reservation
                        {
                            ReservationId = reader.GetInt32(reader.GetOrdinal("reservation_id")),
                            ComputerId = reader.GetInt32(reader.GetOrdinal("computer_id")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("customer_id")),
                            StartTime = reader.GetDateTime(reader.GetOrdinal("start_time")),
                            EndTime = reader.GetDateTime(reader.GetOrdinal("end_time")),
                            Status = reader.GetString(reader.GetOrdinal("status")),
                            Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                            Computer = new Computer
                            {
                                ComputerId = reader.GetInt32(reader.GetOrdinal("computer_id")),
                                StationName = reader.GetString(reader.GetOrdinal("station_name")),
                                Status = reader.GetString(reader.GetOrdinal("computer_status")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("computer_is_active"))
                            },
                            Customer = new Customer
                            {
                                CustomerId = reader.GetInt32(reader.GetOrdinal("customer_id")),
                                FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                                LastName = reader.GetString(reader.GetOrdinal("last_name")),
                                Phone = reader.GetString(reader.GetOrdinal("phone")),
                                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email"))
                            }
                        };
                        reservations.Add(reservation);
                    }
                }
            }
            return reservations;
        }

        public async Task<List<Reservation>> GetActiveReservationsAsync()
        {
            var reservations = new List<Reservation>();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    "SELECT reservation_id, computer_id, customer_id, employee_id, reservation_date, start_time, end_time, status, notes, created_at FROM reservations WHERE status = 'Active'",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            reservations.Add(new Reservation
                            {
                                ReservationId = reader.GetInt32(0),
                                ComputerId = reader.GetInt32(1),
                                CustomerId = reader.GetInt32(2),
                                EmployeeId = reader.GetInt32(3),
                                ReservationDate = reader.GetDateTime(4),
                                StartTime = reader.GetDateTime(5),
                                EndTime = reader.GetDateTime(6),
                                Status = reader.GetString(7),
                                Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                                CreatedAt = reader.GetDateTime(9)
                            });
                        }
                    }
                }
            }
            return reservations;
        }
    }
} 