using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class ListFulfillmentsResponse
    {
        [JsonProperty("fulfillments")]
        public List<Fulfillment> Fulfillments { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("page")]
        public long Page { get; set; }

        [JsonProperty("pages")]
        public long Pages { get; set; }
    }

    public class Fulfillment
    {
        [JsonProperty("fulfillmentId")]
        public long FulfillmentId { get; set; }

        [JsonProperty("orderId")]
        public long OrderId { get; set; }

        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("customerEmail")]
        public string CustomerEmail { get; set; }

        [JsonProperty("trackingNumber")]
        public string TrackingNumber { get; set; }

        [JsonProperty("createDate")]
        public string CreateDate { get; set; }

        [JsonProperty("shipDate")]
        public string ShipDate { get; set; }

        [JsonProperty("voidDate")]
        public string VoidDate { get; set; }

        [JsonProperty("deliveryDate")]
        public string DeliveryDate { get; set; }

        [JsonProperty("carrierCode")]
        public string CarrierCode { get; set; }

        [JsonProperty("fulfillmentProviderCode")]
        public string FulfillmentProviderCode { get; set; }

        [JsonProperty("fulfillmentServiceCode")]
        public string FulfillmentServiceCode { get; set; }

        [JsonProperty("fulfillmentFee")]
        public double FulfillmentFee { get; set; }

        [JsonProperty("voidRequested")]
        public bool VoidRequested { get; set; }

        [JsonProperty("voided")]
        public bool Voided { get; set; }

        [JsonProperty("marketplaceNotified")]
        public bool MarketplaceNotified { get; set; }

        [JsonProperty("notifyErrorMessage")]
        public string NotifyErrorMessage { get; set; }

        [JsonProperty("shipTo")]
        public ShipTo ShipTo { get; set; }
    }   
}