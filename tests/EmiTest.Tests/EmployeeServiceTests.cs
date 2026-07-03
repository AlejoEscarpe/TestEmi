using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using EmiTest.Application.Services;
using EmiTest.Domain.Interfaces;
using EmiTest.Application.Strategies;
using EmiTest.Domain.Entities;
using EmiTest.Application.DTOs;
using EmiTest.Domain.Enums;

namespace EmiTest.Tests
{
    public class EmployeeServiceTests
    {
        [Fact]
        public async Task CreateEmployeeAsync_AddsEmployeeAndInitialPositionHistory()
        {
            // Arrange
            var repoMock = new Mock<IEmployeeRepository>();
            var factoryMock = new Mock<IBonusStrategyFactory>();

            factoryMock.Setup(f => f.GetStrategy(It.IsAny<PositionType>())).Returns(new RegularEmployeeBonusStrategy());

            repoMock.Setup(r => r.AddAsync(It.IsAny<Employee>()))
                .Callback<Employee>(e => e.Id = 42)
                .Returns(Task.CompletedTask);

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var service = new EmployeeService(repoMock.Object, factoryMock.Object);

            var dto = new CreateEmployeeDto
            {
                Name = "Test User",
                CurrentPosition = PositionType.Regular,
                Salary = 1500m,
                DepartmentId = 1
            };

            // Act
            var result = await service.CreateEmployeeAsync(dto);

            // Assert
            Assert.Equal(42, result.Id);
            Assert.Equal(dto.Name, result.Name);
            Assert.Single(result.PositionHistory);
            Assert.Equal(dto.Salary * 0.10m, result.YearlyBonus);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_ChangesPosition_ClosesPreviousHistory_And_AddsNew()
        {
            // Arrange
            var repoMock = new Mock<IEmployeeRepository>();
            var factoryMock = new Mock<IBonusStrategyFactory>();
            factoryMock.Setup(f => f.GetStrategy(It.IsAny<PositionType>())).Returns(new ManagerBonusStrategy());

            var existingEmployee = new Employee
            {
                Id = 1,
                Name = "Existing",
                CurrentPosition = PositionType.Regular,
                Salary = 1000m,
                DepartmentId = 2,
                PositionHistories = new List<PositionHistory>
                {
                    new PositionHistory
                    {
                        Id = 10,
                        Position = PositionType.Regular.ToString(),
                        PositionType = PositionType.Regular,
                        StartDate = DateTime.UtcNow.AddYears(-1),
                        EndDate = null
                    }
                }
            };

            repoMock.Setup(r => r.GetEmployeeWithHistoryAsync(1)).ReturnsAsync(existingEmployee);
            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var service = new EmployeeService(repoMock.Object, factoryMock.Object);

            var updateDto = new CreateEmployeeDto
            {
                Name = "Existing Updated",
                CurrentPosition = PositionType.Manager,
                Salary = 2000m,
                DepartmentId = 2
            };

            // Act
            var updated = await service.UpdateEmployeeAsync(1, updateDto);

            // Assert
            Assert.True(updated);
            Assert.Equal("Existing Updated", existingEmployee.Name);
            Assert.Equal(PositionType.Manager, existingEmployee.CurrentPosition);
            // previous history must have EndDate set
            Assert.Contains(existingEmployee.PositionHistories, ph => ph.EndDate != null);
            // new history added
            Assert.Equal(2, existingEmployee.PositionHistories.Count);
        }
    }
}
