using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using ComputerClubManagementSystem.Models;

namespace ComputerClubManagementSystem.Managers
{
    public class SalesManager : BaseManager
    {
        private readonly NpgsqlConnection _connection;

        public SalesManager(string connectionString) : base(connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
            _connection.Open();
        }

        public async Task<List<SaleDetail>> GetAllSalesAsync()
        {
            var sales = new List<SaleDetail>();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"SELECT s.sale_id, c.first_name || ' ' || c.last_name as customer_name, 
                             p.name as product_name, si.quantity, si.unit_price as price, 
                             total_amount, s.sale_date, 
                             e.first_name || ' ' || e.last_name as employee_name, s.notes
                      FROM sales s
                      JOIN sale_items si ON s.sale_id = si.sale_id
                      JOIN customers c ON s.customer_id = c.customer_id
                      JOIN products p ON si.product_id = p.product_id
                      JOIN employees e ON s.employee_id = e.employee_id
                      ORDER BY s.sale_date DESC",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            sales.Add(new SaleDetail
                            {
                                SaleId = reader.GetInt32(0),
                                CustomerName = reader.GetString(1),
                                ProductName = reader.GetString(2),
                                Quantity = reader.GetInt32(3),
                                Price = reader.GetDecimal(4),
                                TotalAmount = reader.GetDecimal(5),
                                SaleDate = reader.GetDateTime(6),
                                EmployeeName = reader.GetString(7),
                                Notes = reader.IsDBNull(8) ? null : reader.GetString(8)
                            });
                        }
                    }
                }
            }
            return sales;
        }

        public async Task<SaleDetail> GetSaleByIdAsync(int id)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"SELECT s.sale_id, c.first_name || ' ' || c.last_name as customer_name, 
                             p.name as product_name, si.quantity, si.unit_price as price, 
                             total_amount, s.sale_date, 
                             e.first_name || ' ' || e.last_name as employee_name, s.notes
                      FROM sales s
                      JOIN sale_items si ON s.sale_id = si.sale_id
                      JOIN customers c ON s.customer_id = c.customer_id
                      JOIN products p ON si.product_id = p.product_id
                      JOIN employees e ON s.employee_id = e.employee_id
                      WHERE s.sale_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new SaleDetail
                            {
                                SaleId = reader.GetInt32(0),
                                CustomerName = reader.GetString(1),
                                ProductName = reader.GetString(2),
                                Quantity = reader.GetInt32(3),
                                Price = reader.GetDecimal(4),
                                TotalAmount = reader.GetDecimal(5),
                                SaleDate = reader.GetDateTime(6),
                                EmployeeName = reader.GetString(7),
                                Notes = reader.IsDBNull(8) ? null : reader.GetString(8)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<Sale> CreateSaleAsync(Sale sale)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"INSERT INTO sales (customer_id, product_id, quantity, price, total_amount, 
                                       sale_date, employee_id, notes)
                      VALUES (@customerId, @productId, @quantity, @price, @totalAmount, 
                             @saleDate, @employeeId, @notes)
                      RETURNING sale_id",
                    connection))
                {
                    command.Parameters.AddWithValue("@customerId", sale.CustomerId);
                    command.Parameters.AddWithValue("@productId", sale.ProductId);
                    command.Parameters.AddWithValue("@quantity", sale.Quantity);
                    command.Parameters.AddWithValue("@price", sale.Price);
                    command.Parameters.AddWithValue("@totalAmount", sale.TotalAmount);
                    command.Parameters.AddWithValue("@saleDate", DateTime.Now);
                    command.Parameters.AddWithValue("@employeeId", sale.EmployeeId);
                    command.Parameters.AddWithValue("@notes", (object)sale.Notes ?? DBNull.Value);

                    sale.SaleId = Convert.ToInt32(await command.ExecuteScalarAsync());
                }
            }
            return sale;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var productManager = new ProductManager(_connectionString);
            return await productManager.GetAllProductsAsync();
        }

        public async Task<SaleDetail> AddSaleAsync(int productId, int customerId, int quantity)
        {
            // Получаем информацию о товаре
            decimal price = 0;
            using (var cmd = new NpgsqlCommand(
                "SELECT price FROM products WHERE product_id = @productId", _connection))
            {
                cmd.Parameters.AddWithValue("@productId", productId);
                price = (decimal)await cmd.ExecuteScalarAsync();
            }

            var totalAmount = price * quantity;
            var saleDate = DateTime.Now;

            // Добавляем продажу
            int saleId;
            using (var cmd = new NpgsqlCommand(
                @"INSERT INTO sales (customer_id, employee_id, sale_date, total_amount)
                  VALUES (@customerId, @employeeId, @saleDate, @totalAmount)
                  RETURNING sale_id", _connection))
            {
                cmd.Parameters.AddWithValue("@customerId", customerId);
                cmd.Parameters.AddWithValue("@employeeId", 1); // TODO: Использовать ID текущего пользователя
                cmd.Parameters.AddWithValue("@saleDate", saleDate);
                cmd.Parameters.AddWithValue("@totalAmount", totalAmount);

                saleId = (int)await cmd.ExecuteScalarAsync();
            }

            // Добавляем детали продажи
            using (var cmd = new NpgsqlCommand(
                @"INSERT INTO sale_items (sale_id, product_id, quantity, unit_price, subtotal)
                  VALUES (@saleId, @productId, @quantity, @price, @subtotal)", _connection))
            {
                cmd.Parameters.AddWithValue("@saleId", saleId);
                cmd.Parameters.AddWithValue("@productId", productId);
                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@subtotal", totalAmount);
                await cmd.ExecuteNonQueryAsync();
            }

            // Обновляем количество товара
            using (var cmd = new NpgsqlCommand(
                @"UPDATE products 
                  SET stock_quantity = stock_quantity - @quantity
                  WHERE product_id = @productId", _connection))
            {
                cmd.Parameters.AddWithValue("@productId", productId);
                cmd.Parameters.AddWithValue("@quantity", quantity);
                await cmd.ExecuteNonQueryAsync();
            }

            // Возвращаем детали созданной продажи
            return await GetSaleByIdAsync(saleId);
        }
    }
} 