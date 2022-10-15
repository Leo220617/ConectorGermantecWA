using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAConectorAPI.Models.Apis
{
    public class Unimart
    {
        public string u_public_key { get; set; }
        public string u_signature { get; set; }
        public int u_timestamp { get; set; }
        public List<u_products> u_products { get; set; }
    }
    public class u_products
    {
        public string sku { get; set; }
        public int quantity { get; set; }
        public string unit_cost { get; set; }
    }
}