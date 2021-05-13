using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SalesforceDataCollector.Client;
using SalesforceDataCollector.Services;
using Quartz;
using System.Linq;

namespace SalesforceDataCollector
{
    public class MainJob : IJob
    {
        private readonly ILogger _logger;
        private readonly ISalesforceClient _salesforceClient;
        private readonly AccountService _accountService;

        public MainJob
        (
            ILogger<MainJob> logger,
            ISalesforceClient salesforceClient,
            AccountService accountService
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _salesforceClient = salesforceClient ?? throw new ArgumentNullException(nameof(salesforceClient));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var accounts = await _salesforceClient.GetAllAccountsAsync();

            _logger.LogInformation($"Obtained {accounts.Count()} accounts");

            await _accountService.SaveAccountsAsync(accounts);
        }
    }
}
