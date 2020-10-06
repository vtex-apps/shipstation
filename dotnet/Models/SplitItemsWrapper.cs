using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class SplitItemsWrapper
    {
        public double AmountPaid { get; set; }
        public double ShippingAmount { get; set; }
        public double TaxAmount { get; set; }
    }
}
