using System.Collections.Generic;

namespace EmiTest.Domain.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Relación Uno a Muchos: Un departamento tiene muchos empleados
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}