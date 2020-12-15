using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class ChangeOrderRequest
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("discountValue")]
        public int? DiscountValue { get; set; }

        [JsonProperty("incrementValue")]
        public int? IncrementValue { get; set; }

        [JsonProperty("itemsRemoved")]
        public List<ChangeItem> ItemsRemoved { get; set; }

        [JsonProperty("itemsAdded")]
        public List<ChangeItem> ItemsAdded { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }
    }

    public class ChangeItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("price")]
        public long Price { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }
    }
}
