using System.Collections.Generic;

namespace Application.Shared.Dtos
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}