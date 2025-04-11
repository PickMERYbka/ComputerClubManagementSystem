using System;
using System.Linq;

namespace ComputerClubManagementSystem.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName 
        { 
            get => $"{FirstName} {LastName}".Trim();
            set
            {
                var parts = (value ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                FirstName = parts.Length > 0 ? parts[0] : "";
                LastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";
            }
        }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string MembershipStatus { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public Customer()
        {
            CreatedAt = DateTime.Now;
            MembershipStatus = "Standard";
        }

        public Customer(string firstName, string lastName, string phone = null, string email = null)
        {
            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
            Email = email;
            CreatedAt = DateTime.Now;
            MembershipStatus = "Standard";
        }

        public Customer(string fullName, string phone = null, string email = null)
        {
            FullName = fullName;
            Phone = phone;
            Email = email;
            CreatedAt = DateTime.Now;
            MembershipStatus = "Standard";
        }
    }
} 