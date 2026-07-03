using EmiTest.Domain.Enums;

namespace EmiTest.Application.DTOs
{
    public class CreateEmployeeDto
    {
        public string Name { get; set; } = string.Empty;
        public PositionType CurrentPosition { get; set; }
        public decimal Salary { get; set; }
        public int DepartmentId { get; set; }
    }
}