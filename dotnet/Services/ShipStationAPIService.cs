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
using System.Reflection;

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
                case ShipStationConstants.VtexOrderStatus.PaymentApproved:
                    status = ShipStationConstants.ShipStationOrderStatus.AwaitingShipment;
                    break;
                case ShipStationConstants.VtexOrderStatus.Cancel:
                case ShipStationConstants.VtexOrderStatus.Canceled:
                    status = ShipStationConstants.ShipStationOrderStatus.Canceled;
                    break;
                case ShipStationConstants.VtexOrderStatus.ApprovePayment:
                case ShipStationConstants.VtexOrderStatus.AuthorizeFullfilment:
                case ShipStationConstants.VtexOrderStatus.CancellationRequested:
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

            Console.WriteLine($"-----> Vtex status = '{orderStatus}' ShipStation Status = '{status}' <-----");

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

                //Console.WriteLine($"SendRequest [{responseMessage.StatusCode}] {responseContent}");
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
            ResponseWrapper responseWrapper = null;
            if (vtexOrder != null)
            {
                string url = $"https://{ShipStationConstants.API.HOST}/{ShipStationConstants.API.ORDERS}/{ShipStationConstants.API.CREATE_ORDER}";
                MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();

                //StringBuilder custom1 = new StringBuilder();
                //var assemblies = vtexOrder.Items.SelectMany(i => i.Assemblies);
                //foreach (var assembly in assemblies)
                //{
                //    foreach (PropertyInfo prop in assembly.GetType().GetProperties())
                //    {
                //        var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                //        try
                //        {
                //            custom1.AppendLine(prop.GetValue(assembly, null).ToString());
                //            Console.WriteLine($"- {type}   {prop.GetValue(assembly, null)}");
                //        }
                //        catch(Exception ex)
                //        {
                //            Console.WriteLine($"- err - {type}   {ex.Message}");
                //        }

                //    }
                //}

                //custom1.AppendJoin("|", assemblies);

                ShipStationOrder createUpdateOrderRequest = new ShipStationOrder();
                createUpdateOrderRequest.AdvancedOptions = new AdvancedOptions
                {
                    
                };

                //createUpdateOrderRequest.AmountPaid = ToDollar(vtexOrder.Totals.Sum(t => t.Value));
                //createUpdateOrderRequest.ShippingAmount = ToDollar(vtexOrder.Totals.Where(t => t.Id == "Shipping").Select(d => d.Value).FirstOrDefault());
                //createUpdateOrderRequest.TaxAmount = ToDollar(vtexOrder.Totals.Where(t => t.Id == "Tax").Select(d => d.Value).FirstOrDefault());
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
                //createUpdateOrderRequest.Confirmation = ShipStationConstants.ShipStationConfirmation.None;
                createUpdateOrderRequest.CustomerEmail = vtexOrder.ClientProfileData.Email;
                //createUpdateOrderRequest.CustomerId = 0;  // read-only
                createUpdateOrderRequest.CustomerNotes = null;
                createUpdateOrderRequest.CustomerUsername = vtexOrder.ClientProfileData.Email;
                createUpdateOrderRequest.Dimensions = new Dimensions
                {

                };

                createUpdateOrderRequest.Gift = vtexOrder.Items.Any(i => i.IsGift ?? false);
                createUpdateOrderRequest.GiftMessage = null;
                createUpdateOrderRequest.InsuranceOptions = new InsuranceOptions
                {

                };

                createUpdateOrderRequest.InternalNotes = null;
                createUpdateOrderRequest.InternationalOptions = new InternationalOptions
                {

                };

                createUpdateOrderRequest.OrderDate = vtexOrder.CreationDate;
                createUpdateOrderRequest.OrderKey = vtexOrder.Sequence;
                createUpdateOrderRequest.OrderNumber = vtexOrder.OrderId;
                createUpdateOrderRequest.OrderStatus = await this.GetShipStationOrderStatus(vtexOrder.State);
                createUpdateOrderRequest.PackageCode = null;
                createUpdateOrderRequest.PaymentDate = vtexOrder.ReceiptData.ReceiptCollection.Select(r => r.Date).FirstOrDefault();
                List<string> paymentMethods = new List<string>();
                // Need to find format for gift cards
                //paymentMethods.AddRange(vtexOrder.PaymentData.GiftCards.)
                paymentMethods.AddRange(vtexOrder.PaymentData.Transactions.SelectMany(t => t.Payments).Select(p => p.PaymentSystemName).Distinct().ToList());
                createUpdateOrderRequest.PaymentMethod = String.Join(", ", paymentMethods.ToArray());
                createUpdateOrderRequest.RequestedShippingService = vtexOrder.ShippingData.LogisticsInfo.SelectMany(l => l.Slas).SelectMany(s => s.DeliveryIds).Select(d => d.CourierName).FirstOrDefault();
                createUpdateOrderRequest.ServiceCode = null;
                createUpdateOrderRequest.ShipByDate = null;
                createUpdateOrderRequest.ShipDate = null;
                createUpdateOrderRequest.ShipTo = new ToAddress
                {
                    City = vtexOrder.ShippingData.Address.City,
                    Company = null,
                    Country = vtexOrder.ShippingData.Address.Country.Substring(0, 2),
                    Name = vtexOrder.ShippingData.Address.ReceiverName,
                    Phone = vtexOrder.ClientProfileData.Phone,
                    PostalCode = vtexOrder.ShippingData.Address.PostalCode,
                    Residential = vtexOrder.ShippingData.Address.AddressType == "residential",
                    State = vtexOrder.ShippingData.Address.State,
                    Street1 = vtexOrder.ShippingData.Address.Street,
                    //Street2 = vtexOrder.ShippingData.Address.Number.ToString(),
                    //Street3 = vtexOrder.ShippingData.Address.Neighborhood
                };

                createUpdateOrderRequest.TagIds = new List<long>();
                createUpdateOrderRequest.Weight = new Weight
                {

                };

                createUpdateOrderRequest.Items = new List<OrderItem>();

                Dictionary<string, List<OrderItem>> splitItems = new Dictionary<string, List<OrderItem>>();
                Dictionary<string, SplitItemsWrapper> splitItemsTotal = new Dictionary<string, SplitItemsWrapper>();
                long totalTax = 0L;
                long itemsTotal = 0L;
                long shippingTax = 0L;
                long shippingTotal = 0L;
                foreach (VtexOrderItem item in vtexOrder.Items)
                {
                    Console.WriteLine($"    ----------  [{item.Id}] '{item.Name}' --------------  ");
                    LogisticsInfo logisticsInfo = vtexOrder.ShippingData.LogisticsInfo.Where(l => l.ItemId.Equals(item.Id)).FirstOrDefault();
                    Sla sla = logisticsInfo.Slas.Where(s => s.Id.Equals(logisticsInfo.SelectedSla)).FirstOrDefault();
                    if (!merchantSettings.SendPickupInStore && (sla.PickupStoreInfo.IsPickupStore ?? false))
                    {
                        Console.WriteLine($"{item.Name} is Pickup.  Skipping.");
                    }
                    else
                    {
                        long itemTax = 0L;
                        foreach (PriceTag priceTag in item.PriceTags)
                        {
                            string name = priceTag.Name.ToLower();
                            if (name.Contains("tax@") || name.Contains("taxhub@"))
                            {
                                if (priceTag.IsPercentual ?? false)
                                {
                                    itemTax += (long)(item.SellingPrice * item.Quantity * priceTag.RawValue);
                                }
                                else
                                {
                                    itemTax += priceTag.Value * item.Quantity;
                                }
                            }
                        }

                        totalTax += itemTax;
                        itemsTotal += item.SellingPrice * item.Quantity;
                        shippingTax += sla.Tax;
                        shippingTotal += sla.Price;

                        OrderItem orderItem = new OrderItem();
                        orderItem.Adjustment = false;
                        orderItem.FulfillmentSku = item.SellerSku;
                        orderItem.ImageUrl = item.ImageUrl;
                        orderItem.LineItemKey = item.Id;
                        orderItem.Name = item.Name;
                        orderItem.Options = new List<Option>();

                        //foreach (PropertyInfo prop in item.AdditionalInfo.GetType().GetProperties())
                        //{
                        //    var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        //    try
                        //    {
                        //        Option option = new Option
                        //        {
                        //            Name = prop.Name,
                        //            Value = prop.GetValue(item.AdditionalInfo, null).ToString()
                        //        };

                        //        orderItem.Options.Add(option);
                        //        Console.WriteLine($"- {type} '{prop.Name}'  {prop.GetValue(item.AdditionalInfo, null)}");
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        Console.WriteLine($"- err - {type}   {ex.Message}");
                        //    }
                        //}

                        orderItem.Options.Add(new Option { Name = "Brand Id", Value = item.AdditionalInfo.BrandId });
                        orderItem.Options.Add(new Option { Name = "Brand Name", Value = item.AdditionalInfo.BrandName });
                        orderItem.Options.Add(new Option { Name = "Categories Ids", Value = item.AdditionalInfo.CategoriesIds });

                        foreach (ItemAssembly assembly in item.Assemblies)
                        {
                            Option option = new Option
                            {
                                Name = assembly.Id,
                                Value = JsonConvert.SerializeObject(assembly.InputValues)
                            };

                            orderItem.Options.Add(option);
                        }

                        orderItem.ProductId = item.ProductId;
                        orderItem.Quantity = item.Quantity;
                        orderItem.ShippingAmount = ToDollar(sla.Price);
                        orderItem.Sku = item.SellerSku;
                        orderItem.TaxAmount = ToDollar(itemTax);
                        orderItem.UnitPrice = ToDollar(item.SellingPrice);
                        orderItem.Upc = item.Ean;
                        orderItem.WarehouseLocation = null;
                        orderItem.Weight = new Weight
                        {
                            Units = merchantSettings.WeightUnit,
                            Value = item.AdditionalInfo.Dimension.Weight
                        };

                        string warehouseId = string.Empty;
                        if (merchantSettings.SplitShipmentByLocation)
                        {
                            List<DeliveryId> deliveryIds = sla.DeliveryIds;
                            warehouseId = deliveryIds.Select(w => w.WarehouseId).FirstOrDefault();
                            Console.WriteLine($"--------------------------------->     Setting Warehouse Id to '{warehouseId}'");
                        }

                        if (splitItems.Keys.Contains(warehouseId))
                        {
                            if (splitItems[warehouseId] != null)
                            {
                                splitItems[warehouseId].Add(orderItem);
                            }
                            else
                            {
                                splitItems[warehouseId] = new List<OrderItem>();
                                splitItems[warehouseId].Add(orderItem);
                            }
                        }
                        else
                        {
                            splitItems.Add(warehouseId, new List<OrderItem>());
                            splitItems[warehouseId].Add(orderItem);
                        }

                        if (splitItemsTotal.Keys.Contains(warehouseId))
                        {
                            if (splitItemsTotal[warehouseId] != null)
                            {
                                splitItemsTotal[warehouseId].ShippingAmount += orderItem.ShippingAmount ?? 0D;
                                splitItemsTotal[warehouseId].TaxAmount += orderItem.TaxAmount ?? 0D;
                                splitItemsTotal[warehouseId].AmountPaid += ((orderItem.UnitPrice * orderItem.Quantity) + orderItem.ShippingAmount ?? 0D + orderItem.TaxAmount ?? 0D + ToDollar(sla.Tax));
                            }
                            else
                            {
                                splitItemsTotal[warehouseId] = new SplitItemsWrapper
                                {
                                    AmountPaid = ((orderItem.UnitPrice * orderItem.Quantity) + orderItem.ShippingAmount ?? 0D + orderItem.TaxAmount ?? 0D + ToDollar(sla.Tax)),
                                    ShippingAmount = orderItem.ShippingAmount ?? 0D,
                                    TaxAmount = orderItem.TaxAmount ?? 0D
                                };
                            }
                        }
                        else
                        {
                            SplitItemsWrapper itemsWrapper = new SplitItemsWrapper
                            {
                                AmountPaid = ((orderItem.UnitPrice * orderItem.Quantity) + orderItem.ShippingAmount ?? 0D + orderItem.TaxAmount ?? 0D + ToDollar(sla.Tax)),
                                ShippingAmount = orderItem.ShippingAmount ?? 0D,
                                TaxAmount = orderItem.TaxAmount ?? 0D
                            };

                            splitItemsTotal.Add(warehouseId, itemsWrapper);
                        }

                        //createUpdateOrderRequest.Items.Add(orderItem);
                    }
                }

                createUpdateOrderRequest.AmountPaid = ToDollar(itemsTotal + totalTax + shippingTax + shippingTotal);
                createUpdateOrderRequest.ShippingAmount = ToDollar(shippingTotal);
                createUpdateOrderRequest.TaxAmount = ToDollar(totalTax);

                foreach(string warehouseId in splitItems.Keys)
                {
                    Console.WriteLine($"--------------------------------->  Warehouse Id '{warehouseId}' {splitItems[warehouseId].Count} Items.");
                    ShipStationOrder shipStationOrderTemp = createUpdateOrderRequest;
                    if (!string.IsNullOrEmpty(warehouseId))
                    {
                        shipStationOrderTemp.OrderKey = shipStationOrderTemp.OrderKey + "|" + warehouseId;
                        shipStationOrderTemp.AmountPaid = splitItemsTotal[warehouseId].AmountPaid;
                        shipStationOrderTemp.ShippingAmount = splitItemsTotal[warehouseId].ShippingAmount;
                        shipStationOrderTemp.TaxAmount = splitItemsTotal[warehouseId].TaxAmount;

                        List<ListWarehousesResponse> listWarehouses = await this.ListWarehouses();
                        shipStationOrderTemp.AdvancedOptions.WarehouseId = listWarehouses.Where(w => w.WarehouseName.Equals(warehouseId)).Select(w => w.WarehouseId).FirstOrDefault();
                    }

                    //shipStationOrderTemp.AdvancedOptions.WarehouseId = warehouseId;
                    shipStationOrderTemp.Items = splitItems[warehouseId];
                    if (shipStationOrderTemp.Items.Count > 0)
                    {
                        responseWrapper = await this.SendRequest(url, shipStationOrderTemp);
                        _context.Vtex.Logger.Info("CreateUpdateOrder", null, $"OrderKey={vtexOrder.OrderFormId} OrderNumber={vtexOrder.Sequence} '{vtexOrder.State}'='{shipStationOrderTemp.OrderStatus}'");
                        //Console.WriteLine($"CreateUpdateOrder '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
                        Console.WriteLine($"CreateUpdateOrder '{responseWrapper.Message}' [{responseWrapper.IsSuccess}]");
                        //Console.WriteLine($"CreateUpdateOrder [{responseWrapper.IsSuccess}]");

                        _context.Vtex.Logger.Info("CreateUpdateOrder", shipStationOrderTemp.OrderNumber, JsonConvert.SerializeObject(shipStationOrderTemp));
                    }
                }

                //_context.Vtex.Logger.Info("CreateUpdateOrder", createUpdateOrderRequest.OrderNumber, $"Paid={createUpdateOrderRequest.AmountPaid} (items={itemsTotal} tax={totalTax} shippingTax={shippingTax} shippingTotal={shippingTotal})");
                //_context.Vtex.Logger.Info("CreateUpdateOrder", createUpdateOrderRequest.OrderNumber, JsonConvert.SerializeObject(createUpdateOrderRequest));

                //if (createUpdateOrderRequest.Items.Count > 0)
                //{
                //    responseWrapper = await this.SendRequest(url, createUpdateOrderRequest);
                //    _context.Vtex.Logger.Info("CreateUpdateOrder", null, $"OrderKey={vtexOrder.OrderFormId} OrderNumber={vtexOrder.Sequence} '{vtexOrder.State}'='{createUpdateOrderRequest.OrderStatus}'");
                //    //Console.WriteLine($"CreateUpdateOrder '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
                //    Console.WriteLine($"CreateUpdateOrder '{responseWrapper.Message}' [{responseWrapper.IsSuccess}]");
                //    //Console.WriteLine($"CreateUpdateOrder [{responseWrapper.IsSuccess}]");
                //}
                //else
                //{
                //    responseWrapper = new ResponseWrapper
                //    {
                //        IsSuccess = false,
                //        Message = "No Ship Items"
                //    };
                //}
            }

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

        public async Task<CreateWarehouseResponse> CreateWarehouse(CreateWarehouseRequest createWarehouseRequest)
        {
            CreateWarehouseResponse createWarehouseResponse = null;
            string url = $"https://{ShipStationConstants.API.HOST}/{ShipStationConstants.API.WAREHOUSES}/{ShipStationConstants.API.CREATE_WAREHOUSE}";
            ResponseWrapper responseWrapper = await this.SendRequest(url, createWarehouseRequest);
            Console.WriteLine($"CreateWarehouse '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
            _context.Vtex.Logger.Info("CreateWarehouse", null, JsonConvert.SerializeObject(responseWrapper));

            if(responseWrapper.IsSuccess)
            {
                createWarehouseResponse = JsonConvert.DeserializeObject<CreateWarehouseResponse>(responseWrapper.ResponseText);
            }
            else
            {
                Console.WriteLine($"CreateWarehouse '{responseWrapper.Message}'");
            }

            return createWarehouseResponse;
        }

        public async Task<List<ListWarehousesResponse>> ListWarehouses()
        {
            List<ListWarehousesResponse> response = null;
            string url = $"https://{ShipStationConstants.API.HOST}/{ShipStationConstants.API.WAREHOUSES}";
            ResponseWrapper responseWrapper = await this.GetRequest(url);
            Console.WriteLine($"ListWarehouses '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
            _context.Vtex.Logger.Info("ListWarehouses", null, JsonConvert.SerializeObject(responseWrapper));
            if (responseWrapper.IsSuccess)
            {
                response = JsonConvert.DeserializeObject<List<ListWarehousesResponse>>(responseWrapper.ResponseText);
            }

            return response;
        }
    }
}
