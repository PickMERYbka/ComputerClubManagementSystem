using System;

namespace ComputerClubManagementSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public User()
        {
        }

        public User(int id, string username, string fullName, string role, DateTime createdAt, bool isActive)
        {
            Id = id;
            UserId = id;
            Username = username;
            FullName = fullName;
            Role = role;
            CreatedAt = createdAt;
            IsActive = isActive;
        }
    }
} 