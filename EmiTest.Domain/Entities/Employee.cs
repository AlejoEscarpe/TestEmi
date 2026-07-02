using System.Collections.Generic;
using EmiTest.Domain.Enums;

namespace EmiTest.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; } // [cite: 12]
        public string Name { get; set; } = string.Empty; // [cite: 13]
        public PositionType CurrentPosition { get; set; } // [cite: 14]
        public decimal Salary { get; set; } // [cite: 16]

        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        public ICollection<PositionHistory> PositionHistories { get; set; } = new List<PositionHistory>();

        public ICollection<Project> Projects { get; set; } = new List<Project>();

        // Abstracción para inyectar la estrategia de cálculo de bonos (SOLID - OCP)
        public decimal CalculateYearlyBonus(IBonusStrategy bonusStrategy)
        {
            return bonusStrategy.Calculate(this.Salary); // [cite: 17]
        }
    }

    // Interfaz requerida para el patrón Strategy en el cálculo de bonos
    public interface IBonusStrategy
    {
        decimal Calculate(decimal salary);
    }
}