using Employees.Web.Controllers;
using Employees.Web.Database;
using Employees.Web.Models;
using Employees.Web.Repositories.Implementations;
using Employees.Web.Repositories.Interfaces;
using Employees.Web.Services.Queries.Implementations;
using Employees.Web.Services.Queries.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace Employees.Tests.ControllerTests
{
    public class ProjectsControllerTests
    {
        [Fact]
        public async Task ProjectsController_GetAllProjectsPagedAsync_HappyPath()
        {
            var projects = new List<Project>
            {
                Project.Create(1,15,DateTime.Now.AddDays(-20),DateTime.Now.AddDays(-18)),
                Project.Create(2,15,DateTime.Now.AddMonths(-2),DateTime.Now),
                Project.Create(3,18,DateTime.Now.AddYears(-3),null),
                Project.Create(4,19,DateTime.Now.AddDays(-33),null),
                Project.Create(5,19,DateTime.Now.AddMonths(-5),DateTime.Now)
            };

            var dbContext = new EmployeesDbContext(
            new DbContextOptionsBuilder<EmployeesDbContext>()
                .UseInMemoryDatabase("GetAllProjectsPagedAsync_HappyPath").Options);

            Repository<Project> repository = new Repository<Project>(dbContext);
            ProjectQueryService service = new ProjectQueryService(dbContext);

            var controller = new ProjectsController(service, repository);

            await dbContext.Projects.AddRangeAsync(projects);
            await dbContext.SaveChangesAsync();

            var result = await controller.GetAllProjectsPagedAsync(1, 10);

            Assert.NotNull(result);
            Assert.True(result.Items.Count() == 5);
            Assert.True(result.TotalPages == 1);
            Assert.True(result.PageNumber == 1);
        }

        [Fact]
        public async Task ProjectsController_Import_HappyPath()
        {
            var dbContext = new EmployeesDbContext(
            new DbContextOptionsBuilder<EmployeesDbContext>()
                .UseInMemoryDatabase("Import_HappyPath").Options);

            Repository<Project> repository = new Repository<Project>(dbContext);
            ProjectQueryService service = new ProjectQueryService(dbContext);

            var controller = new ProjectsController(service, repository);

            var filePath = Path.Combine(AppContext.BaseDirectory, "TestFiles", "TestImportFile.csv");
            var fileBytes = File.ReadAllBytes(filePath);
            var stream = new MemoryStream(fileBytes);
            var formFile = new FormFile(stream, 0, stream.Length, "file", "TestImportFile.csv");

            var result = await controller.ImportProjectsFromCSV(formFile);

            Assert.True(result);

            var projects = dbContext.Projects.ToList();
            Assert.Equal(5, projects.Count);

            var project = projects.First();
            Assert.NotNull(project);
        }

        [Fact]
        public async Task ProjectsController_Import_MissingValues()
        {
            var dbContext = new EmployeesDbContext(
            new DbContextOptionsBuilder<EmployeesDbContext>()
                .UseInMemoryDatabase("Import_MissingValues").Options);

            Repository<Project> repository = new Repository<Project>(dbContext);
            ProjectQueryService service = new ProjectQueryService(dbContext);

            var controller = new ProjectsController(service, repository);

            var filePath = Path.Combine(AppContext.BaseDirectory, "TestFiles", "TestImportFile_MissingValues.csv");
            var fileBytes = File.ReadAllBytes(filePath);
            var stream = new MemoryStream(fileBytes);
            var formFile = new FormFile(stream, 0, stream.Length, "file", "TestImportFile_MissingValues.csv");

            var result = await controller.ImportProjectsFromCSV(formFile);

            Assert.True(result);

            var projects = dbContext.Projects.ToList();
            Assert.Single(projects);
        }

        [Fact]
        public async Task ProjectsController_GetLongestCommonProjects_HappyPath()
        {
            var dbContext = new EmployeesDbContext(
            new DbContextOptionsBuilder<EmployeesDbContext>()
                .UseInMemoryDatabase("GetLongestCommonProjects_HappyPath").Options);

            Repository<Project> repository = new Repository<Project>(dbContext);
            ProjectQueryService service = new ProjectQueryService(dbContext);

            var controller = new ProjectsController(service, repository);

            //1 pair with a total of 4 days together
            var filePath = Path.Combine(AppContext.BaseDirectory, "TestFiles", "TestCommon_HappyPath.csv");
            var fileBytes = File.ReadAllBytes(filePath);
            var stream = new MemoryStream(fileBytes);
            var formFile = new FormFile(stream, 0, stream.Length, "file", "TestImportFile.csv");

            var import = await controller.ImportProjectsFromCSV(formFile);

            Assert.True(import);

            var longestCommonList = await controller.GetLongestCommonProjects();

            Assert.NotNull(longestCommonList);
            Assert.Contains("5 days worked together", longestCommonList.First().TimeSum);
        }

        [Fact]
        public async Task ProjectsController_GetLongestCommonProjects_HappyPath_WithNullValues()
        {
            var dbContext = new EmployeesDbContext(
            new DbContextOptionsBuilder<EmployeesDbContext>()
                .UseInMemoryDatabase("GetLongestCommonProjects_HappyPath_WithNullValues").Options);

            Repository<Project> repository = new Repository<Project>(dbContext);
            ProjectQueryService service = new ProjectQueryService(dbContext);

            var controller = new ProjectsController(service, repository);

            var filePath = Path.Combine(AppContext.BaseDirectory, "TestFiles", "TestCommon_HappyPath_WithNullValues.csv");
            var fileBytes = File.ReadAllBytes(filePath);
            var stream = new MemoryStream(fileBytes);
            var formFile = new FormFile(stream, 0, stream.Length, "file", "TestImportFile.csv");

            var import = await controller.ImportProjectsFromCSV(formFile);

            Assert.True(import);

            var longestCommonList = await controller.GetLongestCommonProjects();

            var spanMath = DateTime.Parse("09-05-2018") - DateTime.Parse("01-02-2015");
            spanMath = spanMath.Add(TimeSpan.FromDays(1));

            Assert.NotNull(longestCommonList);
            Assert.Contains($"{spanMath.TotalDays} days worked together", longestCommonList.First().TimeSum);
        }

        [Fact]
        public async Task ProjectsController_GetLongestCommonProjects_HappyPath_MultiplePeople()
        {
            var dbContext = new EmployeesDbContext(
            new DbContextOptionsBuilder<EmployeesDbContext>()
                .UseInMemoryDatabase("GetLongestCommonProjects_HappyPath_WithMultiplePeople").Options);

            Repository<Project> repository = new Repository<Project>(dbContext);
            ProjectQueryService service = new ProjectQueryService(dbContext);

            var controller = new ProjectsController(service, repository);

            var filePath = Path.Combine(AppContext.BaseDirectory, "TestFiles", "TestCommon_MultiplePeople.csv");
            var fileBytes = File.ReadAllBytes(filePath);
            var stream = new MemoryStream(fileBytes);
            var formFile = new FormFile(stream, 0, stream.Length, "file", "TestImportFile.csv");

            var import = await controller.ImportProjectsFromCSV(formFile);

            Assert.True(import);

            var longestCommonList = await controller.GetLongestCommonProjects();

            Assert.NotNull(longestCommonList);
            Assert.True(longestCommonList.Count>1);
        }

        [Fact]
        public async Task ProjectsController_GetLongestCommonProjects_OneBig_vs_MultipleSmaller()
        {
            var dbContext = new EmployeesDbContext(
            new DbContextOptionsBuilder<EmployeesDbContext>()
                .UseInMemoryDatabase("GetLongestCommonProjects_OneBig_vs_MultipleSmaller").Options);

            Repository<Project> repository = new Repository<Project>(dbContext);
            ProjectQueryService service = new ProjectQueryService(dbContext);

            var controller = new ProjectsController(service, repository);

            var filePath = Path.Combine(AppContext.BaseDirectory, "TestFiles", "TestCommon_OneBig_vs_MultipleSmaller.csv");
            var fileBytes = File.ReadAllBytes(filePath);
            var stream = new MemoryStream(fileBytes);
            var formFile = new FormFile(stream, 0, stream.Length, "file", "TestImportFile.csv");

            var import = await controller.ImportProjectsFromCSV(formFile);

            Assert.True(import);

            var longestCommonList = await controller.GetLongestCommonProjects();

            Assert.NotNull(longestCommonList);
            Assert.Contains($"368 days worked together", longestCommonList.First().TimeSum);
            Assert.Contains($"3, 4", longestCommonList.First().EmployeeIds);
        }
    }

}
 