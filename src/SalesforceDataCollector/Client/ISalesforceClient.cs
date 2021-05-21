using SalesforceDataCollector.Client.Models;
using System.Threading.Tasks;

namespace SalesforceDataCollector.Client
{
    public interface ISalesforceClient
    {
        /// <summary>
        /// Get data by running a SOQL query
        /// </summary>
        /// <typeparam name="T">Data record type expected in the response</typeparam>
        /// <param name="query">SOQL query</param>
        Task<SalesforceDataResponse<T>> QueryData<T>(string query);

        /// <summary>
        /// Get data using a relative URI (ex /services/data/vXX.X/limits)
        /// </summary>
        /// <typeparam name="T">Data record type expected in the response</typeparam>
        /// <param name="relativeUri">ex. /services/data/vXX.X/limits</param>
        Task<SalesforceDataResponse<T>> GetData<T>(string relativeUri);
    }
}
