using Employees.Web.Dtos;
using Employees.Web.IQueryableExtensions;
using Employees.Web.Models;
using Employees.Web.Repositories.Interfaces;
using Employees.Web.Services.Queries.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Employees.Web.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectQueryService _projectQueryService;
        private readonly IRepository<Project> _repository;

        public ProjectsController(IProjectQueryService projectQueryService, IRepository<Project> repository)
        {
            _projectQueryService = projectQueryService;
            _repository = repository;
        }

        [HttpGet]
        [Route("")]
        public async Task<PagedResult<ProjectDto>> GetAllProjectsPagedAsync(int pageNumber, int pageSize)
        {
            return await _projectQueryService.Projects().Select(x => x.MapToDto()).GetPagedAsync(pageNumber, pageSize);
        }

        [HttpGet]
        [Route("longest-common")]
        public async Task<List<LongestProjectDto>> GetLongestCommonProjects()
        {
            //initialize the query to start with
            var projects = _projectQueryService.Projects();

            var finalResult = new List<LongestProjectDto>();

            var projectDictByName1 = await projects.ToListAsync();
            var projectDictByName = new Dictionary<int, List<Project>>();
            projectDictByName = projectDictByName1
                .GroupBy(p => p.Name)
                .ToDictionary(
                g => g.Key,
                g => g.ToList()
                );

            projectDictByName = projectDictByName.Where(dict => dict.Value.Count > 1).ToDictionary(x=> x.Key, x => x.Value);
            
            Dictionary<(int, int), TimeSpan > totalTimes = new Dictionary<(int, int), TimeSpan>();

            foreach (var projectByNameList in projectDictByName)
            {
                foreach (var project in projectByNameList.Value)
                {
                    var currentProjectIndex = projectByNameList.Value.IndexOf(project); //save the index of the current project during iteration
                    for (int x = projectByNameList.Value.IndexOf(project); x < projectByNameList.Value.Count - 1; x++) 
                    { 
                        var employee1 = projectByNameList.Value[currentProjectIndex];
                        var employee2 = projectByNameList.Value[x+1];

                        if(employee1.To == null)
                        {
                            employee1.To= DateTime.Now;
                        }

                        if(employee2.To == null)
                        {
                            employee2.To= DateTime.Now;
                        }

                        var possibleOverlapStart = employee1.From > employee2.From ? employee1.From : employee2.From;
                        DateTime possibleOverlapEnd = (DateTime)(employee1.To < employee2.To ? employee1.To : employee2.To);

                        var actualOverlap = possibleOverlapStart < possibleOverlapEnd ?
                                                (possibleOverlapEnd - possibleOverlapStart).Add(TimeSpan.FromDays(1)) :
                                                TimeSpan.Zero;

                        if (actualOverlap != TimeSpan.Zero)
                        {
                            var tryGetTotalTime = totalTimes.TryGetValue((employee1.EmployeeId, employee2.EmployeeId), out var value);

                            if (!tryGetTotalTime)
                            {
                                totalTimes.Add((employee1.EmployeeId, employee2.EmployeeId), actualOverlap);
                            }
                            else
                            {
                                var totalTime = totalTimes[(employee1.EmployeeId, employee2.EmployeeId)];

                                totalTime = totalTime + actualOverlap;

                                totalTimes[(employee1.EmployeeId, employee2.EmployeeId)] = totalTime;
                            }

                        }
                    }
                }
            }

            totalTimes = totalTimes.OrderByDescending(key => key.Value).ToDictionary(x=> x.Key, x=> x.Value);

            bool multiplePairs = true;
            int index = 0;

            while (multiplePairs)
            {
                if (totalTimes.Count == 0) break;

                var result = totalTimes.Skip(index).First();

                var dto = new LongestProjectDto(
                    $"{result.Key}",
                    $"{result.Value.TotalDays} days worked together"
                    );

                finalResult.Add(dto);

                if (totalTimes.Count == index + 1) break;

                if (totalTimes.Skip(index+1).First().Value == result.Value)
                {
                    index++;
                }
                else
                {
                    multiplePairs = false;
                }
            }

            return finalResult;
        }

        [HttpPost]
        [Route("import")]
        public async Task<bool> ImportProjectsFromCSV(IFormFile file)
        {
            if (file == null || Path.GetExtension(file.FileName).ToLowerInvariant() != ".csv") return false;

            var reader = new StreamReader(file.OpenReadStream());

            try
            {
                string? line;
                bool isHeadersLine = true;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (isHeadersLine)
                    {
                        isHeadersLine = false;
                        continue;
                    }

                    var lineParts = line.Split(',');

                    //ToDo: possibly add logging magic, so that we know which lines failed to be imported
                    if(lineParts.Length != 4) continue;

                    var tryParseEmployeeId = int.TryParse(lineParts[0], out var employeeIdParsed);
                    var tryParseName = int.TryParse(lineParts[1], out var nameParsed);
                    var tryParseFrom = DateTime.TryParse(lineParts[2], out var fromParsed);

                    DateTime? toParsed = null;
                    bool tryParseTo = string.IsNullOrWhiteSpace(lineParts[3]) ? true : false;
                    if (!tryParseTo)
                    {
                        tryParseTo = DateTime.TryParse(lineParts[3], out var parsed);

                        if (!tryParseTo) continue;

                        toParsed = parsed;
                    }

                    if (!tryParseFrom || !tryParseName || !tryParseEmployeeId) continue;
             
                    //First map to specific import DTO for validation purposes and then if everything ok, create actual model
                    var projectDto = new ProjectImportDto(
                        employeeIdParsed,
                        nameParsed,
                        fromParsed,
                        toParsed
                    );

                    //Validation Magic here if needed

                    var entity = Project.Create(
                        projectDto.EmployeeId,
                        projectDto.Name,
                        projectDto.From,
                        projectDto.To
                    );

                    _repository.Add(entity);
                }

            }
            finally
            {
                reader.Dispose();
            }

            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
