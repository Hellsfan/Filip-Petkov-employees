namespace Employees.Web.Dtos
{
    public record ProjectDto(
        Guid Id,
        int EmployeeId,
        int Name,
        DateTime From,
        DateTime? To
        );
}
