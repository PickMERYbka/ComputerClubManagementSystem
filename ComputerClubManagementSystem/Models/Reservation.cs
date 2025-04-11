using System;

namespace ComputerClubManagementSystem.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        public int ComputerId { get; set; }
        public int CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Навигационные свойства
        public Computer Computer { get; set; }
        public Customer Customer { get; set; }
        public User Employee { get; set; }

        // Вычисляемые свойства
        public string CustomerName
        {
            get
            {
                return Customer?.FullName ?? "Неизвестный клиент";
            }
        }

        public Reservation()
        {
            ReservationDate = DateTime.Today;
            StartTime = DateTime.Now;
            EndTime = DateTime.Now.AddHours(1);
            Status = "Pending";
            CreatedAt = DateTime.Now;
        }

        public Reservation(int computerId, int customerId, DateTime startTime, DateTime endTime)
        {
            ComputerId = computerId;
            CustomerId = customerId;
            ReservationDate = startTime.Date;
            StartTime = startTime;
            EndTime = endTime;
            Status = "Pending";
            CreatedAt = DateTime.Now;
        }
    }
} 