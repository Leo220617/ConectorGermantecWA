using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAConectorAPI.Models.Apis
{
    public class FiltroInventarios
    {
        public int top { get; set; }
        public int skip { get; set; }
        public string WhsCodeList { get; set; }
        public string ItemCode { get; set; }
        public string PriceListCode { get; set; }
        public DateTime Date { get; set; }
        public string Currency { get; set; }
    }
}