using System;

namespace ComputerClubManagementSystem.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public Product()
        {
            IsActive = true;
            CreatedAt = DateTime.Now;
        }

        public Product(string name, string description, decimal price, int stockQuantity, string category)
        {
            Name = name;
            Description = description;
            Price = price;
            StockQuantity = stockQuantity;
            Category = category;
            IsActive = true;
            CreatedAt = DateTime.Now;
        }
    }
} 