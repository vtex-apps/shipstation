namespace service.Controllers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using ShipStation.Data;
    using ShipStation.Models;
    using ShipStation.Services;
    using System;
    using System.Net.Http;
    using Vtex.Api.Context;

    public class EventsController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IShipStationAPIService _shipStationAPIService;
        private readonly IVtexAPIService _vtexAPIService;
        private readonly IShipStationRepository _shipStationRepository;

        public EventsController(IIOServiceContext context, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IShipStationAPIService shipStationAPIService, IVtexAPIService vtexAPIService, IShipStationRepository shipStationRepository)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this._clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            this._shipStationAPIService = shipStationAPIService ?? throw new ArgumentNullException(nameof(shipStationAPIService));
            this._vtexAPIService = vtexAPIService ?? throw new ArgumentNullException(nameof(vtexAPIService));
            this._shipStationRepository = shipStationRepository ?? throw new ArgumentNullException(nameof(shipStationRepository));
        }

        public string OnAppsLinked(string account, string workspace)
        {
            return $"OnAppsLinked event detected for {account}/{workspace}";
        }

        public void AllStates(string account, string workspace)
        {
            //Console.WriteLine($"AllStates event detected for {account}/{workspace}");
            string bodyAsText = new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync().Result;
            //Console.WriteLine($"[AllStates Notification] : '{bodyAsText}'");
            AllStatesNotification allStatesNotification = JsonConvert.DeserializeObject<AllStatesNotification>(bodyAsText);
            _context.Vtex.Logger.Debug("Order Broadcast", null, $"Notification {bodyAsText}");
            Console.WriteLine($"Notification: Order {allStatesNotification.OrderId} [{allStatesNotification.CurrentState}]");
            bool success = _vtexAPIService.ProcessNotification(allStatesNotification).Result;
            if(!success)
            {
                _context.Vtex.Logger.Info("Order Broadcast", null, $"Failed to Process Notification {bodyAsText}");
                throw new Exception("Failed to Process Notification");
            }
        }
    }
}
