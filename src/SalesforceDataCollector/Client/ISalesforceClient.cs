using SalesforceDataCollector.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesforceDataCollector.Client
{
    public interface ISalesforceClient
    {
        Task<IEnumerable<Account>> GetAccountsAsync();
    }
}
