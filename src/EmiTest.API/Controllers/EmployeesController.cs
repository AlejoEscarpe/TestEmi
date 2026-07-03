using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmiTest.Application.DTOs;
using EmiTest.Application.Interfaces;

namespace EmiTest.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetById(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound(new { Message = $"Employee with ID {id} not found." });

            return Ok(employee);
        }

        
        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetByDepartment(int departmentId)
        {
            var employees = await _employeeService.GetEmployeesByDepartmentAsync(departmentId);
            return Ok(employees);
        }

        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
        {
            var createdEmployee = await _employeeService.CreateEmployeeAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdEmployee.Id }, createdEmployee);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateEmployeeDto dto)
        {
            var updated = await _employeeService.UpdateEmployeeAsync(id, dto);
            if (!updated) return NotFound(new { Message = $"Employee with ID {id} not found." });

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _employeeService.DeleteEmployeeAsync(id);
            if (!deleted) return NotFound(new { Message = $"Employee with ID {id} not found." });

            return NoContent();
        }
    }
}