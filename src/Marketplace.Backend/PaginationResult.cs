using System.Collections.Generic;

namespace Marketplace.Backend
{
    public class PaginationResult<T>
    {
        public IList<T> Items { get; set; }
        public int ItemsCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}