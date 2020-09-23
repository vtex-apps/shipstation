using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class ListShipmentsResponse
    {
        [JsonProperty("shipments")]
        public List<Shipment> Shipments { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("page")]
        public long Page { get; set; }

        [JsonProperty("pages")]
        public long Pages { get; set; }
    }

    public class Shipment
    {
        [JsonProperty("shipmentId")]
        public long ShipmentId { get; set; }

        [JsonProperty("orderId")]
        public long OrderId { get; set; }

        [JsonProperty("orderKey", NullValueHandling = NullValueHandling.Ignore)]
        public string OrderKey { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("createDate")]
        public DateTimeOffset CreateDate { get; set; }

        [JsonProperty("shipDate")]
        public DateTimeOffset ShipDate { get; set; }

        [JsonProperty("shipmentCost")]
        public double ShipmentCost { get; set; }

        [JsonProperty("insuranceCost")]
        public long InsuranceCost { get; set; }

        [JsonProperty("trackingNumber")]
        public string TrackingNumber { get; set; }

        [JsonProperty("isReturnLabel")]
        public bool IsReturnLabel { get; set; }

        [JsonProperty("batchNumber")]
        public string BatchNumber { get; set; }

        [JsonProperty("carrierCode")]
        public string CarrierCode { get; set; }

        [JsonProperty("serviceCode")]
        public string ServiceCode { get; set; }

        [JsonProperty("packageCode")]
        public string PackageCode { get; set; }

        [JsonProperty("confirmation")]
        public string Confirmation { get; set; }

        [JsonProperty("warehouseId")]
        public long WarehouseId { get; set; }

        [JsonProperty("voided")]
        public bool Voided { get; set; }

        [JsonProperty("voidDate")]
        public object VoidDate { get; set; }

        [JsonProperty("marketplaceNotified")]
        public bool MarketplaceNotified { get; set; }

        [JsonProperty("notifyErrorMessage")]
        public object NotifyErrorMessage { get; set; }

        [JsonProperty("shipTo")]
        public ShipTo ShipTo { get; set; }

        [JsonProperty("weight")]
        public Weight Weight { get; set; }

        [JsonProperty("dimensions")]
        public object Dimensions { get; set; }

        [JsonProperty("insuranceOptions")]
        public InsuranceOptions InsuranceOptions { get; set; }

        [JsonProperty("advancedOptions")]
        public object AdvancedOptions { get; set; }

        [JsonProperty("shipmentItems")]
        public List<ShipmentItem> ShipmentItems { get; set; }

        [JsonProperty("labelData")]
        public object LabelData { get; set; }

        [JsonProperty("formData")]
        public object FormData { get; set; }
    }

    public class ShipTo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("street1")]
        public string Street1 { get; set; }

        [JsonProperty("street2")]
        public string Street2 { get; set; }

        [JsonProperty("street3")]
        public object Street3 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("residential")]
        public object Residential { get; set; }
    }

    public class ShipmentItem
    {
        [JsonProperty("orderItemId")]
        public long OrderItemId { get; set; }

        [JsonProperty("lineItemKey")]
        public object LineItemKey { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("imageUrl")]
        public object ImageUrl { get; set; }

        [JsonProperty("weight")]
        public object Weight { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("unitPrice")]
        public long UnitPrice { get; set; }

        [JsonProperty("warehouseLocation")]
        public object WarehouseLocation { get; set; }

        [JsonProperty("options")]
        public object Options { get; set; }

        [JsonProperty("productId")]
        public long ProductId { get; set; }

        [JsonProperty("fulfillmentSku")]
        public object FulfillmentSku { get; set; }
    }
}
