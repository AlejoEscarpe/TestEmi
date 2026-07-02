using System;
using System.Collections.Generic;

namespace EmiTest.Application.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CurrentPosition { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public decimal YearlyBonus { get; set; }
        public List<PositionHistoryDto> PositionHistory { get; set; } = new();
    }

    public class PositionHistoryDto
    {
        public string Position { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}