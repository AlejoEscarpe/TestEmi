using System.Collections.Generic;
using System.Threading.Tasks;
using EmiTest.Domain.Entities;

namespace EmiTest.Domain.Interfaces
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<IEnumerable<Employee>> GetEmployeesByDepartmentAndProjectsAsync(int departmentId);

        Task<Employee?> GetEmployeeWithHistoryAsync(int id);
    }
}