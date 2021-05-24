using SalesforceDataCollector.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesforceDataCollector.Services
{
    public interface IAccountService
    {
        Task<int> AddNewAccountsAsync(IEnumerable<Account> accounts);

        Task<int> UpdateModifiedAccountsAsync(IEnumerable<Account> accounts);

        Task<int> RemoveMissingAccountsAsync(IEnumerable<Account> accounts);
    }
}
