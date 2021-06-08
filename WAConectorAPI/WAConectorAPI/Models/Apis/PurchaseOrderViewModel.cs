using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAConectorAPI.Models.Apis
{
    public class PurchaseOrderViewModel
    {
        public string CardCode { get; set; }
        public string DocCurrency { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public int DocNum { get; set; }
        public string DocType { get; set; }
        public string HandWritten { get; set; }
        public string NumAtCard { get; set; }
        public string ReserveInvoice { get; set; }
        public int Series { get; set; }
        public DateTime TaxDate { get; set; }
        public string U_SCGIE { get; set; }
        public string Comments { get; set; }
        public int SalesPersonCode { get; set; }
        public int DocumentOwner { get; set; }
        public decimal DocTotal { get; set; }
        public PurchaiseOrderLinesViewModel[] Lines { get; set; }
    }
}