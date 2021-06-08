

namespace WAConectorAPI.Models.ModelCliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DetOrdenes")]
    public partial class DetOrdenes
    {
        public int id { get; set; }
        public string orderid { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuestos { get; set; }
        public decimal SubTotal { get; set; }
 
        public decimal Total { get; set; }
        public int TaxCode { get; set; }
        public string itemid { get; set; }
        public string itemCode { get; set; }
        public int quantity { get; set; }
        public decimal unitPrice { get; set; }
        public string WarehouseCode { get; set; }
    }
}