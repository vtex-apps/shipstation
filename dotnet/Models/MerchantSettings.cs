using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class MerchantSettings
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string StoreName { get; set; }
        //public string AppKey { get; set; }
        //public string AppToken { get; set; }
        public string WeightUnit { get; set; }
        public bool SplitShipmentByLocation { get; set; }
        public bool SendPickupInStore { get; set; }
        public bool MarketplaceOnly { get; set; }
        public string OrderSource { get; set; }
        public bool SendItemDetails { get; set; }
        public bool UpdateOrderStatus { get; set; }
        public bool UseSequenceAsOrderNumber { get; set; }
        public string BrandedReturnsUrl { get; set; }
        public bool UseRefIdAsSku { get; set; }
        public bool SendSkuDetails { get; set; }
        public bool AddDockToOptions { get; set; }
        public bool ShowPaymentMethod { get; set; }
    }
}
