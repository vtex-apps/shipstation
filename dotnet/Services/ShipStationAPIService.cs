using ShipStation.Data;
using ShipStation.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vtex.Api.Context;

namespace ShipStation.Services
{
    public class ShipStationAPIService : IShipStationAPIService
    {
        private readonly IIOServiceContext _context;
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IShipStationRepository _shipStationRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _applicationName;

        public ShipStationAPIService(IIOServiceContext context, IVtexEnvironmentVariableProvider environmentVariableProvider, IShipStationRepository shipStationRepository, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory)
        {
            this._context = context ??
                            throw new ArgumentNullException(nameof(context));

            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._shipStationRepository = shipStationRepository ??
                                                throw new ArgumentNullException(nameof(shipStationRepository));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                               throw new ArgumentNullException(nameof(clientFactory));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
        }

        private double ToDollar(long asPennies)
        {
            return (double)asPennies / 100;
        }

        private async Task<string> GetShipStationOrderStatus(string orderStatus)
        {
            string status = ShipStationConstants.ShipStationOrderStatus.OnHold;
            switch(orderStatus)
            {
                case ShipStationConstants.VtexOrderStatus.Handling:
                case ShipStationConstants.VtexOrderStatus.Invoiced:
                case ShipStationConstants.VtexOrderStatus.ReadyForHandling:
                case ShipStationConstants.VtexOrderStatus.StartHanding:
                    status = ShipStationConstants.ShipStationOrderStatus.AwaitingShipment;
                    break;
                case ShipStationConstants.VtexOrderStatus.Cancel:
                case ShipStationConstants.VtexOrderStatus.Canceled:
                    status = ShipStationConstants.ShipStationOrderStatus.Canceled;
                    break;
                case ShipStationConstants.VtexOrderStatus.ApprovePayment:
                case ShipStationConstants.VtexOrderStatus.AuthorizeFullfilment:
                case ShipStationConstants.VtexOrderStatus.CancellationRequested:
                case ShipStationConstants.VtexOrderStatus.PaymentApproved:
                case ShipStationConstants.VtexOrderStatus.RequestCancel:
                case ShipStationConstants.VtexOrderStatus.WaitingForOrderAuthorization:
                case ShipStationConstants.VtexOrderStatus.WaitingForSellerDecision:
                case ShipStationConstants.VtexOrderStatus.WindowToCancel:
                    status = ShipStationConstants.ShipStationOrderStatus.OnHold;
                    break;
                case ShipStationConstants.VtexOrderStatus.Invoice:
                case ShipStationConstants.VtexOrderStatus.InvoiceAfterCancellationDeny:
                case ShipStationConstants.VtexOrderStatus.OrderAccepted:
                case ShipStationConstants.VtexOrderStatus.OrderCreated:
                case ShipStationConstants.VtexOrderStatus.PaymentDenied:
                case ShipStationConstants.VtexOrderStatus.PaymentPending:
                    status = ShipStationConstants.ShipStationOrderStatus.AwaitingPayment;
                    break;
                case ShipStationConstants.VtexOrderStatus.OnOrderCompleted:
                case ShipStationConstants.VtexOrderStatus.OrderCompleted:
                    status = ShipStationConstants.ShipStationOrderStatus.Shipped;
                    break;
                case ShipStationConstants.VtexOrderStatus.OrderCreateError:
                case ShipStationConstants.VtexOrderStatus.OrderCreationError:
                case ShipStationConstants.VtexOrderStatus.Replaced:
                    break;
            }

            return status;
        }

        private async Task<ResponseWrapper> SendRequest(string url, object requestObject)
        {
            ResponseWrapper responseWrapper = null;
            MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();

            string jsonSerializedRequest = JsonConvert.SerializeObject(requestObject);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StringContent(jsonSerializedRequest, Encoding.UTF8, ShipStationConstants.APPLICATION_JSON)
            };

            request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{merchantSettings.ApiKey}:{merchantSettings.ApiSecret}")));
            request.Headers.Add(ShipStationConstants.USE_HTTPS_HEADER_NAME, "true");

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();

            try
            {
                HttpResponseMessage responseMessage = await client.SendAsync(request);
                string responseContent = await responseMessage.Content.ReadAsStringAsync();
                responseWrapper = new ResponseWrapper
                {
                    IsSuccess = responseMessage.IsSuccessStatusCode,
                    ResponseText = responseContent
                };
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("SendRequest", null, $"Error Sending Request to {request.RequestUri} {jsonSerializedRequest}", ex);
                responseWrapper = new ResponseWrapper
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

            return responseWrapper;
        }

        private async Task<ResponseWrapper> GetRequest(string url)
        {
            ResponseWrapper responseWrapper = null;
            MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };

            request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{merchantSettings.ApiKey}:{merchantSettings.ApiSecret}")));
            request.Headers.Add(ShipStationConstants.USE_HTTPS_HEADER_NAME, "true");

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();

            try
            {
                HttpResponseMessage responseMessage = await client.SendAsync(request);
                string responseContent = await responseMessage.Content.ReadAsStringAsync();
                responseWrapper = new ResponseWrapper
                {
                    IsSuccess = responseMessage.IsSuccessStatusCode,
                    ResponseText = responseContent
                };
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetRequest", null, $"Error Sending Request to {request.RequestUri}", ex);
                responseWrapper = new ResponseWrapper
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

            return responseWrapper;
        }

        public async Task<bool> CreateUpdateOrder(VtexOrder vtexOrder)
        {
            ShipStationOrder createUpdateOrderRequest = new ShipStationOrder();
            createUpdateOrderRequest.AdvancedOptions = new AdvancedOptions
            {

            };
            createUpdateOrderRequest.AmountPaid = ToDollar(vtexOrder.Totals.Sum(t => t.Value));
            createUpdateOrderRequest.ShippingAmount = 0;
            createUpdateOrderRequest.TaxAmount = 0;
            createUpdateOrderRequest.BillTo = new ToAddress
            {
                City = vtexOrder.ShippingData.Address.City,
                Company = null,
                Country = vtexOrder.ShippingData.Address.Country.Substring(0, 2),
                Name = vtexOrder.ShippingData.Address.ReceiverName,
                Phone = vtexOrder.ClientProfileData.Phone,
                PostalCode = vtexOrder.ShippingData.Address.PostalCode,
                Residential = null,
                State = vtexOrder.ShippingData.Address.State,
                Street1 = vtexOrder.ShippingData.Address.Street,
                //Street2 = vtexOrder.ShippingData.Address.Number.ToString(),
                //Street3 = vtexOrder.ShippingData.Address.Neighborhood
            };
            createUpdateOrderRequest.CarrierCode = null;
            createUpdateOrderRequest.Confirmation = ShipStationConstants.ShipStationConfirmation.None;
            createUpdateOrderRequest.CustomerEmail = vtexOrder.ClientProfileData.Email;
            createUpdateOrderRequest.CustomerId = null;
            createUpdateOrderRequest.CustomerNotes = null;
            createUpdateOrderRequest.CustomerUsername = null;
            createUpdateOrderRequest.Dimensions = new Dimensions
            {

            };
            createUpdateOrderRequest.Gift = null;
            createUpdateOrderRequest.GiftMessage = null;
            createUpdateOrderRequest.InsuranceOptions = new InsuranceOptions
            {

            };
            createUpdateOrderRequest.InternalNotes = null;
            createUpdateOrderRequest.InternationalOptions = new InternationalOptions
            {

            };

            createUpdateOrderRequest.Items = new List<OrderItem>();
            foreach (VtexOrderItem item in vtexOrder.Items)
            {
                OrderItem orderItem = new OrderItem();
                orderItem.Adjustment = false;
                orderItem.FulfillmentSku = item.SellerSku;
                orderItem.ImageUrl = item.ImageUrl;
                orderItem.LineItemKey = item.Id;
                orderItem.Name = item.Name;
                orderItem.Options = new List<Option>();
                orderItem.ProductId = item.ProductId;
                orderItem.Quantity = item.Quantity;
                orderItem.ShippingAmount = null;
                orderItem.Sku = item.SellerSku;
                orderItem.TaxAmount = null;
                orderItem.UnitPrice = ToDollar(item.ListPrice);
                orderItem.Upc = null;
                orderItem.WarehouseLocation = null;
                orderItem.Weight = null;

                createUpdateOrderRequest.Items.Add(orderItem);
            }

            createUpdateOrderRequest.OrderDate = vtexOrder.CreationDate;
            createUpdateOrderRequest.OrderKey = vtexOrder.OrderFormId;
            createUpdateOrderRequest.OrderNumber = vtexOrder.Sequence;
            createUpdateOrderRequest.OrderStatus = await this.GetShipStationOrderStatus(vtexOrder.State);
            createUpdateOrderRequest.PackageCode = null;
            createUpdateOrderRequest.PaymentDate = null; // ReceiptCollection?
            createUpdateOrderRequest.PaymentMethod = null; // paymentData?
            createUpdateOrderRequest.RequestedShippingService = null; // logisticsInfo
            createUpdateOrderRequest.ServiceCode = null;
            createUpdateOrderRequest.ShipByDate = null;
            createUpdateOrderRequest.ShipDate = null;
            createUpdateOrderRequest.ShipTo = new ToAddress
            {
                City = vtexOrder.ShippingData.Address.City,
                Company = null,
                Country = vtexOrder.ShippingData.Address.Country.Substring(0,2),
                Name = vtexOrder.ShippingData.Address.ReceiverName,
                Phone = vtexOrder.ClientProfileData.Phone,
                PostalCode = vtexOrder.ShippingData.Address.PostalCode,
                Residential = null,
                State = vtexOrder.ShippingData.Address.State,
                Street1 = vtexOrder.ShippingData.Address.Street,
                //Street2 = vtexOrder.ShippingData.Address.Number.ToString(),
                //Street3 = vtexOrder.ShippingData.Address.Neighborhood
            };
            createUpdateOrderRequest.TagIds = new List<long>();
            createUpdateOrderRequest.Weight = new Weight
            {

            };

            string url = $"https://{ShipStationConstants.API.HOST}/{ShipStationConstants.API.ORDERS}/{ShipStationConstants.API.CREATE_ORDER}";
            ResponseWrapper responseWrapper = await this.SendRequest(url, createUpdateOrderRequest);
            Console.WriteLine($"CreateUpdateOrder '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");

            _context.Vtex.Logger.Info("CreateUpdateOrder", null, $"OrderKey={vtexOrder.OrderFormId} OrderNumber={vtexOrder.Sequence} '{vtexOrder.State}'='{createUpdateOrderRequest.OrderStatus}'" );

            return responseWrapper.IsSuccess;
        }

        public async Task<string> SubscribeToWebhook(string hookEvent)
        {
            string response = string.Empty;
            string friendlyName = null;
            bool validHook = false;
            switch (hookEvent)
            {
                case ShipStationConstants.WebhookEvent.ITEM_ORDER_NOTIFY:
                    friendlyName = ShipStationConstants.WebhookEvent.FriendlyName.ITEM_ORDER_NOTIFY;
                    validHook = true;
                    break;
                case ShipStationConstants.WebhookEvent.ITEM_SHIP_NOTIFY:
                    friendlyName = ShipStationConstants.WebhookEvent.FriendlyName.ITEM_SHIP_NOTIFY;
                    validHook = true;
                    break;
                case ShipStationConstants.WebhookEvent.ORDER_NOTIFY:
                    friendlyName = ShipStationConstants.WebhookEvent.FriendlyName.ORDER_NOTIFY;
                    validHook = true;
                    break;
                case ShipStationConstants.WebhookEvent.SHIP_NOTIFY:
                    friendlyName = ShipStationConstants.WebhookEvent.FriendlyName.SHIP_NOTIFY;
                    validHook = true;
                    break;
            }

            if (validHook)
            {
                string siteUrl = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.FORWARDED_HOST];

                SubscribeToWebhookRequest subscribeToWebhookRequest = new SubscribeToWebhookRequest
                {
                    Event = hookEvent,
                    FriendlyName = friendlyName,
                    StoreId = null,
                    TargetUrl = $"https://{siteUrl}/{ShipStationConstants.APP_NAME}/{ShipStationConstants.WEB_HOOK_NOTIFICATION}/{hookEvent}"
                };

                string url = $"https://{ShipStationConstants.API.HOST}/{ShipStationConstants.API.WEBHOOKS}/{ShipStationConstants.API.SUBSCRIBE}";
                ResponseWrapper responseWrapper = await this.SendRequest(url, subscribeToWebhookRequest);
                Console.WriteLine($"SubscribeToWebhook '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
                response = responseWrapper.ResponseText;
                _context.Vtex.Logger.Info("SubscribeToWebhook", hookEvent, JsonConvert.SerializeObject(responseWrapper));
            }
            else
            {
                response = $"Invalid Hook Event '{hookEvent}'";
            }

            return response;
        }

        public async Task<string> ProcessResourceUrl(string resourceUrl)
        {
            string response = string.Empty;
            
                ResponseWrapper responseWrapper = await this.GetRequest(resourceUrl);
                Console.WriteLine($"ProcessResourceUrl '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
                response = responseWrapper.ResponseText;
                _context.Vtex.Logger.Info("ProcessResourceUrl", resourceUrl, JsonConvert.SerializeObject(responseWrapper));

            return response;
        }

        public async Task<ListOrdersResponse> ListOrders(string queryPrameters)
        {
            ListOrdersResponse response = null;
            string url = $"https://{ShipStationConstants.API.HOST}/{ShipStationConstants.API.ORDERS}?{queryPrameters}";
            ResponseWrapper responseWrapper = await this.GetRequest(url);
            Console.WriteLine($"ListOrders '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
            _context.Vtex.Logger.Info("ListOrders", null, JsonConvert.SerializeObject(responseWrapper));
            if(responseWrapper.IsSuccess)
            {
                response = JsonConvert.DeserializeObject<ListOrdersResponse>(responseWrapper.ResponseText);
            }

            return response;
        }

        public async Task<ListShipmentsResponse> ListShipments(string queryPrameters)
        {
            ListShipmentsResponse response = null;
            string url = $"https://{ShipStationConstants.API.HOST}/{ShipStationConstants.API.SHIPMENTS}?{queryPrameters}";
            ResponseWrapper responseWrapper = await this.GetRequest(url);
            Console.WriteLine($"ListShipments '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
            _context.Vtex.Logger.Info("ListShipments", null, JsonConvert.SerializeObject(responseWrapper));
            if (responseWrapper.IsSuccess)
            {
                response = JsonConvert.DeserializeObject<ListShipmentsResponse>(responseWrapper.ResponseText);
            }

            return response;
        }

        public async Task<ListFulfillmentsResponse> ListFulFillments(string queryPrameters)
        {
            ListFulfillmentsResponse response = null;
            string url = $"https://{ShipStationConstants.API.HOST}/{ShipStationConstants.API.FULFILLMENTS}?{queryPrameters}";
            ResponseWrapper responseWrapper = await this.GetRequest(url);
            Console.WriteLine($"ListFulFillments '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
            _context.Vtex.Logger.Info("ListFulFillments", null, JsonConvert.SerializeObject(responseWrapper));
            if (responseWrapper.IsSuccess)
            {
                response = JsonConvert.DeserializeObject<ListFulfillmentsResponse>(responseWrapper.ResponseText);
            }

            return response;
        }
    }
}
