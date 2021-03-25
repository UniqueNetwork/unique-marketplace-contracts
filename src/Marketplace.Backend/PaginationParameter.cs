using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Backend
{
    public class PaginationParameter
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}