using System.Collections.Generic;

namespace EmiTest.Domain.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Relación Muchos a Muchos: Un proyecto tiene muchos empleados
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}