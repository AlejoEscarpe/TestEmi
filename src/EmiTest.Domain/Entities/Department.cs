using System.Collections.Generic;

namespace EmiTest.Domain.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}