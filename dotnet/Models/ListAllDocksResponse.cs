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
        public DockPickupStoreInfo PickupStoreInfo { get; set; }
    }

    public partial class DockAddress
    {
        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("country")]
        public DockCountry Country { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("neighborhood")]
        public string Neighborhood { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("complement")]
        public string Complement { get; set; }

        [JsonProperty("reference")]
        public object Reference { get; set; }

        [JsonProperty("location")]
        public object Location { get; set; }
    }

    public partial class DockCountry
    {
        [JsonProperty("acronym")]
        public string Acronym { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class DeliveryFromStoreInfo
    {
        [JsonProperty("isActice")]
        public bool IsActice { get; set; }

        [JsonProperty("deliveryRadius")]
        public long DeliveryRadius { get; set; }

        [JsonProperty("deliveryFee")]
        public long DeliveryFee { get; set; }

        [JsonProperty("deliveryTime")]
        public DateTimeOffset DeliveryTime { get; set; }

        [JsonProperty("maximumWeight")]
        public long MaximumWeight { get; set; }
    }

    public class DockPickupInStoreInfo
    {
        [JsonProperty("isActice")]
        public bool IsActice { get; set; }

        [JsonProperty("additionalInfo")]
        public object AdditionalInfo { get; set; }
    }

    public class DockPickupStoreInfo
    {
        [JsonProperty("isPickupStore")]
        public bool IsPickupStore { get; set; }

        [JsonProperty("storeId")]
        public object StoreId { get; set; }

        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("address")]
        public DockAddress Address { get; set; }

        [JsonProperty("additionalInfo")]
        public object AdditionalInfo { get; set; }

        [JsonProperty("dockId")]
        public object DockId { get; set; }

        [JsonProperty("distance")]
        public object Distance { get; set; }

        [JsonProperty("businessHours")]
        public object BusinessHours { get; set; }

        [JsonProperty("pickupHolidays")]
        public object PickupHolidays { get; set; }

        [JsonProperty("sellerId")]
        public object SellerId { get; set; }

        [JsonProperty("isThirdPartyPickup")]
        public bool IsThirdPartyPickup { get; set; }
    }
}
