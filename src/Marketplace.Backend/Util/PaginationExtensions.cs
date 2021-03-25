using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Backend.Base58
{
    public static class PaginationExtensions
    {
        public static async Task<PaginationResult<T>> PaginateAsync<T>(this IQueryable<T> queryable, PaginationParameter parameter)
        {
            var page = parameter.Page ?? 1;
            var pageSize = parameter.PageSize ?? 10;
            var total = await queryable.CountAsync();
            var items = await queryable.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginationResult<T>()
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                ItemsCount = total
            };
        }
    }
}