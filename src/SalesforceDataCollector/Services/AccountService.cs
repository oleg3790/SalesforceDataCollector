using SalesforceDataCollector.Data;
using SalesforceDataCollector.Data.Models;
using SalesforceDataCollector.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesforceDataCollector.Services
{
    public class AccountService : IAccountService
    {
        private readonly AccountContext _accountContext;

        public AccountService(AccountContext accountContext)
        {
            _accountContext = accountContext ?? throw new ArgumentNullException(nameof(accountContext));
        }

        public async Task SaveAccountsAsync(IEnumerable<Account> accounts)
        {
            foreach (var account in accounts)
            {
                await _accountContext.AddAsync(new AccountDataModel
                {
                    Id = Guid.NewGuid(),
                    Created = DateTime.Now,
                    LastModified = DateTime.Now,
                    SalesforceId = account.Id,
                    AccountNumber = account.AccountNumber,
                    Name = account.Name,
                    IsDeleted = account.IsDeleted
                });
            }

            await _accountContext.SaveChangesAsync();
        }
    }
}
