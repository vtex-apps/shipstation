namespace ShipStation.Controllers
{
    using ShipStation.Data;
    using ShipStation.Models;
    using ShipStation.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Vtex.Api.Context;
    using System.Web;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class RoutesController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IShipStationAPIService _shipStationAPIService;
        private readonly IVtexAPIService _vtexAPIService;
        private readonly IShipStationRepository _shipStationRepository;

        public RoutesController(IIOServiceContext context, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IShipStationAPIService shipStationAPIService, IVtexAPIService vtexAPIService, IShipStationRepository shipStationRepository)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this._clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            this._shipStationAPIService = shipStationAPIService ?? throw new ArgumentNullException(nameof(shipStationAPIService));
            this._vtexAPIService = vtexAPIService ?? throw new ArgumentNullException(nameof(vtexAPIService));
            this._shipStationRepository = shipStationRepository ?? throw new ArgumentNullException(nameof(shipStationRepository));
        }

        public async Task<IActionResult> CreateHook()
        {
            Console.WriteLine("--> CreateHook <--");
            bool createOrUpdateHookResponse = await this._vtexAPIService.CreateOrUpdateHook();
            Response.Headers.Add("Cache-Control", "private");

            return Json(createOrUpdateHookResponse);
        }

        public async Task<IActionResult> CreateWebHook(string hookEvent)
        {
            Console.WriteLine($"--> CreateWebHook '{hookEvent}' <--");
            string response = await _shipStationAPIService.SubscribeToWebhook(hookEvent);
            Response.Headers.Add("Cache-Control", "private");

            return Json(response);
        }

        public async Task<IActionResult> WebHookNotification(string hookEvent)
        {
            Console.WriteLine($"--> WebHookNotification '{hookEvent}' <--");
            //return BadRequest();
            ActionResult status = BadRequest();
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                //Console.WriteLine($"[Web Hook Notification] : '{bodyAsText}'");
                _context.Vtex.Logger.Info("WebHookNotification", hookEvent, bodyAsText);
                WebhookNotification webhookNotification = JsonConvert.DeserializeObject<WebhookNotification>(bodyAsText);
                string resource = await _shipStationAPIService.ProcessResourceUrl(webhookNotification.ResourceUrl);
                //Console.WriteLine($"--> RESOURCE {resource} <--");
                _context.Vtex.Logger.Info("WebHookNotification", webhookNotification.ResourceUrl, resource);
                bool proccessed = false;
                switch (hookEvent)
                {
                    case ShipStationConstants.WebhookEvent.ITEM_ORDER_NOTIFY:
                    //case ShipStationConstants.WebhookEvent.ORDER_NOTIFY:
                        ListOrdersResponse ordersResponse = JsonConvert.DeserializeObject<ListOrdersResponse>(resource);
                        break;
                    case ShipStationConstants.WebhookEvent.ITEM_SHIP_NOTIFY:
                    //case ShipStationConstants.WebhookEvent.SHIP_NOTIFY:
                        ListShipmentsResponse shipmentsResponse = JsonConvert.DeserializeObject<ListShipmentsResponse>(resource);
                        proccessed = await _vtexAPIService.ProcessShipNotification(shipmentsResponse);
                        break;
                }

                if (proccessed)
                {
                    status = Ok();
                }
            }

            Response.Headers.Add("Cache-Control", "private");

            return Json(status);
        }

        public async Task<IActionResult> ProcessResourceUrl(string url)
        {
            url = HttpUtility.UrlDecode(url);
            Console.WriteLine($"--> ProcessResourceUrl '{url}' <--");
            string response = await _shipStationAPIService.ProcessResourceUrl(url);
            Response.Headers.Add("Cache-Control", "private");

            return Json(response);
        }

        public async Task<IActionResult> ProcessNotification()
        {
            return Ok();
            bool success = false;
            ActionResult status = BadRequest();
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                //Console.WriteLine($"[Hook Notification] : '{bodyAsText}'");
                dynamic notification = JsonConvert.DeserializeObject<dynamic>(bodyAsText);
                if (notification != null && notification.hookConfig != null && notification.hookConfig == ShipStationConstants.HOOK_PING)
                {
                    status = Ok();
                    success = true;
                }
                else
                {
                    HookNotification hookNotification = JsonConvert.DeserializeObject<HookNotification>(bodyAsText);
                    success = await _vtexAPIService.ProcessNotification(hookNotification);
                    status = success ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
                }

                _context.Vtex.Logger.Info("ProcessNotification", null, $"Success? [{success}] for {bodyAsText}");
            }
            else
            {
                //Console.WriteLine($"[Hook Notification] : '{HttpContext.Request.Method}'");
            }

            //Console.WriteLine($"[Process Notification] : '{success}'");
            return status;
        }

        public async Task<IActionResult> SynchVtexOrder(string orderId)
        {
            VtexOrder vtexOrder = await _vtexAPIService.GetOrderInformation(orderId);
            bool success = await _shipStationAPIService.CreateUpdateOrder(vtexOrder);
            string shipments = await _vtexAPIService.ValidateOrderShipments(orderId);
            Response.Headers.Add("Cache-Control", "private");

            return Json($"Sent to ShipStation? {success} Shipments: {shipments}");
        }

        public async Task<IActionResult> SetOrderStatus(string orderId, string orderStatus)
        {
            bool success = await _vtexAPIService.SetOrderStatus(orderId, orderStatus);
            Response.Headers.Add("Cache-Control", "private");

            return Json(success);
        }

        public async Task<IActionResult> ListAllDocks()
        {
            Response.Headers.Add("Cache-Control", "private");
            return Json(await _vtexAPIService.ListAllDocks());
        }

        public async Task<IActionResult> ListAllWarehouses()
        {
            Response.Headers.Add("Cache-Control", "private");
            return Json(await _vtexAPIService.ListAllWarehouses());
        }

        public async Task<IActionResult> ListWarehouses()
        {
            Response.Headers.Add("Cache-Control", "private");
            return Json(await _shipStationAPIService.ListWarehouses());
        }

        public async Task<IActionResult> ListStores()
        {
            Response.Headers.Add("Cache-Control", "private");
            return Json(await _shipStationAPIService.ListStores());
        }

        public async Task<IActionResult> ProcessOrder()
        {
            bool success = false;
            ActionResult status = BadRequest();
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();

                VtexOrder vtexOrder = JsonConvert.DeserializeObject<VtexOrder>(bodyAsText);
                success = await this._shipStationAPIService.CreateUpdateOrder(vtexOrder);

                status = success ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
            }

            return status;
        }

        public async Task<IActionResult> SetupHooks()
        {
            Response.Headers.Add("Cache-Control", "private");
            //bool success = await this._vtexAPIService.CreateOrUpdateHook();
            string response = await this._shipStationAPIService.SubscribeToWebhook(ShipStationConstants.WebhookEvent.ITEM_SHIP_NOTIFY);
            //return Json($"Vtex Order Hook? {success} - ShipStation Webhook: {response}");
            return Json($"ShipStation Webhook: {response}");
        }

        public async Task<IActionResult> ListShipments()
        {
            Response.Headers.Add("Cache-Control", "private");
            var response = await this._shipStationAPIService.ListShipments(string.Empty);
            //Console.WriteLine($"response.Total = {response.Total}");
            string responseMsg = JsonConvert.SerializeObject(response);
            return Json(responseMsg);
        }

        public async Task<IActionResult> ListOrders()
        {
            Response.Headers.Add("Cache-Control", "private");
            var response = await this._shipStationAPIService.ListOrders("pageSize=500&orderStatus=cancelled");
            return Json(response);
        }

        public async Task<IActionResult> ValidateShipments(string date)
        {
            Response.Headers.Add("Cache-Control", "private");
            DateTime dt = DateTime.Parse(date);
            date = dt.Date.ToString("d");
            var response = await this._vtexAPIService.ValidateShipments(date);
            return Json(response);
        }

        public async Task<IActionResult> ListWebhooks()
        {
            Response.Headers.Add("Cache-Control", "private");
            var response = await this._shipStationAPIService.ListWebHooks();
            return Json(response);
        }

        public async Task<IActionResult> ListActiveWebhooks()
        {
            Response.Headers.Add("Cache-Control", "private");
            ListWebhooksResponse listWebhooksResponse = await this._shipStationAPIService.ListWebHooks();
            var response = listWebhooksResponse.Webhooks.Where(w => w.Active).Select(w => w.Name).ToList();
            return Json(response);
        }

        public async Task<IActionResult> CheckCancelledOrders()
        {
            Response.Headers.Add("Cache-Control", "private");
            var response = await this._vtexAPIService.CheckCancelledOrders(DateTime.Now);
            return Json(response);
        }

        public async Task<IActionResult> CreateWarehouses()
        {
            Response.Headers.Add("Cache-Control", "private");
            StringBuilder sb = new StringBuilder();
            var vtexDocks = await _vtexAPIService.ListAllDocks();
            var shipStationWarehouses = await _shipStationAPIService.ListWarehouses();
            foreach (ListAllDocksResponse listAllDocksResponse in vtexDocks)
            {
                if (!shipStationWarehouses.Any(w => w.WarehouseName.Equals(listAllDocksResponse.Id)))
                {
                    try
                    {
                        CreateWarehouseRequest createWarehouseRequest = new CreateWarehouseRequest
                        {
                            OriginAddress = new NAddress
                            {
                                City = listAllDocksResponse.PickupStoreInfo.Address.City,
                                Company = null,
                                Country = listAllDocksResponse.PickupStoreInfo.Address.Country.Acronym.Substring(0, 2),
                                Name = listAllDocksResponse.PickupStoreInfo.FriendlyName ?? listAllDocksResponse.Name,
                                Phone = "555-555-5555",
                                PostalCode = listAllDocksResponse.PickupStoreInfo.Address.PostalCode,
                                Residential = null,
                                State = listAllDocksResponse.PickupStoreInfo.Address.State,
                                Street1 = listAllDocksResponse.PickupStoreInfo.Address.Street,
                                Street2 = listAllDocksResponse.PickupStoreInfo.Address.Complement,
                                Street3 = null
                            },
                            ReturnAddress = null,
                            IsDefault = false,
                            WarehouseName = listAllDocksResponse.Id
                        };

                        var createWarehouseResponse = await _shipStationAPIService.CreateWarehouse(createWarehouseRequest);
                        sb.AppendLine(JsonConvert.SerializeObject(createWarehouseResponse));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR: '{listAllDocksResponse.Name}' {ex.Message}");
                        _context.Vtex.Logger.Error("CreateWarehouses", null, $"Error creating '{listAllDocksResponse.Name}' warehouse.", ex);
                    }
                }
                else
                {
                    Console.WriteLine($"Skipping '{listAllDocksResponse.Name}' ");
                }
            }

            return Json(sb.ToString());
        }

        public string PrintHeaders()
        {
            string headers = "--->>> Headers <<<---\n";
            foreach (var header in HttpContext.Request.Headers)
            {
                headers += $"{header.Key}: {header.Value}\n";
            }
            return headers;
        }
    }
}
