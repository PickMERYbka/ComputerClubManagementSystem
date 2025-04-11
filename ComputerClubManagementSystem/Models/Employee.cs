using System;

namespace ComputerClubManagementSystem.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public bool IsActive { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public Employee()
        {
            HireDate = DateTime.Now;
            IsActive = true;
        }
    }
} 