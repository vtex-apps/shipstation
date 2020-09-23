using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class OrderInvoiceNotificationRequest
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; }

        [JsonProperty("courier")]
        public string Courier { get; set; }

        [JsonProperty("trackingNumber")]
        public string TrackingNumber { get; set; }

        [JsonProperty("trackingUrl")]
        public string TrackingUrl { get; set; }

        [JsonProperty("items")]
        public List<InvoiceItem> Items { get; set; }

        [JsonProperty("issuanceDate")]
        public string IssuanceDate { get; set; }

        [JsonProperty("invoiceValue")]
        public double InvoiceValue { get; set; }
    }

    public class InvoiceItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }
    }
}
