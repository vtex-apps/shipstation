using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ShipStation.Models
{
    public class OrderHook
    {
        [JsonProperty("filter")]
        public Filter Filter { get; set; }

        [JsonProperty("hook")]
        public Hook Hook { get; set; }
    }

    /// <summary>
    /// Status Filter Array
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// The events status to filter
        /// </summary>
        [JsonProperty("status")]
        public List<string> Status { get; set; }
    }

    /// <summary>
    /// Object of EndPoint infos
    /// </summary>
    public class Hook
    {
        [JsonProperty("headers")]
        public Headers Headers { get; set; }

        /// <summary>
        /// End Point URL
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    /// <summary>
    /// Credentials Array
    /// </summary>
    public class Headers
    {
        /// <summary>
        /// Endpoint key
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
