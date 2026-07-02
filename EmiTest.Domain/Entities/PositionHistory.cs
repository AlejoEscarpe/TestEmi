using System;
using EmiTest.Domain.Enums;

namespace EmiTest.Domain.Entities
{
    public class PositionHistory
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; } // [cite: 24]
        public string Position { get; set; } = string.Empty; // [cite: 26]
        public PositionType PositionType { get; set; }
        public DateTime StartDate { get; set; } // [cite: 27]
        public DateTime? EndDate { get; set; } // [cite: 28]

        public Employee? Employee { get; set; }
    }
}