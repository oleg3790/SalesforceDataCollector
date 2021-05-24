using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SalesforceDataCollector.Client;
using SalesforceDataCollector.Models;
using SalesforceDataCollector.Services;

namespace SalesforceDataCollector
{
    public class Synchronizer : ISynchronizer
    {
        private const string AllAccountsQuery = "select id, accountNumber, name, isDeleted, lastModifiedDate from account";

        private readonly ILogger<Synchronizer> _logger;
        private readonly ISalesforceClient _salesforceClient;
        private readonly IAccountService _accountService;

        public Synchronizer
        (
            ILogger<Synchronizer> logger,
            ISalesforceClient salesforceClient,
            IAccountService accountService
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _salesforceClient = salesforceClient ?? throw new ArgumentNullException(nameof(salesforceClient));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public async Task SyncAccounts()
        {
            var isBusy = true;
            string nextRecordSetUrl = null;
            var totalRecordsCollected = 0;
            var accountsToRemove = new List<Account>();

            while (isBusy)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var response = string.IsNullOrWhiteSpace(nextRecordSetUrl) 
                    ? await _salesforceClient.QueryData<Account>(AllAccountsQuery)
                    : await _salesforceClient.GetData<Account>(nextRecordSetUrl);

                totalRecordsCollected += response.Records.Count();
                accountsToRemove.AddRange(response.Records);

                _logger.LogInformation($"{totalRecordsCollected} of {response.TotalSize} accounts collected in {stopwatch.Elapsed}");

                nextRecordSetUrl = response.NextRecordsUrl;
                isBusy = !response.Done && !string.IsNullOrWhiteSpace(response.NextRecordsUrl);

                await _accountService.AddNewAccountsAsync(response.Records);
                await _accountService.UpdateModifiedAccountsAsync(response.Records);
            }

            await _accountService.RemoveMissingAccountsAsync(accountsToRemove);
        }
    }
}
