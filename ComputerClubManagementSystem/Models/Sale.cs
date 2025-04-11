using System;

namespace ComputerClubManagementSystem.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime SaleDate { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Навигационные свойства
        public Product Product { get; set; }
        public Customer Customer { get; set; }
        public User Employee { get; set; }

        public Sale()
        {
            SaleDate = DateTime.Now;
            CreatedAt = DateTime.Now;
        }

        public Sale(int productId, int customerId, int employeeId, int quantity, decimal unitPrice)
        {
            ProductId = productId;
            CustomerId = customerId;
            EmployeeId = employeeId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Price = unitPrice;
            TotalAmount = quantity * unitPrice;
            SaleDate = DateTime.Now;
            CreatedAt = DateTime.Now;
        }
    }
} 