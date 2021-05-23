using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace SalesforceDataCollector
{
    public class MainJob : IJob
    {
        private readonly ILogger _logger;
        private readonly ISynchronizer _synchronizer;

        public MainJob
        (
            ILogger<MainJob> logger,
            ISynchronizer synchronizer
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _synchronizer = synchronizer ?? throw new ArgumentNullException(nameof(synchronizer));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var stopwatch = new Stopwatch();
                
                _logger.LogInformation("******* Starting Accounts Sync *******");
                stopwatch.Start();

                await _synchronizer.SyncAccounts();

                _logger.LogInformation($"Data synced in {stopwatch.Elapsed}\n");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encountered while running job");
            }
        }
    }
}
