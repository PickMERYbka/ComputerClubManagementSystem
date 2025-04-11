using System;

namespace ComputerClubManagementSystem.Models
{
    public static class CurrentUser
    {
        public static int EmployeeId { get; set; }
        public static string Username { get; set; }
        public static string FullName { get; set; }
        public static string Role { get; set; }

        public static void SetUser(User user)
        {
            EmployeeId = user.UserId;
            Username = user.Username;
            FullName = user.FullName;
            Role = user.Role;
        }

        public static void Clear()
        {
            EmployeeId = 0;
            Username = null;
            FullName = null;
            Role = null;
        }
    }
} 