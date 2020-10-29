using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class ListWebhooksResponse
    {
        [JsonProperty("webhooks")]
        public List<Webhook> Webhooks { get; set; }
    }

    public class Webhook
    {
        [JsonProperty("IsLabelAPIHook")]
        public bool IsLabelApiHook { get; set; }

        [JsonProperty("WebHookID")]
        public long? WebHookId { get; set; }

        [JsonProperty("SellerID")]
        public long? SellerId { get; set; }

        [JsonProperty("StoreID")]
        public long? StoreId { get; set; }

        [JsonProperty("HookType")]
        public string HookType { get; set; }

        [JsonProperty("MessageFormat")]
        public string MessageFormat { get; set; }

        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("BulkCopyBatchID")]
        public object BulkCopyBatchId { get; set; }

        [JsonProperty("BulkCopyRecordID")]
        public object BulkCopyRecordId { get; set; }

        [JsonProperty("Active")]
        public bool Active { get; set; }

        [JsonProperty("WebhookLogs")]
        public List<object> WebhookLogs { get; set; }

        [JsonProperty("Seller")]
        public object Seller { get; set; }

        [JsonProperty("Store")]
        public object Store { get; set; }
    }
}
