using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SalesforceDataCollector.Client;
using SalesforceDataCollector.Services;
using Quartz;
using System.Diagnostics;

namespace SalesforceDataCollector
{
    public class MainJob : IJob
    {
        private readonly ILogger _logger;
        private readonly ISalesforceClient _salesforceClient;
        private readonly IAccountService _accountService;

        public MainJob
        (
            ILogger<MainJob> logger,
            ISalesforceClient salesforceClient,
            IAccountService accountService
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _salesforceClient = salesforceClient ?? throw new ArgumentNullException(nameof(salesforceClient));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var stopwatch = new Stopwatch();
                
                _logger.LogInformation("Starting API Data Collect");
                stopwatch.Start();

                var accounts = await _salesforceClient.GetAllAccountsAsync();
                _logger.LogInformation($"API data collected in {stopwatch.ElapsedTicks / Stopwatch.Frequency} seconds");

                _logger.LogInformation("Starting Data Sync");
                stopwatch.Restart();

                await _accountService.SaveAccountsAsync(accounts);
                _logger.LogInformation($"Data synced in {stopwatch.ElapsedTicks / Stopwatch.Frequency} seconds");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encountered while running job");
            }
        }
    }
}
