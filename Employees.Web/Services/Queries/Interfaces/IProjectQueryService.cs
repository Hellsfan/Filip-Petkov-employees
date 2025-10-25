using Employees.Web.Dtos;
using Employees.Web.Models;

namespace Employees.Web.Services.Queries.Interfaces
{
    public interface IProjectQueryService
    {
        IQueryable<Project> Projects();
    }
}
