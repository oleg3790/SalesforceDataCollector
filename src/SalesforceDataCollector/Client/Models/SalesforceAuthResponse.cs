using Newtonsoft.Json;

namespace SalesforceDataCollector.Client.Models
{
    public class SalesforceAuthResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("instance_url")]
        public string InstanceUrl { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}
