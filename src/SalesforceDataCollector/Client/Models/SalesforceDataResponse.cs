using System.Collections.Generic;

namespace SalesforceDataCollector.Client.Models
{
    public class SalesforceDataResponse<T>
    {
        public int TotalSize { get; set; }
        public bool Done { get; set; }
        public IEnumerable<T> Records { get; set; }
    }
}
