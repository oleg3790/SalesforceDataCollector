using Microsoft.Extensions.Logging;
using SalesforceDataCollector.Data;
using SalesforceDataCollector.Data.Models;
using SalesforceDataCollector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesforceDataCollector.Services
{
    public class AccountService : IAccountService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly AccountContext _accountContext;

        public AccountService
        (
            ILogger<AccountService> logger,
            AccountContext accountContext
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountContext = accountContext ?? throw new ArgumentNullException(nameof(accountContext));
        }

        public async Task SaveAccountsAsync(IEnumerable<Account> accounts)
        {
            // Add new accounts
            var newAccounts = accounts
                .Where(a => !_accountContext.Accounts.Any(ea => ea.Id == a.Id ));

            _logger.LogInformation($"Adding {newAccounts.Count()} new accounts");
            await _accountContext.AddRangeAsync(newAccounts.Select(na => na.ToDataModel()));

            // Delete missing accounts
            var nonExistingAccounts = _accountContext.Accounts
                .AsEnumerable()
                .Where(ea => !accounts.Any(a => a.Id == ea.Id));

            _logger.LogInformation($"Removing {nonExistingAccounts.Count()} accounts");
            _accountContext.RemoveRange(nonExistingAccounts);

            // Update changed accounts
            var changedAccounts = accounts
                .Where(a => _accountContext.Accounts.Any(ea => a.Id == ea.Id && a.LastModifiedDate > ea.LastModified));

            _logger.LogInformation($"Updating {changedAccounts.Count()} accounts");

            foreach (var changedAccount in changedAccounts)
            {
                var existingAccunt = await _accountContext.Accounts.FindAsync(changedAccount.Id);

                existingAccunt.LastModified = changedAccount.LastModifiedDate;
                existingAccunt.Name = changedAccount.Name;
                existingAccunt.AccountNumber = changedAccount.AccountNumber;
                existingAccunt.IsDeleted = changedAccount.IsDeleted;
            }

            await _accountContext.SaveChangesAsync();
        }
    }
}
