using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using ComputerClubManagementSystem.Models;

namespace ComputerClubManagementSystem.Managers
{
    public class ProductManager : BaseManager
    {
        public ProductManager(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    "SELECT product_id, name, description, price, stock_quantity, category, is_active, created_at FROM products ORDER BY name",
                    connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                ProductId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                StockQuantity = reader.GetInt32(4),
                                Category = reader.GetString(5),
                                IsActive = reader.GetBoolean(6),
                                CreatedAt = reader.GetDateTime(7)
                            });
                        }
                    }
                }
            }
            return products;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    "SELECT product_id, name, description, price, stock_quantity, category, is_active, created_at FROM products WHERE product_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Product
                            {
                                ProductId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                StockQuantity = reader.GetInt32(4),
                                Category = reader.GetString(5),
                                IsActive = reader.GetBoolean(6),
                                CreatedAt = reader.GetDateTime(7)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"INSERT INTO products (name, description, price, stock_quantity, category, is_active, created_at)
                      VALUES (@name, @description, @price, @stockQuantity, @category, @isActive, @createdAt)
                      RETURNING product_id",
                    connection))
                {
                    command.Parameters.AddWithValue("@name", product.Name);
                    command.Parameters.AddWithValue("@description", (object)product.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@price", product.Price);
                    command.Parameters.AddWithValue("@stockQuantity", product.StockQuantity);
                    command.Parameters.AddWithValue("@category", product.Category);
                    command.Parameters.AddWithValue("@isActive", true);
                    command.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    product.ProductId = Convert.ToInt32(await command.ExecuteScalarAsync());
                }
            }
            return product;
        }

        public async Task UpdateProductAsync(Product product)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"UPDATE products 
                      SET name = @name,
                          description = @description,
                          price = @price,
                          stock_quantity = @stockQuantity,
                          category = @category,
                          is_active = @isActive
                      WHERE product_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", product.ProductId);
                    command.Parameters.AddWithValue("@name", product.Name);
                    command.Parameters.AddWithValue("@description", (object)product.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@price", product.Price);
                    command.Parameters.AddWithValue("@stockQuantity", product.StockQuantity);
                    command.Parameters.AddWithValue("@category", product.Category);
                    command.Parameters.AddWithValue("@isActive", product.IsActive);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateStockQuantityAsync(int productId, int quantity)
        {
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    "UPDATE products SET stock_quantity = @quantity WHERE product_id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("@id", productId);
                    command.Parameters.AddWithValue("@quantity", quantity);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
} 