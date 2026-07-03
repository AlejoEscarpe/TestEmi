using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EmiTest.Infrastructure.Persistence;
using EmiTest.Domain.Entities;
using System.Collections.Generic;

namespace EmiTest.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<EmiTest.API.Program>>
    {
        private readonly WebApplicationFactory<EmiTest.API.Program> _factory;

        public IntegrationTests(WebApplicationFactory<EmiTest.API.Program> factory)
        {
            // Customize factory to use InMemory DB
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing ApplicationDbContext registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    // Add InMemory DB
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestEmiDb");
                    });

                    // Build service provider to seed data
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                        db.Database.EnsureDeleted();
                        db.Database.EnsureCreated();

                        // Seed minimal data
                        var dept = new Department { Id = 1, Name = "Dev" };
                        db.Departments.Add(dept);

                        var emp = new Employee
                        {
                            Id = 1,
                            Name = "Seeded",
                            CurrentPosition = Domain.Enums.PositionType.Regular,
                            Salary = 1000m,
                            DepartmentId = 1
                        };
                        emp.PositionHistories.Add(new PositionHistory { Position = "Regular", PositionType = Domain.Enums.PositionType.Regular, StartDate = System.DateTime.UtcNow });
                        db.Employees.Add(emp);

                        db.SaveChanges();
                    }
                });
            });
        }

        private async Task<string> GetJwtTokenAsync(HttpClient client, string username)
        {
            var loginObj = new { username = username, password = "x" };
            var content = new StringContent(JsonSerializer.Serialize(loginObj), Encoding.UTF8, "application/json");
            var res = await client.PostAsync("/api/auth/login", content);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("token", out var tokenProp) || doc.RootElement.TryGetProperty("Token", out tokenProp))
            {
                return tokenProp.GetString() ?? string.Empty;
            }

            throw new System.Exception("Token not found in login response: " + json);
        }

        [Fact]
        public async Task GetEmployees_WithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var res = await client.GetAsync("/api/employees");
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [Fact]
        public async Task GetEmployees_AsAdmin_ReturnsOk()
        {
            var client = _factory.CreateClient();
            var token = await GetJwtTokenAsync(client, "admin");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await client.GetAsync("/api/employees");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            var json = await res.Content.ReadAsStringAsync();
            Assert.Contains("Seeded", json);
        }

        [Fact]
        public async Task PostEmployee_AsUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();
            var token = await GetJwtTokenAsync(client, "regularuser");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var newEmp = new { Name = "New", CurrentPosition = Domain.Enums.PositionType.Regular, Salary = 1200m, DepartmentId = 1 };
            var content = new StringContent(JsonSerializer.Serialize(newEmp), Encoding.UTF8, "application/json");
            var res = await client.PostAsync("/api/employees", content);

            Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        }
    }
}
