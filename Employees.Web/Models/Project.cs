namespace Employees.Web.Models
{
    public class Project
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public int EmployeeId { get; set; }
        public int Name { get; set; }
        public DateTime From { get; set; }
        public DateTime? To { get; set; }

        protected Project() { }

        protected Project(int employeeId, int name, DateTime from, DateTime? to)
        {
            EmployeeId = employeeId;
            Name = name;
            From = from;
            To = to;
        }

        public static Project Create(int employeeId, int name, DateTime from, DateTime? to)
        {
            return new Project(employeeId, name, from, to);
        }
    }
}
