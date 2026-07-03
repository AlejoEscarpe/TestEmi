using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EmiTest.Domain.Entities;
using EmiTest.Domain.Interfaces;
using EmiTest.Infrastructure.Persistence;

namespace EmiTest.Infrastructure.Repositories
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Employee?> GetEmployeeWithHistoryAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.PositionHistories)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAndProjectsAsync(int departmentId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Projects)
                .Where(e => e.DepartmentId == departmentId && e.Projects.Any()) 
                .ToListAsync();
        }
    }
}