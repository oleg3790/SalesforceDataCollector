using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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
using SalesforceDataCollector.Models;

namespace SalesforceDataCollector.Client
{
    public class SalesforceClient : ISalesforceClient
    {
        private const string _salesforceLoginBaseUrl = "https://login.salesforce.com";

        private readonly string _authToken;

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

            _authToken = GenerateAuthJWTToken();
        }

        public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            try
            {
                var sfAuth = Authenticate();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encountered while trying to get all Salesforce accounts");
            }
        }

        /// <summary>
        /// Authenticates this client to the Salesforce API
        /// </summary>
        private async Task<SalesforceAuthResponse> Authenticate()
        {
            var response = await _client.PostAsync
            (
                $"{_salesforceLoginBaseUrl}/services/oauth2/token",
                new FormUrlEncodedContent
                (
                    new[] {
                        new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                        new KeyValuePair<string, string>("assertion", _authToken)
                    }
                )
            );

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Could not authenticate to Salesforce.\nResponse Code: {response.StatusCode}\nResponse Content: {responseContent}");
            }

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
                { "iss", _config.GetValue<string>("Salesforce:ClientId") },
                { "aud", _salesforceLoginBaseUrl },
                { "sub", _config.GetValue<string>("Salesforce:User") },
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
