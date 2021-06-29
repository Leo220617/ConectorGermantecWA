 
namespace WAConectorAPI.Models.ModelCliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("InventariosBodegas")]
    public class InventariosBodegas
    {
        public int id { get; set; }
        public int IdVtex { get; set; }
        public string RefId { get; set; }
        public string Bodega { get; set; }
    }
}