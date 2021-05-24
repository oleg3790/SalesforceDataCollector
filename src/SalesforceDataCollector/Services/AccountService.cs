using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SalesforceDataCollector.Data;
using SalesforceDataCollector.Data.Models;
using SalesforceDataCollector.Models;

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

        public async Task AddNewAccountsAsync(IEnumerable<Account> accounts)
        {
            var existingAccounts = GetAllDbAccounts();

            var newAccounts = accounts
                .Where(a => !existingAccounts.Any(ea => ea.Id == a.Id))
                .ToList();

            await _accountContext.AddRangeAsync(newAccounts.Select(na => na.ToDataModel()));
            _logger.LogInformation($"Added {newAccounts.Count} new accounts");

            await _accountContext.SaveChangesAsync();
        }

        public async Task UpdateModifiedAccountsAsync(IEnumerable<Account> accounts)
        {
            var existingAccounts = GetAllDbAccounts();

            var modifiedAccounts = accounts
                .Where(a => existingAccounts.Any(ea => a.Id == ea.Id && a.LastModifiedDate > ea.LastModified))
                .ToList();

            foreach (var modifiedAccount in modifiedAccounts)
            {
                var existingAccount = await _accountContext.Accounts.FindAsync(modifiedAccount.Id);

                existingAccount.LastModified = modifiedAccount.LastModifiedDate;
                existingAccount.Name = modifiedAccount.Name;
                existingAccount.AccountNumber = modifiedAccount.AccountNumber;
                existingAccount.IsDeleted = modifiedAccount.IsDeleted;
            }

            _logger.LogInformation($"Updated {modifiedAccounts.Count} accounts");

            await _accountContext.SaveChangesAsync();
        }

        public async Task RemoveMissingAccountsAsync(IEnumerable<Account> accounts)
        {
            var existingAccounts = GetAllDbAccounts();

            var nonExistingAccounts = existingAccounts
                .Where(ea => !accounts.Any(a => a.Id == ea.Id))
                .ToList();

            _accountContext.RemoveRange(nonExistingAccounts);

            _logger.LogInformation($"Removed {nonExistingAccounts.Count} accounts");

            await _accountContext.SaveChangesAsync();
        }

        private IList<AccountDataModel> GetAllDbAccounts() =>
            _accountContext.Accounts.ToList(); // Materialize the collection of ids so that EF doesn't make a DB call to check each incoming account
    }
}
