using Microsoft.EntityFrameworkCore;
using EmiTest.Domain.Entities;

namespace EmiTest.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<PositionHistory> PositionHistories => Set<PositionHistory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuración de Department (Uno a Muchos con Employee)
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            });

            // 2. Configuración de Project (Muchos a Muchos con Employee)
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Description).HasMaxLength(500);

                // Configura la tabla intermedia implícita o explícita para la relación de la Sección 4.1
                entity.HasMany(p => p.Employees)
                      .WithMany(e => e.Projects)
                      .UsingEntity(j => j.ToTable("EmployeeProjects"));
            });

            // 3. Configuración de PositionHistory (Uno a Muchos con Employee)
            modelBuilder.Entity<PositionHistory>(entity =>
            {
                entity.HasKey(ph => ph.Id);
                entity.Property(ph => ph.Position).IsRequired().HasMaxLength(100);
                entity.Property(ph => ph.StartDate).IsRequired();
                entity.Property(ph => ph.EndDate).IsRequired(false);

                entity.HasOne(ph => ph.Employee)
                      .WithMany(e => e.PositionHistories)
                      .HasForeignKey(ph => ph.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 4. Configuración de Employee
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.CurrentPosition).IsRequired();

                entity.HasOne(e => e.Department)
                      .WithMany(d => d.Employees)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}