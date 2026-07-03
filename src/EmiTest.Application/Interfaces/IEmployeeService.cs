using System.Collections.Generic;
using System.Threading.Tasks;
using EmiTest.Application.DTOs;

namespace EmiTest.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto);
        Task<bool> UpdateEmployeeAsync(int id, CreateEmployeeDto dto);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(int departmentId);
    }
}