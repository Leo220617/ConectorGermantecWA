using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAConectorAPI.Models.Vtex
{
    public class putInventory
    {
        public bool unlimitedQuantity { get; set; }
        public string dateUtcOnBalanceSystem { get; set; }
        public int quantity { get; set; }
     
    }
}