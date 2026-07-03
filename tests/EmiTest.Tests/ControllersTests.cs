using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using EmiTest.API.Controllers;
using EmiTest.Application.Interfaces;
using EmiTest.Application.DTOs;
using EmiTest.API.DTOs;
using EmiTest.Domain.Enums;

namespace EmiTest.Tests
{
    public class ControllersTests
    {
        [Fact]
        public async Task EmployeesController_GetAll_ReturnsOkWithEmployees()
        {
            // Arrange
            var svcMock = new Mock<IEmployeeService>();
            svcMock.Setup(s => s.GetAllEmployeesAsync()).ReturnsAsync(new List<EmployeeDto>
            {
                new EmployeeDto { Id = 1, Name = "Alice", CurrentPosition = "Regular", Salary = 1000m }
            });

            var controller = new EmployeesController(svcMock.Object);

            // Act
            var result = await controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<EmployeeDto>>(ok.Value);
            Assert.Single(list);
        }

        [Fact]
        public async Task EmployeesController_GetById_ReturnsNotFound_WhenMissing()
        {
            var svcMock = new Mock<IEmployeeService>();
            svcMock.Setup(s => s.GetEmployeeByIdAsync(99)).ReturnsAsync((EmployeeDto?)null);

            var controller = new EmployeesController(svcMock.Object);

            var result = await controller.GetById(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value.ToString()!.ToLower());
        }

        [Fact]
        public async Task EmployeesController_Create_ReturnsCreated()
        {
            var svcMock = new Mock<IEmployeeService>();
            var created = new EmployeeDto { Id = 5, Name = "New" };
            svcMock.Setup(s => s.CreateEmployeeAsync(It.IsAny<CreateEmployeeDto>())).ReturnsAsync(created);

            var controller = new EmployeesController(svcMock.Object);

            var dto = new CreateEmployeeDto { Name = "New", CurrentPosition = PositionType.Regular, Salary = 1200m, DepartmentId = 1 };

            var result = await controller.Create(dto);

            var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsType<EmployeeDto>(createdAt.Value);
            Assert.Equal(5, value.Id);
        }

        [Fact]
        public void AuthController_Register_ReturnsOkMessage()
        {
            var inMemory = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsATestKeyWithSufficientLengthForHmacSha256_0123456789abcdef" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:DurationInMinutes", "60" }
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();
            var controller = new AuthController(config);

            var res = controller.Register(new RegisterDto { Username = "u", Password = "p", Role = "User" }) as OkObjectResult;
            Assert.NotNull(res);
            Assert.Contains("registered successfully", res!.Value!.ToString()!.ToLower());
        }

        [Fact]
        public void AuthController_Login_ReturnsToken()
        {
            var inMemory = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsATestKeyWithSufficientLengthForHmacSha256_0123456789abcdef" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:DurationInMinutes", "60" }
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();
            var controller = new AuthController(config);

            var res = controller.Login(new LoginDto { Username = "admin", Password = "x" }) as OkObjectResult;
            Assert.NotNull(res);

            // Serialize value to JSON and inspect Token property
            var json = JsonSerializer.Serialize(res!.Value);
            using var doc = JsonDocument.Parse(json);
            Assert.True(doc.RootElement.TryGetProperty("Token", out var tokenProp) || doc.RootElement.TryGetProperty("token", out tokenProp));
            Assert.False(string.IsNullOrEmpty(tokenProp.GetString()));
        }
    }
}
