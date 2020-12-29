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
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace ShipStation.Services
{
    public class ShipStationAPIService : IShipStationAPIService
    {
        private readonly IIOServiceContext _context;
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IShipStationRepository _shipStationRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly string _applicationName;

        public ShipStationAPIService(IIOServiceContext context, IVtexEnvironmentVariableProvider environmentVariableProvider, IShipStationRepository shipStationRepository, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IMemoryCache memoryCache)
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

            this._memoryCache = memoryCache ?? 
                                throw new ArgumentNullException(nameof(memoryCache));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
        }

        private Regex invalidXMLChars = new Regex(@"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]", RegexOptions.Compiled);

        private string CleanString(string unClean)
        {
            string retval = string.Empty;
            if (!string.IsNullOrWhiteSpace(unClean))
            {
                retval = invalidXMLChars.Replace(unClean, "");
            }

            return retval;
        }

        private double ToDollar(long asPennies)
        {
            //Console.WriteLine($"ToDollar: {asPennies} = {(double)asPennies / 100D}");
            return (double)asPennies / 100D;
        }

        private async Task<string> GetShipStationOrderStatus(string orderStatus)
        {
            string status = ShipStationConstants.ShipStationOrderStatus.OnHold;
            switch(orderStatus)
            {
                case ShipStationConstants.VtexOrderStatus.Handling:
                case ShipStationConstants.VtexOrderStatus.ReadyForHandling:
                case ShipStationConstants.VtexOrderStatus.StartHanding:
                case ShipStationConstants.VtexOrderStatus.PaymentApproved:
                    status = ShipStationConstants.ShipStationOrderStatus.AwaitingShipment;
                    break;
                case ShipStationConstants.VtexOrderStatus.Cancel:
                case ShipStationConstants.VtexOrderStatus.Canceled:
                case ShipStationConstants.VtexOrderStatus.Cancelled:
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
                case ShipStationConstants.VtexOrderStatus.Invoiced:
                case ShipStationConstants.VtexOrderStatus.OnOrderCompleted:
                case ShipStationConstants.VtexOrderStatus.OrderCompleted:
                    status = ShipStationConstants.ShipStationOrderStatus.Shipped;
                    break;
                case ShipStationConstants.VtexOrderStatus.OrderCreateError:
                case ShipStationConstants.VtexOrderStatus.OrderCreationError:
                case ShipStationConstants.VtexOrderStatus.Replaced:
                    break;
            }

            //Console.WriteLine($"-----> Vtex status = '{orderStatus}' ShipStation Status = '{status}' <-----");

            return status;
        }

        //private async Task<string> FormatInputValues(InputValues inputValues)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    if (!string.IsNullOrEmpty(inputValues.Line1))
        //        sb.AppendLine($"1:{inputValues.Line1}");
        //    if (!string.IsNullOrEmpty(inputValues.Line2))
        //        sb.AppendLine($"2:{inputValues.Line2}");
        //    if (!string.IsNullOrEmpty(inputValues.Line3))
        //        sb.AppendLine($"3:{inputValues.Line3}");
        //    if (!string.IsNullOrEmpty(inputValues.Line4))
        //        sb.AppendLine($"4:{inputValues.Line4}");
        //    if (!string.IsNullOrEmpty(inputValues.TextStyle))
        //        sb.AppendLine($"Text:{inputValues.TextStyle}");

        //    return sb.ToString();
        //}

        //private async Task<List<Option>> FormatInputValues(InputValues inputValues)
        //{
        //    List<Option> options = new List<Option>();
        //    Option option = null;
        //    option = new Option { Name = "Line 1", Value = CleanString(inputValues.Line1) };
        //    options.Add(option);
        //    option = new Option { Name = "Line 2", Value = CleanString(inputValues.Line2) };
        //    options.Add(option);
        //    option = new Option { Name = "Line 3", Value = CleanString(inputValues.Line3) };
        //    options.Add(option);
        //    option = new Option { Name = "Line 4", Value = CleanString(inputValues.Line4) };
        //    options.Add(option);
        //    if (!string.IsNullOrEmpty(inputValues.TextStyle))
        //    {
        //        option = new Option { Name = "Text", Value = CleanString(inputValues.TextStyle) };
        //        options.Add(option);
        //    }

        //    return options;
        //}

        private async Task<List<Option>> FormatInputValues(object inputValues)
        {
            List<Option> options = new List<Option>();
            Option option = null;
            try
            {
                IDictionary<string, JToken> Jsondata = JObject.Parse(inputValues.ToString());
                foreach (KeyValuePair<string, JToken> element in Jsondata)
                {
                    string innerKey = element.Key;
                    string innerValue = element.Value.ToString();
                    option = new Option { Name = innerKey, Value = CleanString(innerValue) };
                    options.Add(option);
                }
            }
            catch(Exception ex)
            {
                _context.Vtex.Logger.Error("FormatInputValues", null, $"Error formatting {inputValues}", ex);
            }

            return options;
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

                if (!responseWrapper.IsSuccess)
                {
                    Console.WriteLine($"Problem Sending Request. Response: '{responseWrapper.ResponseText}'");
                    _context.Vtex.Logger.Info("SendRequest", null, $"Problem Sending Request. Response: '{responseWrapper.ResponseText}' {jsonSerializedRequest}");
                }

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
            if (vtexOrder != null && !string.IsNullOrEmpty(vtexOrder.Status))
            {
                string url = $"https://{ShipStationConstants.API.HOST}/{ShipStationConstants.API.ORDERS}/{ShipStationConstants.API.CREATE_ORDER}";
                MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
                List<ListWarehousesResponse> listWarehouses = null;
                if (!_memoryCache.TryGetValue("ListWarehouses", out listWarehouses))
                {
                    listWarehouses = await this.ListWarehouses();
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1));
                    _memoryCache.Set("ListWarehouses", listWarehouses, cacheEntryOptions);
                }

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
                string customerEmail = vtexOrder.ClientProfileData.Email;
                //if (customerEmail.Contains('-'))
                //{
                //    try
                //    {
                //        string[] emailArray = customerEmail.Split('-');
                //        customerEmail = emailArray[0];
                //    }
                //    catch (Exception ex)
                //    {
                //        _context.Vtex.Logger.Error("CreateUpdateOrder", null, $"Error parsing email {customerEmail}", ex);
                //    }
                //}

                createUpdateOrderRequest.CustomerEmail = customerEmail;
                //createUpdateOrderRequest.CustomerId = 0;  // read-only
                createUpdateOrderRequest.CustomerNotes = CleanString(vtexOrder?.OpenTextField?.Value);
                createUpdateOrderRequest.CustomerUsername = customerEmail;
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

                if (merchantSettings.UseSequenceAsOrderNumber)
                {
                    createUpdateOrderRequest.OrderKey = vtexOrder.OrderId;
                    createUpdateOrderRequest.OrderNumber = vtexOrder.Sequence;
                }
                else
                {
                    createUpdateOrderRequest.OrderKey = vtexOrder.Sequence;
                    createUpdateOrderRequest.OrderNumber = vtexOrder.OrderId;
                }

                createUpdateOrderRequest.OrderStatus = await this.GetShipStationOrderStatus(vtexOrder.Status);
                createUpdateOrderRequest.PackageCode = null;
                if (vtexOrder.ReceiptData != null && vtexOrder.ReceiptData.ReceiptCollection != null)
                {
                    createUpdateOrderRequest.PaymentDate = vtexOrder.ReceiptData.ReceiptCollection.Select(r => r.Date).FirstOrDefault();
                }

                List<string> paymentMethods = new List<string>();
                // Need to find format for gift cards
                //paymentMethods.AddRange(vtexOrder.PaymentData.GiftCards.)
                paymentMethods.AddRange(vtexOrder.PaymentData.Transactions.SelectMany(t => t.Payments).Select(p => p.PaymentSystemName).Distinct().ToList());
                createUpdateOrderRequest.PaymentMethod = String.Join(", ", paymentMethods.ToArray());
                //if(vtexOrder.ShippingData != null && vtexOrder.ShippingData.LogisticsInfo != null)
                //{
                //    var slas = vtexOrder.ShippingData.LogisticsInfo.SelectMany(l => l.Slas);
                //    var deliveryIds = slas.SelectMany(s => s.DeliveryIds);
                //    var courierName = deliveryIds.Select(d => d.CourierName);
                //    if (courierName != null)
                //    {
                //        createUpdateOrderRequest.RequestedShippingService = courierName.FirstOrDefault();
                //        Console.WriteLine($"createUpdateOrderRequest.RequestedShippingService = '{createUpdateOrderRequest.RequestedShippingService}'");
                //    }
                //}
                //try
                //{
                //    createUpdateOrderRequest.RequestedShippingService = vtexOrder.ShippingData.LogisticsInfo.SelectMany(l => l.Slas).SelectMany(s => s.DeliveryIds).Select(d => d.CourierName).FirstOrDefault();
                //}
                //catch(Exception ex)
                //{
                //    Console.WriteLine($"Error setting RequestedShippingService: {ex.Message}");
                //}
                try
                {
                    createUpdateOrderRequest.RequestedShippingService = vtexOrder.ShippingData.LogisticsInfo.Select(l => l.SelectedSla).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting RequestedShippingService: {ex.Message}");
                }

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
                    Street2 = vtexOrder.ShippingData.Address.Complement,
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
                Dictionary<string, List<Option>> optionsToUpdate = new Dictionary<string, List<Option>>();
                List<string> marketplaceNames = new List<string>();
                long itemIndex = 0L;
                foreach (VtexOrderItem item in vtexOrder.Items)
                {
                    string marketplaceName = vtexOrder.Sellers.Where(i => i.Id.Equals(item.Seller)).Select(s => s.Name).FirstOrDefault();
                    if(!marketplaceNames.Contains(marketplaceName))
                    {
                        marketplaceNames.Add(marketplaceName);
                    };

                    Console.WriteLine($"    ----------  Marketplace '{marketplaceName}' --------------  ");
                    LogisticsInfo logisticsInfo = vtexOrder.ShippingData.LogisticsInfo.Where(l => l.ItemIndex.Equals(itemIndex)).FirstOrDefault();
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
                                    itemTax += (long)Math.Round(item.SellingPrice * priceTag.RawValue, MidpointRounding.AwayFromZero);
                                }
                                else
                                {
                                    itemTax += priceTag.Value;
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
                        orderItem.Name = CleanString(item.Name);
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

                        if (merchantSettings.SendItemDetails)
                        {
                            //orderItem.Options.Add(new Option { Name = "Brand Id", Value = CleanString(item.AdditionalInfo.BrandId) });
                            orderItem.Options.Add(new Option { Name = "Brand Name", Value = CleanString(item.AdditionalInfo.BrandName) });
                            //orderItem.Options.Add(new Option { Name = "Categories Ids", Value = CleanString(item.AdditionalInfo.CategoriesIds) });
                            if (item.AdditionalInfo.Categories != null)
                            {
                                foreach (Category category in item.AdditionalInfo.Categories)
                                {
                                    orderItem.Options.Add(new Option { Name = category.Id.ToString(), Value = CleanString(category.Name) });
                                }
                            }
                        }

                        foreach (ItemAssembly assembly in item.Assemblies)
                        {
                            Option option = new Option { Name = assembly.Id, Value = string.Empty };
                            orderItem.Options.Add(option);
                            orderItem.Options.AddRange(await this.FormatInputValues(assembly.InputValues));
                        }

                        if(item.ParentItemIndex != null)
                        {
                            //orderItem.Adjustment = true;
                            //Console.WriteLine($"item.ParentItemIndex={item.ParentItemIndex} createUpdateOrderRequest.Items.Count={createUpdateOrderRequest.Items.Count}");
                            string parentItemIndex = item.ParentItemIndex.ToString();
                            // Copy monogramming to parent item
                            foreach (ItemAssembly assembly in item.Assemblies)
                            {
                                Option option = new Option { Name = assembly.Id, Value = string.Empty };
                                List<Option> options = new List<Option> { option };
                                options.AddRange(await this.FormatInputValues(assembly.InputValues));

                                if (optionsToUpdate.Keys.Contains(parentItemIndex))
                                {
                                    if (optionsToUpdate[parentItemIndex] != null)
                                    {
                                        optionsToUpdate[parentItemIndex].AddRange(options);
                                    }
                                    else
                                    {
                                        optionsToUpdate[parentItemIndex] = new List<Option>();
                                        optionsToUpdate[parentItemIndex].AddRange(options);
                                    }
                                }
                                else
                                {
                                    optionsToUpdate.Add(parentItemIndex, new List<Option>());
                                    optionsToUpdate[parentItemIndex].AddRange(options);
                                }
                            }
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
                            if (deliveryIds != null)
                            {
                                warehouseId = deliveryIds.Select(w => w.WarehouseId).FirstOrDefault();
                                //Console.WriteLine($"--------------------------------->     Setting Warehouse Id to '{warehouseId}'");
                            }
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
                                splitItemsTotal[warehouseId].AmountPaid += ((orderItem.UnitPrice * orderItem.Quantity) + (orderItem.ShippingAmount ?? 0D) + (orderItem.TaxAmount ?? 0D) + ToDollar(sla.Tax));
                                //Console.WriteLine($"1:{warehouseId}: Amounts= {splitItemsTotal[warehouseId].ShippingAmount} {splitItemsTotal[warehouseId].TaxAmount} {splitItemsTotal[warehouseId].AmountPaid}");
                            }
                            else
                            {
                                splitItemsTotal[warehouseId] = new SplitItemsWrapper
                                {
                                    AmountPaid = ((orderItem.UnitPrice * orderItem.Quantity) + (orderItem.ShippingAmount ?? 0D) + (orderItem.TaxAmount ?? 0D) + ToDollar(sla.Tax)),
                                    ShippingAmount = orderItem.ShippingAmount ?? 0D,
                                    TaxAmount = orderItem.TaxAmount ?? 0D
                                };

                                //Console.WriteLine($"2:{warehouseId}: Amounts= {splitItemsTotal[warehouseId].ShippingAmount} {splitItemsTotal[warehouseId].TaxAmount} {splitItemsTotal[warehouseId].AmountPaid}");
                            }
                        }
                        else
                        {
                            //double itemPrice = orderItem.UnitPrice;
                            //long qnty = orderItem.Quantity;
                            //double itemTotal = itemPrice * qnty;
                            //Console.WriteLine($"    {itemPrice} X {qnty} = {itemTotal}     ");
                            //double shipping = orderItem.ShippingAmount ?? 0D;
                            //double tax = orderItem.TaxAmount ?? 0D;
                            //double shipTax = ToDollar(sla.Tax);
                            //Console.WriteLine($"    {shipping} {tax} {shipTax}      ");
                            //double amtPaid = itemTotal + shipping + tax + shipTax;
                            //Console.WriteLine($"    {itemTotal} + {shipping} + {tax} + {shipTax} = {amtPaid}     ");
                            SplitItemsWrapper itemsWrapper = new SplitItemsWrapper
                            {
                                AmountPaid = ((orderItem.UnitPrice * orderItem.Quantity) + (orderItem.ShippingAmount ?? 0D) + (orderItem.TaxAmount ?? 0D) + ToDollar(sla.Tax)),
                                ShippingAmount = orderItem.ShippingAmount ?? 0D,
                                TaxAmount = orderItem.TaxAmount ?? 0D
                            };

                            splitItemsTotal.Add(warehouseId, itemsWrapper);
                            //Console.WriteLine($"3:{warehouseId}: Amounts= {splitItemsTotal[warehouseId].ShippingAmount} {splitItemsTotal[warehouseId].TaxAmount} {splitItemsTotal[warehouseId].AmountPaid}");
                        }

                        createUpdateOrderRequest.Items.Add(orderItem);
                    }

                    itemIndex++;
                }

                string storeName = merchantSettings.StoreName;
                //Console.WriteLine($"    Store Name = {storeName}    ");
                if (!string.IsNullOrEmpty(storeName))
                {
                    long? storeId = await this.GetStoreIdByName(storeName);
                    if (storeId != null)
                    {
                        createUpdateOrderRequest.AdvancedOptions.StoreId = (long)storeId;
                        //Console.WriteLine($"    Storename '{storeName}' StoreId = {createUpdateOrderRequest.AdvancedOptions.StoreId}");
                    }
                }
                else if(marketplaceNames.Count == 1)
                {
                    long? storeId = await this.GetStoreIdByName(marketplaceNames[0]);
                    if (storeId != null)
                    {
                        createUpdateOrderRequest.AdvancedOptions.StoreId = (long)storeId;
                        //Console.WriteLine($"    Marketplace '{marketplaceNames[0]}' StoreId = {createUpdateOrderRequest.AdvancedOptions.StoreId}");
                    }
                }

                long orderTaxTotal = vtexOrder.Totals.Where(t => t.Id.Contains("Tax",StringComparison.InvariantCultureIgnoreCase)).Select(d => d.Value).FirstOrDefault();
                if(orderTaxTotal != totalTax)
                {
                    _context.Vtex.Logger.Info("CreateUpdateOrder", null, $"OrderKey={vtexOrder.OrderFormId} Order Tax={orderTaxTotal} Item Tax={totalTax}'");
                }

                createUpdateOrderRequest.AmountPaid = ToDollar(itemsTotal + totalTax + shippingTax + shippingTotal);
                createUpdateOrderRequest.ShippingAmount = ToDollar(shippingTotal);
                createUpdateOrderRequest.TaxAmount = ToDollar(totalTax);
                //createUpdateOrderRequest.TaxAmount = ToDollar(orderTaxTotal);

                foreach (string parentId in optionsToUpdate.Keys)
                {
                    createUpdateOrderRequest.Items[int.Parse(parentId)].Options.AddRange(optionsToUpdate[parentId]);
                    foreach (string warehouse in splitItems.Keys)
                    {
                        foreach (OrderItem warehouseItem in splitItems[warehouse])
                        {
                            if (warehouseItem.LineItemKey.Equals(parentId))
                            {
                                warehouseItem.Options = createUpdateOrderRequest.Items[int.Parse(parentId)].Options;
                            }
                        }
                    }
                }

                foreach(string warehouseId in splitItems.Keys)
                {
                    //Console.WriteLine($"--------------------------------->  Warehouse Id '{warehouseId}' {splitItems[warehouseId].Count} Items.");
                    ShipStationOrder shipStationOrderTemp = createUpdateOrderRequest;
                    if (!string.IsNullOrEmpty(warehouseId))
                    {
                        shipStationOrderTemp.OrderKey = shipStationOrderTemp.OrderKey + "|" + warehouseId;
                        shipStationOrderTemp.AmountPaid = splitItemsTotal[warehouseId].AmountPaid;
                        shipStationOrderTemp.ShippingAmount = splitItemsTotal[warehouseId].ShippingAmount;
                        shipStationOrderTemp.TaxAmount = splitItemsTotal[warehouseId].TaxAmount;

                        long shipStationWarehouseId = listWarehouses.Where(w => w.WarehouseName.Equals(warehouseId)).Select(w => w.WarehouseId).FirstOrDefault();
                        Console.WriteLine($"    === shipStationWarehouseId = {shipStationWarehouseId}   === ");
                        if (shipStationWarehouseId > 0)
                        {
                            shipStationOrderTemp.AdvancedOptions.WarehouseId = shipStationWarehouseId;
                        }
                    }

                    //shipStationOrderTemp.AdvancedOptions.WarehouseId = warehouseId;
                    shipStationOrderTemp.Items = splitItems[warehouseId];
                    if (shipStationOrderTemp.Items.Count > 0)
                    {
                        responseWrapper = await this.SendRequest(url, shipStationOrderTemp);
                        _context.Vtex.Logger.Info("CreateUpdateOrder", null, $"OrderKey={vtexOrder.OrderFormId} OrderNumber={vtexOrder.Sequence} '{vtexOrder.Status}'='{shipStationOrderTemp.OrderStatus}'");
                        Console.WriteLine($"CreateUpdateOrder '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
                        //Console.WriteLine($"CreateUpdateOrder '{responseWrapper.Message}' [{responseWrapper.IsSuccess}]");
                        Console.WriteLine($"CreateUpdateOrder [{responseWrapper.IsSuccess}]");

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

        public async Task<ListWebhooksResponse> ListWebHooks()
        {
            ListWebhooksResponse response = null;
            string url = $"https://{ShipStationConstants.API.HOST}/{ShipStationConstants.API.WEBHOOKS}";
            ResponseWrapper responseWrapper = await this.GetRequest(url);
            //Console.WriteLine($"ListWarehouses '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
            _context.Vtex.Logger.Info("ListWebHooks", null, JsonConvert.SerializeObject(responseWrapper));
            if (responseWrapper.IsSuccess)
            {
                response = JsonConvert.DeserializeObject<ListWebhooksResponse>(responseWrapper.ResponseText);
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
            //Console.WriteLine($"ListShipments '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
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
            //Console.WriteLine($"ListWarehouses '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
            _context.Vtex.Logger.Info("ListWarehouses", null, JsonConvert.SerializeObject(responseWrapper));
            if (responseWrapper.IsSuccess)
            {
                response = JsonConvert.DeserializeObject<List<ListWarehousesResponse>>(responseWrapper.ResponseText);
            }

            return response;
        }

        public async Task<List<ListStoresResponse>> ListStores()
        {
            List<ListStoresResponse> response = null;
            string url = $"https://{ShipStationConstants.API.HOST}/{ShipStationConstants.API.STORES}";
            ResponseWrapper responseWrapper = await this.GetRequest(url);
            Console.WriteLine($"ListStores '{responseWrapper.Message}' [{responseWrapper.IsSuccess}] {responseWrapper.ResponseText}");
            _context.Vtex.Logger.Info("ListStores", null, JsonConvert.SerializeObject(responseWrapper));
            if (responseWrapper.IsSuccess)
            {
                response = JsonConvert.DeserializeObject<List<ListStoresResponse>>(responseWrapper.ResponseText);
            }

            return response;
        }

        public async Task<long?> GetStoreIdByName(string storeName)
        {
            long? storeId = null;

            if(!string.IsNullOrEmpty(storeName))
            {
                //Console.WriteLine($"Merchant Test Store '{storeName}'");
                List<ListStoresResponse> listStoresResponses = await _shipStationRepository.GetShipStationStores();
                if(listStoresResponses != null)
                {
                    storeId = listStoresResponses.Where(s => s.StoreName.Equals(storeName)).Select(s => s.StoreId).FirstOrDefault();
                    //Console.WriteLine($"storeId 1 '{storeId}'");
                    if(storeId == 0)
                    {
                        storeId = null;
                    }
                }

                if(storeId == null)
                {
                    listStoresResponses = await this.ListStores();
                    if(listStoresResponses != null)
                    {
                        await _shipStationRepository.SaveShipStationStoreList(listStoresResponses);
                        storeId = listStoresResponses.Where(s => s.StoreName.Equals(storeName)).Select(s => s.StoreId).FirstOrDefault();
                        //Console.WriteLine($"storeId 2 '{storeId}'");
                    }
                }
            }

            return storeId;
        }
    }
}
