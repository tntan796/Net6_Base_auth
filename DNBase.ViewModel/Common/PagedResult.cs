using System;
using System.Collections.Generic;
using System.Linq;

namespace DNBase.ViewModel
{
    public class PagedResultModel<T>
    {
        public IReadOnlyList<T> Items { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPage => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public PagedResultModel<TDto> ChangeType<TDto>(Func<T, TDto> cast)
        {
            return new PagedResultModel<TDto>
            {
                Items = Items.Select(cast).ToList(),
                PageIndex = PageIndex,
                PageSize = PageSize,
                TotalCount = TotalCount
            };
        }
    }
}