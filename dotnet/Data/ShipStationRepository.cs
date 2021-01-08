namespace ShipStation.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using ShipStation.Models;
    using ShipStation.Services;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Vtex.Api.Context;

    public class ShipStationRepository : IShipStationRepository
    {
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IIOServiceContext _context;
        private readonly string _applicationName;

        public ShipStationRepository(IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context)
        {
            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                               throw new ArgumentNullException(nameof(clientFactory));

            this._context = context ??
                               throw new ArgumentNullException(nameof(context));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
        }

        public async Task<MerchantSettings> GetMerchantSettings()
        {
            // Load merchant settings
            // 'http://apps.{{region}}.vtex.io/{{account}}/{{workspace}}/apps/{{appName}}/settings'
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://apps.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_WORKSPACE_HEADER_NAME]}/apps/{ShipStationConstants.APP_SETTINGS}/settings"),
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<MerchantSettings>(responseContent);
        }

        public async Task<List<ListStoresResponse>> GetShipStationStores()
        {
            List<ListStoresResponse> listStoresResponses = null;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_WORKSPACE_HEADER_NAME]}/buckets/{this._applicationName}/{ShipStationConstants.BUCKET}/files/{ShipStationConstants.STORE_LIST}"),
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            //request.Headers.Add("Cache-Control", "no-cache");

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            Console.WriteLine($"GetShipStationStores {response.StatusCode}:{response.ReasonPhrase}");
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                listStoresResponses = JsonConvert.DeserializeObject<List<ListStoresResponse>>(responseContent);
            }

            return listStoresResponses;
        }

        public async Task SaveShipStationStoreList(List<ListStoresResponse> listOfStores)
        {
            var jsonSerializedStoreList = JsonConvert.SerializeObject(listOfStores);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_WORKSPACE_HEADER_NAME]}/buckets/{this._applicationName}/{ShipStationConstants.BUCKET}/files/{ShipStationConstants.STORE_LIST}"),
                Content = new StringContent(jsonSerializedStoreList, Encoding.UTF8, ShipStationConstants.APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            Console.WriteLine($"SaveShipStationStoreList {response.StatusCode}:{response.ReasonPhrase}");

            response.EnsureSuccessStatusCode();
        }

        public async Task<DateTime> GetLastShipmentCheck()
        {
            DateTime lastCheck = DateTime.Now;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_WORKSPACE_HEADER_NAME]}/buckets/{this._applicationName}/{ShipStationConstants.BUCKET}/files/{ShipStationConstants.SHIPMENT_CHECK}"),
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            Console.WriteLine($"GetLastShipmentCheck {response.StatusCode}:{response.ReasonPhrase}");
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseContent))
                {
                    lastCheck = JsonConvert.DeserializeObject<DateTime>(responseContent);
                }
                else
                {
                    await this.SetLastShipmentCheck(lastCheck);
                }
            }
            else
            {
                await this.SetLastShipmentCheck(lastCheck.AddDays(-7));
            }

            // DEBUG !!!!
            //lastCheck = DateTime.Now.AddHours(-72);
            // DEBUG !!!!

            return lastCheck;
        }

        public async Task SetLastShipmentCheck(DateTime lastCheck)
        {
            var jsonSerializedStoreList = JsonConvert.SerializeObject(lastCheck);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_WORKSPACE_HEADER_NAME]}/buckets/{this._applicationName}/{ShipStationConstants.BUCKET}/files/{ShipStationConstants.SHIPMENT_CHECK}"),
                Content = new StringContent(jsonSerializedStoreList, Encoding.UTF8, ShipStationConstants.APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            Console.WriteLine($"    -----------------------     SetLastShipmentCheck {response.StatusCode}:{response.ReasonPhrase}      -------------------------------     ");

            response.EnsureSuccessStatusCode();
        }

        public async Task<DateTime> GetLastCancelledOrderCheck()
        {
            DateTime lastCheck = DateTime.Now;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_WORKSPACE_HEADER_NAME]}/buckets/{this._applicationName}/{ShipStationConstants.BUCKET}/files/{ShipStationConstants.CANCELLED_ORDER_CHECK}"),
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            Console.WriteLine($"GetLastCancelledOrderCheck {response.StatusCode}:{response.ReasonPhrase}");
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseContent))
                {
                    lastCheck = JsonConvert.DeserializeObject<DateTime>(responseContent);
                }
                else
                {
                    await this.SetLastCancelledOrderCheck(lastCheck);
                }
            }
            else
            {
                await this.SetLastCancelledOrderCheck(lastCheck.AddDays(-1));
            }

            // DEBUG !!!!
            //lastCheck = DateTime.Now.AddHours(-72);
            // DEBUG !!!!

            return lastCheck;
        }

        public async Task SetLastCancelledOrderCheck(DateTime lastCheck)
        {
            var jsonSerializedStoreList = JsonConvert.SerializeObject(lastCheck);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_WORKSPACE_HEADER_NAME]}/buckets/{this._applicationName}/{ShipStationConstants.BUCKET}/files/{ShipStationConstants.CANCELLED_ORDER_CHECK}"),
                Content = new StringContent(jsonSerializedStoreList, Encoding.UTF8, ShipStationConstants.APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            Console.WriteLine($"    -----------------------     SetLastCancelledOrderCheck {response.StatusCode}:{response.ReasonPhrase}      -------------------------------     ");

            response.EnsureSuccessStatusCode();
        }
    }
}
