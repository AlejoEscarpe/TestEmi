using System.Collections.Generic;
using System.Threading.Tasks;
using EmiTest.Domain.Entities;

namespace EmiTest.Domain.Interfaces
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        // Query de la Sección 4.3: Obtener empleados por departamento y que tengan proyectos asignados
        Task<IEnumerable<Employee>> GetEmployeesByDepartmentAndProjectsAsync(int departmentId);

        // Para obtener un empleado con su historial de posiciones incluido (Sección 1 y 2)
        Task<Employee?> GetEmployeeWithHistoryAsync(int id);
    }
}