using SalesforceDataCollector.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesforceDataCollector.Client
{
    public class SalesforceClient : ISalesforceClient
    {
        public Task<IEnumerable<Account>> GetAccountsAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
