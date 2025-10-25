using Employees.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Employees.Web.Database
{
    public class EmployeesDbContext : DbContext
    {
        public EmployeesDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmployeesDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
    