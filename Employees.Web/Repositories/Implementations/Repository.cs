using Employees.Web.Database;
using Employees.Web.Repositories.Interfaces;

namespace Employees.Web.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly EmployeesDbContext _context;

        public Repository(EmployeesDbContext context)
        {
            _context = context;
        }

        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public async Task SaveChangesAsync()
        {
           await _context.SaveChangesAsync();
        }
    }
}
