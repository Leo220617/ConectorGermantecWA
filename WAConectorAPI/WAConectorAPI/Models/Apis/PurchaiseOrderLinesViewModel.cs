using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAConectorAPI.Models.Apis
{
    public class PurchaiseOrderLinesViewModel
    {
        public string CostingCode { get; set; } = "";
        public string CostingCode2 { get; set; } = "";
        public string CostingCode3 { get; set; } = "";
        public string CostingCode4 { get; set; } = "";
        public string CostingCode5 { get; set; } = "";

        public string Currency { get; set; }
        public decimal DiscountPercent { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public string TaxCode { get; set; }
        public string TaxOnly { get; set; }
        public decimal UnitPrice { get; set; }
        public string WarehouseCode { get; set; }

    }
}