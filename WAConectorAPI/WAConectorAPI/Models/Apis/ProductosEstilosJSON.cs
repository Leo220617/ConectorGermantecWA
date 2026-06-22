using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAConectorAPI.Models.Apis
{
    public class ProductosEstilosJSON
    {
        public string sku { get; set; }
        public int stock { get; set; }
        public decimal unit_price { get; set; }
        public string name { get; set; }
        public string brand { get; set; }
        public string category { get; set; }
        public string main_image { get; set; }
        public List<string> features { get; set; }
        public string gtin { get; set; }
        public string mpn { get; set; }
        public string cabys { get; set; }
        public List<string> images { get; set; }
    }
}