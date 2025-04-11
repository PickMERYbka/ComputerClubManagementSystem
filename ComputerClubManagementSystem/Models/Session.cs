using System;

namespace ComputerClubManagementSystem.Models
{
    public class Session
    {
        public int SessionId { get; set; }
        public int ComputerId { get; set; }
        public int CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalCost { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        // Навигационные свойства
        public Computer Computer { get; set; }
        public Customer Customer { get; set; }
        public User Employee { get; set; }

        // Вычисляемые свойства
        public double CurrentDurationMinutes
        {
            get
            {
                if (EndTime.HasValue)
                    return (EndTime.Value - StartTime).TotalMinutes;
                else
                    return (DateTime.Now - StartTime).TotalMinutes;
            }
        }

        public decimal CurrentCost
        {
            get
            {
                if (Computer == null)
                    return 0;
                
                double hours = CurrentDurationMinutes / 60.0;
                return (decimal)hours * Computer.HourlyRate;
            }
        }

        public Session()
        {
            StartTime = DateTime.Now;
            Status = "Active";
            IsCompleted = false;
            CreatedAt = DateTime.Now;
        }

        public Session(int computerId, int customerId, int employeeId)
        {
            ComputerId = computerId;
            CustomerId = customerId;
            EmployeeId = employeeId;
            StartTime = DateTime.Now;
            Status = "Active";
            IsCompleted = false;
            CreatedAt = DateTime.Now;
        }
    }
} 