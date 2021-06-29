 
namespace WAConectorAPI.Models.ModelCliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Parametros")]
    public partial class Parametros
    {
        public int id { get; set; }
        public string APP_KEY { get; set; }
        public string APP_TOKEN { get; set; }
        public string urlOrdenesVTEX { get; set; }
        public string urlOrdenVTEX { get; set; }
        public string urlInventarioActualizar { get; set; }
        public string urlTomarSKU { get; set; }
        public string urlActualizarPrecio { get; set; }
    }
}