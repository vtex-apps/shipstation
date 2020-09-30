using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class ListAllDocksResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("priority")]
        public long Priority { get; set; }

        [JsonProperty("dockTimeFake")]
        public string DockTimeFake { get; set; }

        [JsonProperty("timeFakeOverhead")]
        public string TimeFakeOverhead { get; set; }

        [JsonProperty("salesChannels")]
        public List<string> SalesChannels { get; set; }

        [JsonProperty("salesChannel")]
        public object SalesChannel { get; set; }

        [JsonProperty("freightTableIds")]
        public List<string> FreightTableIds { get; set; }

        [JsonProperty("wmsEndPoint")]
        public string WmsEndPoint { get; set; }

        [JsonProperty("pickupStoreInfo")]
        public PickupStoreInfo PickupStoreInfo { get; set; }
    }
}
