using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmiTest.Application.DTOs;
using EmiTest.Application.Interfaces;
using EmiTest.Application.Strategies;
using EmiTest.Domain.Entities;
using EmiTest.Domain.Interfaces;

namespace EmiTest.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IBonusStrategyFactory _bonusStrategyFactory;

        public EmployeeService(IEmployeeRepository employeeRepository, IBonusStrategyFactory bonusStrategyFactory)
        {
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _bonusStrategyFactory = bonusStrategyFactory ?? throw new ArgumentNullException(nameof(bonusStrategyFactory));
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();
            return employees.Select(MapToDto);
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetEmployeeWithHistoryAsync(id);
            if (employee == null) return null;

            return MapToDto(employee);
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
        {
            var employee = new Employee
            {
                Name = dto.Name,
                CurrentPosition = dto.CurrentPosition,
                Salary = dto.Salary,
                DepartmentId = dto.DepartmentId
            };

            // Añadimos el registro inicial al historial de posiciones
            employee.PositionHistories.Add(new PositionHistory
            {
                Position = dto.CurrentPosition.ToString(),
                PositionType = dto.CurrentPosition,
                StartDate = DateTime.UtcNow
            });

            await _employeeRepository.AddAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return MapToDto(employee);
        }

        public async Task<bool> UpdateEmployeeAsync(int id, CreateEmployeeDto dto)
        {
            var employee = await _employeeRepository.GetEmployeeWithHistoryAsync(id);
            if (employee == null) return false;

            // Si la posición cambió, cerramos la anterior en el historial y abrimos una nueva
            if (employee.CurrentPosition != dto.CurrentPosition)
            {
                var currentHistory = employee.PositionHistories.FirstOrDefault(ph => ph.EndDate == null);
                if (currentHistory != null)
                {
                    currentHistory.EndDate = DateTime.UtcNow;
                }

                employee.PositionHistories.Add(new PositionHistory
                {
                    Position = dto.CurrentPosition.ToString(),
                    PositionType = dto.CurrentPosition,
                    StartDate = DateTime.UtcNow
                });
            }

            employee.Name = dto.Name;
            employee.CurrentPosition = dto.CurrentPosition;
            employee.Salary = dto.Salary;
            employee.DepartmentId = dto.DepartmentId;

            _employeeRepository.Update(employee);
            return await _employeeRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return false;

            _employeeRepository.Delete(employee);
            return await _employeeRepository.SaveChangesAsync();
        }

        // Resolución de la query optimizada usando LINQ 
        public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(int departmentId)
        {
            var employees = await _employeeRepository.GetEmployeesByDepartmentAndProjectsAsync(departmentId);
            return employees.Select(MapToDto);
        }

        // Método privado helper para mapear Entidades a DTOs y calcular el Bono con el patrón Strategy
        private EmployeeDto MapToDto(Employee employee)
        {
            // Resolvemos la estrategia usando nuestra Factory de forma dinámica 
            var strategy = _bonusStrategyFactory.GetStrategy(employee.CurrentPosition);

            return new EmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                CurrentPosition = employee.CurrentPosition.ToString(),
                Salary = employee.Salary,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department?.Name ?? "N/A",
                YearlyBonus = employee.CalculateYearlyBonus(strategy), // Ejecuta el patrón Strategy
                PositionHistory = employee.PositionHistories.Select(ph => new PositionHistoryDto
                {
                    Position = ph.Position,
                    StartDate = ph.StartDate,
                    EndDate = ph.EndDate
                }).ToList()
            };
        }
    }
}