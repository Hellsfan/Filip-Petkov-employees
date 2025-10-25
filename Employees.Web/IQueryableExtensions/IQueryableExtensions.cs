using Employees.Web.Dtos;
using Employees.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Employees.Web.IQueryableExtensions
{
    public static class IQueryableExtensions
    {
        public static async Task<PagedResult<T>> GetPagedAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize) where T : class
        {
            var totalCount = query.Count();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                PageSize = pageSize,
                PageNumber = pageNumber,
                TotalCount = totalCount
            };
        }
    }
}
