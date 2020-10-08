using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class ListStoresResponse
    {
        [JsonProperty("storeId")]
        public long StoreId { get; set; }

        [JsonProperty("storeName")]
        public string StoreName { get; set; }

        [JsonProperty("marketplaceId")]
        public long MarketplaceId { get; set; }

        [JsonProperty("marketplaceName")]
        public string MarketplaceName { get; set; }

        [JsonProperty("accountName")]
        public object AccountName { get; set; }

        [JsonProperty("email")]
        public object Email { get; set; }

        [JsonProperty("integrationUrl")]
        public string IntegrationUrl { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("publicEmail")]
        public string PublicEmail { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("refreshDate")]
        public string RefreshDate { get; set; }

        [JsonProperty("lastRefreshAttempt")]
        public string LastRefreshAttempt { get; set; }

        [JsonProperty("createDate")]
        public string CreateDate { get; set; }

        [JsonProperty("modifyDate")]
        public string ModifyDate { get; set; }

        [JsonProperty("autoRefresh")]
        public bool AutoRefresh { get; set; }
    }
}
