namespace Employees.Web.Dtos
{
    public record ProjectImportDto(
        int EmployeeId,
        int Name,
        DateTime From,
        DateTime? To
    );
}
