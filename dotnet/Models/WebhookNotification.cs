using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class WebhookNotification
    {
        /// <summary>
        /// Use this URL to get the resource that triggered the webhook. 200-character limit.
        /// Access the URL with ShipStation API Basic Authentication credentials.
        /// </summary>
        [JsonProperty("resource_url")]
        public string ResourceUrl { get; set; }

        /// <summary>
        /// The event type that triggered the webhook. Will be one of the following values:
        /// ORDER_NOTIFY, ITEM_ORDER_NOTIFY, SHIP_NOTIFY, ITEM_SHIP_NOTIFY
        /// </summary>
        [JsonProperty("resource_type")]
        public string ResourceType { get; set; }
    }
}
