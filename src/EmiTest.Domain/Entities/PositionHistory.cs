using System;
using EmiTest.Domain.Enums;

namespace EmiTest.Domain.Entities
{
    public class PositionHistory
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; } 
        public string Position { get; set; } = string.Empty; 
        public PositionType PositionType { get; set; }
        public DateTime StartDate { get; set; } 
        public DateTime? EndDate { get; set; } 

        public Employee? Employee { get; set; }
    }
}