using System;

namespace SalesforceDataCollector.Models
{
    public class Account
    {
        public string Id { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
    }
}
