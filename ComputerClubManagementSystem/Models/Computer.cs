using System;

namespace ComputerClubManagementSystem.Models
{
    public class Computer
    {
        public int ComputerId { get; set; }
        public string Name { get; set; }
        public string StationName 
        { 
            get => Name;
            set => Name = value;
        }
        public string ComputerType { get; set; }
        public string Specifications { get; set; }
        public decimal HourlyRate { get; set; }
        public string Status { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public Computer()
        {
            CreatedAt = DateTime.Now;
            IsActive = true;
            Status = "Available";
        }
    }
} 