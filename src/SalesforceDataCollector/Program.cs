using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SalesforceDataCollector.Client;
using SalesforceDataCollector.Data;
using SalesforceDataCollector.Services;
using Quartz;
using System.Net.Http;

namespace SalesforceDataCollector
{
    class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("devsettings.json", true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var dbConnection = hostContext.Configuration.GetConnectionString("default");

                    services.Configure<QuartzOptions>(hostContext.Configuration.GetSection("Quartz"))
                            .AddQuartz(q =>
                            {
                                q.UseMicrosoftDependencyInjectionScopedJobFactory();
                                q.ConfigureJob(hostContext.Configuration);
                            })
                            .AddQuartzHostedService(x => x.WaitForJobsToComplete = true)
                            .AddDbContext<AccountContext>(options => options.UseNpgsql(dbConnection))
                            .RegisterAppServices();
                });

        static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).RunConsoleAsync();
        }
    }

    internal static class Extentions
    {
        public static IServiceCollectionQuartzConfigurator ConfigureJob(this IServiceCollectionQuartzConfigurator configurator, IConfiguration appConfig)
        {
            var interval = appConfig.GetValue<int>("Quartz:repeatInterval");

            var jobKey = new JobKey("Main Job");

            configurator.AddJob<MainJob>(opts => opts.WithIdentity(jobKey));

            configurator.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("Main Trigger")
                .WithSimpleSchedule(s => s.WithIntervalInMinutes(interval).RepeatForever().Build())
            );

            return configurator;
        }

        public static IServiceCollection RegisterAppServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<HttpClient>()
                .AddSingleton<ISalesforceClient, SalesforceClient>()
                .AddScoped<IAccountService, AccountService>()
            ;
        }
    }
}
