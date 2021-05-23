using System.Threading.Tasks;

namespace SalesforceDataCollector
{
    public interface ISynchronizer
    {
        Task SyncAccounts();
    }
}
