using System.Collections.Generic;

namespace DNBase.ViewModel
{
    public class ElasticSearchResult<T>
    {
        public int Page { get; set; }
        public long Total { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public List<T> Data { get; set; }
    }
}
