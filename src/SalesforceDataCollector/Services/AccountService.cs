using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SalesforceDataCollector.Data;
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
            var newAccounts = accounts
                .Where(a => !_accountContext.Accounts.Any(ea => ea.Id == a.Id ))
                .ToList(); // Materialize collection

            await _accountContext.AddRangeAsync(newAccounts.Select(na => na.ToDataModel()));
            _logger.LogInformation($"Added {newAccounts.Count} new accounts");

            await _accountContext.SaveChangesAsync();
        }

        public async Task UpdateModifiedAccountsAsync(IEnumerable<Account> accounts)
        {
            var modifiedAccounts = accounts
                .Where(a => _accountContext.Accounts.Any(ea => a.Id == ea.Id && a.LastModifiedDate > ea.LastModified))
                .ToList(); // Materialize collection

            foreach (var modifiedAccount in modifiedAccounts)
            {
                var existingAccount = await _accountContext.Accounts.FindAsync(modifiedAccount.Id);

                existingAccount.LastModified = modifiedAccount.LastModifiedDate;
                existingAccount.Name = modifiedAccount.Name;
                existingAccount.AccountNumber = modifiedAccount.AccountNumber;
                existingAccount.IsDeleted = modifiedAccount.IsDeleted;
            }

            _logger.LogInformation($"Updated {modifiedAccounts.Count()} accounts");

            await _accountContext.SaveChangesAsync();
        }

        public async Task RemoveMissingAccountsAsync(IEnumerable<Account> accounts)
        {
            var nonExistingAccounts = _accountContext.Accounts
                .AsEnumerable()
                .Where(ea => !accounts.Any(a => a.Id == ea.Id))
                .ToList(); // Materialize collection

            _accountContext.RemoveRange(nonExistingAccounts);

            _logger.LogInformation($"Removed {nonExistingAccounts.Count()} accounts");

            await _accountContext.SaveChangesAsync();
        }
    }
}
