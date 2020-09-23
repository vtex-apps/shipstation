using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class OrderInvoiceNotificationResponse
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("receipt")]
        public string Receipt { get; set; }
    }
}
