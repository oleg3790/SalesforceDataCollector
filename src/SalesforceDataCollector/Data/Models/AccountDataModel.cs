using System;

namespace SalesforceDataCollector.Data.Models
{
    public class AccountDataModel
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public string SalesforceId { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
    }
}
