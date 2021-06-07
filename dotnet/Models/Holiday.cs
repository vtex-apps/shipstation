using Newtonsoft.Json;
using System;

namespace ShipStation.Models
{
    public class Holiday
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}