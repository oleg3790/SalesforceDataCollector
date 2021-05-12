using SalesforceDataCollector.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesforceDataCollector.Services
{
    public interface IAccountService
    {
        Task SaveAccountsAsync(IEnumerable<Account> accounts);
    }
}
