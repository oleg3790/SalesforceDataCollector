using Microsoft.Extensions.Logging;
using SalesforceDataCollector.Client;
using SalesforceDataCollector.Models;
using SalesforceDataCollector.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SalesforceDataCollector
{
    public class Synchronizer
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

        public async Task SyncAccounts(bool isDone)
        {
            var stopwatch = new Stopwatch();

            _logger.LogInformation("******* Starting API Data Collect *******");
            stopwatch.Start();

            var response = await _salesforceClient.QueryData<Account>(AllAccountsQuery);
            _logger.LogInformation($"API data collected in {stopwatch.Elapsed}");

            _logger.LogInformation("Starting Data Sync");
            stopwatch.Restart();

            await _accountService.SyncAccountsAsync(response.Records);
            _logger.LogInformation($"Data synced in {stopwatch.Elapsed}\n");

            while(!response.Done && !string.IsNullOrWhiteSpace(response.NextRecordsUrl))
            {

            }
        }
    }
}
