using System;
using System.Net;

namespace SalesforceDataCollector.Exceptions
{
    public class ApiResponseException : Exception
    {
        public ApiResponseException(HttpStatusCode responseStatusCode, string responseContent, string message = null)
            : base($"{message ?? "Error encountered in API response"}\n\nResponse Code: {responseStatusCode}\nResponse Content: {responseContent}")
        { }
    }
}
