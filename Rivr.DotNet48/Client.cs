using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using System.Runtime.Caching;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rivr.Core;
using Rivr.Core.Models;
using Environment = Rivr.Core.Models.Environment;

namespace Rivr.DotNet48
{
    /// <inheritdoc />
    public class Client : IClient
    {
        /// <inheritdoc />
        public Config Config { get; }

        internal readonly HttpClient AuthHttpClient;
        internal readonly HttpClient ApiHttpClient;
        internal readonly MemoryCache MemoryCache;
        internal readonly string ClientId;
        internal readonly string ClientSecret;
        private PlatformClient _platformOperations;
        private readonly ConcurrentDictionary<Guid, MerchantClient> _merchantClients = new();

        internal JsonSerializerSettings JsonSerializerSettings => new()
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            Converters = { new StringEnumConverter() }
        };

        /// <inheritdoc />
        public Client(Config config) : this(new HttpClient(), new HttpClient(), config)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="authHttpClient"></param>
        /// <param name="apiHttpClient"></param>
        /// <param name="config"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Client(HttpClient authHttpClient, HttpClient apiHttpClient, Config config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            ClientId = config.ClientId ?? throw new ArgumentNullException(nameof(config.ClientId));
            ClientSecret = config.ClientSecret ?? throw new ArgumentNullException(nameof(config.ClientSecret));

            if (string.IsNullOrEmpty(ClientId))
            {
                throw new ArgumentNullException(nameof(ClientId), "ClientId is mandatory");
            }

            if (string.IsNullOrEmpty(ClientSecret))
            {
                throw new ArgumentNullException(nameof(ClientSecret), "ClientSecret is mandatory");
            }

            AuthHttpClient = authHttpClient;
            ApiHttpClient = apiHttpClient;
            MemoryCache = MemoryCache.Default;

            AuthHttpClient.BaseAddress = new Uri(Config.Environment == Environment.Production ? Config.AuthBaseUri : Config.AuthBaseUriTest);
            ApiHttpClient.BaseAddress = new Uri(Config.Environment == Environment.Production ? Config.ApiBaseUri : Config.ApiBaseUriTest);
        }

        /// <inheritdoc />
        public async Task<Health> GetHealthAsync()
        {
            var response = await ApiHttpClient.GetAsync("health");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Health>(content, JsonSerializerSettings);
        }

        /// <inheritdoc />
        public IPlatformOperations AsPlatform()
        {
            if (_platformOperations == null)
            {
                _platformOperations = new PlatformClient(this);
            }

            return _platformOperations;
        }

        /// <inheritdoc />
        public IMerchantOperations OnBehalfOfMerchant(Guid merchantId)
        {
            return _merchantClients.GetOrAdd(merchantId, id => new MerchantClient(this, id));
        }
    }
}