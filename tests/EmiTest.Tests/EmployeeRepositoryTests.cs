using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using EmiTest.Infrastructure.Persistence;
using EmiTest.Domain.Entities;
using System.Collections.Generic;

namespace EmiTest.Tests
{
    public class EmployeeRepositoryTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetEmployeesByDepartmentAndProjectsAsync_ReturnsOnlyEmployeesWithProjects()
        {
            // Arrange
            using var context = CreateInMemoryContext("RepoTestDb1");
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var dept1 = new Department { Id = 1, Name = "Dept1" };
            var dept2 = new Department { Id = 2, Name = "Dept2" };
            context.Departments.AddRange(dept1, dept2);

            var project = new Project { Id = 1, Name = "Proj1", Description = "D" };
            context.Projects.Add(project);

            var empWithProject = new Employee { Id = 10, Name = "WithProj", CurrentPosition = Domain.Enums.PositionType.Regular, Salary = 1000m, DepartmentId = 1 };
            empWithProject.PositionHistories.Add(new PositionHistory { Position = "Regular", PositionType = Domain.Enums.PositionType.Regular, StartDate = DateTime.UtcNow });
            empWithProject.Projects.Add(project);

            var empWithoutProject = new Employee { Id = 11, Name = "NoProj", CurrentPosition = Domain.Enums.PositionType.Regular, Salary = 900m, DepartmentId = 1 };
            empWithoutProject.PositionHistories.Add(new PositionHistory { Position = "Regular", PositionType = Domain.Enums.PositionType.Regular, StartDate = DateTime.UtcNow });

            var empOtherDept = new Employee { Id = 12, Name = "OtherDept", CurrentPosition = Domain.Enums.PositionType.Regular, Salary = 1100m, DepartmentId = 2 };
            empOtherDept.PositionHistories.Add(new PositionHistory { Position = "Regular", PositionType = Domain.Enums.PositionType.Regular, StartDate = DateTime.UtcNow });

            context.Employees.AddRange(empWithProject, empWithoutProject, empOtherDept);
            await context.SaveChangesAsync();

            // Act
            var repo = new EmiTest.Infrastructure.Repositories.EmployeeRepository(context);
            var results = (await repo.GetEmployeesByDepartmentAndProjectsAsync(1)).ToList();

            // Assert
            Assert.Single(results);
            Assert.Equal("WithProj", results[0].Name);
        }

        [Fact]
        public async Task GetEmployeeWithHistoryAsync_IncludesPositionHistoriesAndDepartment()
        {
            using var context = CreateInMemoryContext("RepoTestDb2");
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var dept = new Department { Id = 5, Name = "HR" };
            context.Departments.Add(dept);

            var emp = new Employee { Id = 20, Name = "Historied", CurrentPosition = Domain.Enums.PositionType.Manager, Salary = 3000m, DepartmentId = 5 };
            emp.PositionHistories.Add(new PositionHistory { Position = "Junior", PositionType = Domain.Enums.PositionType.Regular, StartDate = DateTime.UtcNow.AddYears(-3), EndDate = DateTime.UtcNow.AddYears(-2) });
            emp.PositionHistories.Add(new PositionHistory { Position = "Manager", PositionType = Domain.Enums.PositionType.Manager, StartDate = DateTime.UtcNow.AddYears(-2) });

            context.Employees.Add(emp);
            await context.SaveChangesAsync();

            var repo = new EmiTest.Infrastructure.Repositories.EmployeeRepository(context);
            var fetched = await repo.GetEmployeeWithHistoryAsync(20);

            Assert.NotNull(fetched);
            Assert.Equal("Historied", fetched!.Name);
            Assert.NotNull(fetched.Department);
            Assert.Equal("HR", fetched.Department!.Name);
            Assert.Equal(2, fetched.PositionHistories.Count);
        }
    }
}
