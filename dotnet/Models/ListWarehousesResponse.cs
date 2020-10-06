using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class ListWarehousesResponse
    {
        [JsonProperty("warehouseId")]
        public long WarehouseId { get; set; }

        [JsonProperty("warehouseName")]
        public string WarehouseName { get; set; }

        [JsonProperty("originAddress")]
        public NAddress OriginAddress { get; set; }

        [JsonProperty("returnAddress")]
        public NAddress ReturnAddress { get; set; }

        [JsonProperty("createDate")]
        public string CreateDate { get; set; }

        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; }
    }
}
