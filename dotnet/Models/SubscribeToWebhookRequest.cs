using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class SubscribeToWebhookRequest
    {
        /// <summary>
        /// The URL to send the webhooks to
        /// string, required
        /// </summary>
        [JsonProperty("target_url")]
        public string TargetUrl { get; set; }

        /// <summary>
        /// The type of webhook to subscribe to. Must contain one of the following values:
        /// ORDER_NOTIFY, ITEM_ORDER_NOTIFY, SHIP_NOTIFY, ITEM_SHIP_NOTIFY
        /// string, required
        /// </summary>
        [JsonProperty("event")]
        public string Event { get; set; }

        /// <summary>
        /// If passed in, the webhooks will only be triggered for this store_id
        /// int, optional
        /// </summary>
        [JsonProperty("store_id")]
        public int? StoreId { get; set; }

        /// <summary>
        /// Display name for the webhook
        /// string, optional
        /// </summary>
        [JsonProperty("friendly_name")]
        public string FriendlyName { get; set; }
    }
}
