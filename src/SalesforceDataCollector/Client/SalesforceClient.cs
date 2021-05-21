using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using SalesforceDataCollector.Client.Models;
using SalesforceDataCollector.Exceptions;
using SalesforceDataCollector.Models;

namespace SalesforceDataCollector.Client
{
    public class SalesforceClient : ISalesforceClient
    {
        private const string SalesforceLoginBaseUrl = "https://login.salesforce.com";
        private const string AllAccountsQuery = "select id, accountNumber, name, isDeleted, lastModifiedDate from account";

        private readonly string _apiVersion;

        private readonly ILogger<SalesforceClient> _logger;
        private readonly HttpClient _client;
        private readonly IConfiguration _config;

        public SalesforceClient
        (
            ILogger<SalesforceClient> logger,
            HttpClient client,
            IConfiguration config
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _apiVersion = config.GetValue<string>("Salesforce:ApiVersion") ?? "51.0";
        }

        public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            var sfAuth = await Authenticate();
            _logger.LogDebug($"Authentication Response: {JsonConvert.SerializeObject(sfAuth)}");

            var queryRequestUri = BuildQueryRequestUri(sfAuth.InstanceUrl, AllAccountsQuery);
            var requestMessage = BuildRequestMessage(HttpMethod.Get, queryRequestUri, sfAuth);
            var response = await _client.SendAsync(requestMessage);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiResponseException(response.StatusCode, responseContent);
            }

            var data = JsonConvert.DeserializeObject<SalesforceDataResponse<Account>>(responseContent);
            _logger.LogInformation($"{data.Records.Count()} of {data.TotalSize} Accounts Fetched");

            return data.Records;
        }

        private HttpRequestMessage BuildRequestMessage(HttpMethod method, string requestUri, SalesforceAuthResponse authContent)
        {
            var message = new HttpRequestMessage
            {
                RequestUri = new Uri(requestUri),
                Method = method
            };
            message.Headers.Authorization = new AuthenticationHeaderValue(authContent.TokenType, authContent.AccessToken);

            return message;
        }

        private string BuildQueryRequestUri(string instanceUrl, string query) =>
            $"{instanceUrl}/services/data/v{_apiVersion}/query?q={WebUtility.HtmlEncode(query)}";

        /// <summary>
        /// Authenticates this client to the Salesforce API
        /// </summary>
        private async Task<SalesforceAuthResponse> Authenticate()
        {
            var authToken = GenerateAuthJWTToken();
            _logger.LogTrace($"Auth Token: {authToken}");

            var response = await _client.PostAsync
            (
                $"{SalesforceLoginBaseUrl}/services/oauth2/token",
                new FormUrlEncodedContent
                (
                    new[] {
                        new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                        new KeyValuePair<string, string>("assertion", authToken)
                    }
                )
            );

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiResponseException(response.StatusCode, responseContent, "Could not authenticate to Salesforce");
            }

            _logger.LogInformation($"Authenticated to the Salesforce API successfully!");

            return JsonConvert.DeserializeObject<SalesforceAuthResponse>(responseContent);
        }

        private string GenerateAuthJWTToken()
        {
            var base64PrivateKey = _config.GetValue<string>("Salesforce:CertPrivateKey");

            if (string.IsNullOrWhiteSpace(base64PrivateKey))
            {
                throw new Exception("Salesforce certificate private key not found");
            }

            var privateKey = Encoding.ASCII.GetString(Convert.FromBase64String(base64PrivateKey));

            var rsaParams = GetRsaParameters(privateKey);
            var encoder = GetRS256JWTEncoder(rsaParams);

            var payload = new Dictionary<string, object>
            {
                { "iss", _config.GetValue<string>("Salesforce:ClientId") ?? throw new Exception("No Salesforce client Id found in config") },
                { "aud", SalesforceLoginBaseUrl },
                { "sub", _config.GetValue<string>("Salesforce:User") ?? throw new Exception("No Salesforce user found in config") },
                { "exp", DateTimeOffset.UtcNow.AddMinutes(3).ToUnixTimeSeconds() }
            };

            var token = encoder.Encode(null, payload, new byte[0]);

            return token;
        }

        private IJwtEncoder GetRS256JWTEncoder(RSAParameters rsaParams)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(rsaParams);

            var algorithm = new RS256Algorithm(csp, csp);
            var serializer = new JsonNetSerializer();
            var urlEncoder = new JwtBase64UrlEncoder();
            var encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            return encoder;
        }

        private RSAParameters GetRsaParameters(string rsaPrivateKey)
        {
            var byteArray = Encoding.ASCII.GetBytes(rsaPrivateKey);
            using (var ms = new MemoryStream(byteArray))
            {
                using (var sr = new StreamReader(ms))
                {
                    var pemReader = new PemReader(sr);
                    var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
                    return DotNetUtilities.ToRSAParameters(keyPair.Private as RsaPrivateCrtKeyParameters);
                }
            }
        }
    }
}
