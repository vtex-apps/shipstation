﻿using ShipStation.Data;
using ShipStation.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vtex.Api.Context;

namespace ShipStation.Services
{
    public class VtexAPIService : IVtexAPIService
    {
        private readonly IIOServiceContext _context;
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IShipStationRepository _shipStationRepository;
        private readonly IShipStationAPIService _shipStationAPIService;        
        private readonly string _applicationName;

        public VtexAPIService(IIOServiceContext context, IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IShipStationRepository shipStationRepository, IShipStationAPIService shipStationAPIService)
        {
            this._context = context ??
                            throw new ArgumentNullException(nameof(context));

            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                               throw new ArgumentNullException(nameof(clientFactory));

            this._shipStationRepository = shipStationRepository ??
                               throw new ArgumentNullException(nameof(shipStationRepository));

            this._shipStationAPIService = shipStationAPIService ??
                               throw new ArgumentNullException(nameof(shipStationAPIService));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
        }

        public async Task<bool> CreateOrUpdateHook()
        {
            // POST https://{accountName}.{environment}.com.br/api/orders/hook/config
            string baseUrl = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.FORWARDED_HOST];

            HookNotification createOrUpdateHookResponse = new HookNotification();
            OrderHook orderHook = new OrderHook
            {
                Filter = new Filter
                {
                    Status = new List<string>
                    {
                        ShipStationConstants.VtexOrderStatus.ReadyForHandling,
                    }
                },
                Hook = new Hook
                {
                    Headers = new Headers
                    {
                        Key = ShipStationConstants.ENDPOINT_KEY
                    },
                    Url = new Uri($"https://{baseUrl}/{ShipStationConstants.APP_NAME}/{ShipStationConstants.ENDPOINT_KEY}")
                }
            };

            var jsonSerializedOrderHook = JsonConvert.SerializeObject(orderHook);
            //Console.WriteLine($"Hook = {jsonSerializedOrderHook}");
            Console.WriteLine($"Url = https://{baseUrl}/{ShipStationConstants.APP_NAME}/{ShipStationConstants.ENDPOINT_KEY}");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.{ShipStationConstants.ENVIRONMENT}.com.br/api/orders/hook/config"),
                Content = new StringContent(jsonSerializedOrderHook, Encoding.UTF8, ShipStationConstants.APPLICATION_JSON)
            };

            request.Headers.Add(ShipStationConstants.USE_HTTPS_HEADER_NAME, "true");
            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(ShipStationConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
            request.Headers.Add(ShipStationConstants.APP_KEY, merchantSettings.AppKey);
            request.Headers.Add(ShipStationConstants.APP_TOKEN, merchantSettings.AppToken);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[-] CreateOrUpdateHook Response {response.StatusCode} Content = '{responseContent}' [-]");
            _context.Vtex.Logger.Info("CreateOrUpdateHook", "Response", $"[{response.StatusCode}] {responseContent}");
            //if (response.IsSuccessStatusCode)
            //{
            //    createOrUpdateHookResponse = JsonConvert.DeserializeObject<HookNotification>(responseContent);
            //}

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> VerifyHook()
        {
            bool verified = false;
            // POST https://{accountName}.{environment}.com.br/api/orders/hook/config
            string baseUrl = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.FORWARDED_HOST];

            OrderHook verifyHookResponse = new OrderHook();
            OrderHook orderHook = new OrderHook
            {
                Filter = new Filter
                {
                    Status = new List<string>
                    {
                        ShipStationConstants.VtexOrderStatus.ReadyForHandling,
                    }
                },
                Hook = new Hook
                {
                    Headers = new Headers
                    {
                        Key = ShipStationConstants.ENDPOINT_KEY
                    },
                    Url = new Uri($"https://{baseUrl}/{ShipStationConstants.APP_NAME}/{ShipStationConstants.ENDPOINT_KEY}")
                }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.{ShipStationConstants.ENVIRONMENT}.com.br/api/orders/hook/config")
            };

            request.Headers.Add(ShipStationConstants.USE_HTTPS_HEADER_NAME, "true");
            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(ShipStationConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
            request.Headers.Add(ShipStationConstants.APP_KEY, merchantSettings.AppKey);
            request.Headers.Add(ShipStationConstants.APP_TOKEN, merchantSettings.AppToken);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            _context.Vtex.Logger.Info("VerifyHook", "Response", $"[{response.StatusCode}] {responseContent}");
            if (response.IsSuccessStatusCode)
            {
                verifyHookResponse = JsonConvert.DeserializeObject<OrderHook>(responseContent);
                if (verifyHookResponse.Hook.Url.Equals($"https://{baseUrl}/{ShipStationConstants.APP_NAME}/{ShipStationConstants.ENDPOINT_KEY}")
                    && verifyHookResponse.Hook.Headers.Key.Equals(ShipStationConstants.ENDPOINT_KEY))
                {
                    verified = true;
                }
            }

            return verified;
        }

        public async Task<VtexOrder> GetOrderInformation(string orderId)
        {
            //Console.WriteLine("------- Headers -------");
            //foreach (var header in this._httpContextAccessor.HttpContext.Request.Headers)
            //{
            //    Console.WriteLine($"{header.Key}: {header.Value}");
            //}
            //Console.WriteLine($"http://{this._httpContextAccessor.HttpContext.Request.Headers[Constants.VTEX_ACCOUNT_HEADER_NAME]}.{Constants.ENVIRONMENT}.com.br/api/checkout/pvt/orders/{orderId}");

            VtexOrder vtexOrder = null;

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.{ShipStationConstants.ENVIRONMENT}.com.br/api/checkout/pvt/orders/{orderId}")
                };

                request.Headers.Add(ShipStationConstants.USE_HTTPS_HEADER_NAME, "true");
                //request.Headers.Add(Constants.ACCEPT, Constants.APPLICATION_JSON);
                //request.Headers.Add(Constants.CONTENT_TYPE, Constants.APPLICATION_JSON);
                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
                //Console.WriteLine($"Token = '{authToken}'");
                if (authToken != null)
                {
                    request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(ShipStationConstants.VTEX_ID_HEADER_NAME, authToken);
                    request.Headers.Add(ShipStationConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }

                //StringBuilder sb = new StringBuilder();

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    vtexOrder = JsonConvert.DeserializeObject<VtexOrder>(responseContent);
                    Console.WriteLine($"GetOrderInformation: [{response.StatusCode}] ");
                }
                else
                {
                    Console.WriteLine($"GetOrderInformation: [{response.StatusCode}] '{responseContent}'");
                    _context.Vtex.Logger.Info("GetOrderInformation", null, $"Order# {orderId} [{response.StatusCode}] '{responseContent}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetOrderInformation Error: {ex.Message}");
                _context.Vtex.Logger.Error("GetOrderInformation", null, $"Order# {orderId} Error", ex);
            }

            return vtexOrder;
        }

        public async Task<bool> ProcessNotification(HookNotification hookNotification)
        {
            bool success = false;

            switch (hookNotification.Domain)
            {
                case ShipStationConstants.Domain.Fulfillment:
                    switch (hookNotification.State)
                    {
                        case ShipStationConstants.VtexOrderStatus.ReadyForHandling:
                            VtexOrder vtexOrder = await this.GetOrderInformation(hookNotification.OrderId);
                            success = await this._shipStationAPIService.CreateUpdateOrder(vtexOrder);
                            break;
                        default:
                            Console.WriteLine($"State {hookNotification.State} not implemeted.");
                            _context.Vtex.Logger.Info("ProcessNotification", null, $"State {hookNotification.State} not implemeted.");
                            break;
                    }
                    break;
                case ShipStationConstants.Domain.Marketplace:
                    break;
                default:
                    Console.WriteLine($"Domain {hookNotification.Domain} not implemeted.");
                    _context.Vtex.Logger.Info("ProcessNotification", null, $"Domain {hookNotification.Domain} not implemeted.");
                    break;
            }

            return success;
        }
    }
}
