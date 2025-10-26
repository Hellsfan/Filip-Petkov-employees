using Employees.Web.Models;
using System.Linq.Expressions;

namespace Employees.Web.Dtos
{
    public static class Mapper
    {
        public static ProjectDto MapToDto(this Project project)
        {
            return new ProjectDto(project.Id, project.EmployeeId, project.Name, project.From, project.To == null ? DateTime.Now : project.To);
        }
    }
}
