using System;
using System.Windows.Forms;
using System.Configuration;
using Npgsql;
using ComputerClubManagementSystem.Forms;
using ComputerClubManagementSystem.Models;

namespace ComputerClubManagementSystem
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var defaultUser = new User
            {
                UserId = 1,
                Username = "admin",
                FullName = "Администратор",
                Role = "admin",
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            Application.Run(new MainForm(connectionString, defaultUser));
        }
    }
}
