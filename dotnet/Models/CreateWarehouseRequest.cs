using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class CreateWarehouseRequest
    {
        [JsonProperty("warehouseName")]
        public string WarehouseName { get; set; }

        [JsonProperty("originAddress")]
        public NAddress OriginAddress { get; set; }

        [JsonProperty("returnAddress")]
        public NAddress ReturnAddress { get; set; }

        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; }
    }
}
