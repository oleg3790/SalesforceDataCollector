using SalesforceDataCollector.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesforceDataCollector.Services
{
    public interface IAccountService
    {
        Task AddNewAccountsAsync(IEnumerable<Account> accounts);

        Task UpdateModifiedAccountsAsync(IEnumerable<Account> accounts);

        Task RemoveMissingAccountsAsync(IEnumerable<Account> accounts);
    }
}
