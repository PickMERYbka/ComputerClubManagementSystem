using System;

namespace ComputerClubManagementSystem.Models
{
    public class SaleDetail
    {
        public int SaleId { get; set; }
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime SaleDate { get; set; }
        public string EmployeeName { get; set; }
        public string Notes { get; set; }
    }
} 