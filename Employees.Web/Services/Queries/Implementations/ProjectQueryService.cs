using Employees.Web.Database;
using Employees.Web.Dtos;
using Employees.Web.Models;
using Employees.Web.Services.Queries.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Employees.Web.Services.Queries.Implementations
{
    public class ProjectQueryService : IProjectQueryService
    {
        private readonly EmployeesDbContext _context; 
        public ProjectQueryService(EmployeesDbContext context) 
        {
            _context = context;
        }

        public IQueryable<Project> Projects()
        {
            return _context.Projects.AsNoTracking().AsQueryable();
        }
    }
}
