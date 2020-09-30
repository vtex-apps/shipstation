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
            ActionResult status = BadRequest();
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                Console.WriteLine($"[Web Hook Notification] : '{bodyAsText}'");
                _context.Vtex.Logger.Info("WebHookNotification", hookEvent, bodyAsText);
                WebhookNotification webhookNotification = JsonConvert.DeserializeObject<WebhookNotification>(bodyAsText);
                string resource = await _shipStationAPIService.ProcessResourceUrl(webhookNotification.ResourceUrl);
                Console.WriteLine($"--> RESOURCE {resource} <--");
                _context.Vtex.Logger.Info("WebHookNotification", webhookNotification.ResourceUrl, resource);
                switch (hookEvent)
                {
                    case ShipStationConstants.WebhookEvent.ITEM_ORDER_NOTIFY:
                    //case ShipStationConstants.WebhookEvent.ORDER_NOTIFY:
                        ListOrdersResponse ordersResponse = JsonConvert.DeserializeObject<ListOrdersResponse>(resource);
                        break;
                    case ShipStationConstants.WebhookEvent.ITEM_SHIP_NOTIFY:
                    //case ShipStationConstants.WebhookEvent.SHIP_NOTIFY:
                        ListShipmentsResponse shipmentsResponse = JsonConvert.DeserializeObject<ListShipmentsResponse>(resource);
                        bool proccessed = await _vtexAPIService.ProcessShipNotification(shipmentsResponse);
                        break;
                }

                status = Ok();
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
            bool success = false;
            ActionResult status = BadRequest();
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                Console.WriteLine($"[Hook Notification] : '{bodyAsText}'");
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
                Console.WriteLine($"[Hook Notification] : '{HttpContext.Request.Method}'");
            }

            Console.WriteLine($"[Process Notification] : '{success}'");
            return status;
        }

        public async Task<IActionResult> SynchVtexOrder(string orderId)
        {
            VtexOrder vtexOrder = await _vtexAPIService.GetOrderInformation(orderId);
            bool success = await _shipStationAPIService.CreateUpdateOrder(vtexOrder);
            Response.Headers.Add("Cache-Control", "private");

            return Json(success);
        }

        public async Task<IActionResult> OrderInvoiceNotification(string orderId)
        {
            OrderInvoiceNotificationRequest request = new OrderInvoiceNotificationRequest
            {
                Courier = "UPS",
                InvoiceNumber = "inv-01",
                InvoiceValue = 3095,
                Items = new System.Collections.Generic.List<InvoiceItem> { new InvoiceItem { Id = "003", Price = 2050, Quantity = 1} },
                TrackingNumber = "1Z11111",
                Type = ShipStationConstants.InvoiceType.OUTPUT,
                IssuanceDate = DateTime.Now.ToString()
            };

            OrderInvoiceNotificationResponse response = await _vtexAPIService.OrderInvoiceNotification(orderId, request);
            Response.Headers.Add("Cache-Control", "private");

            return Json(response);
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
