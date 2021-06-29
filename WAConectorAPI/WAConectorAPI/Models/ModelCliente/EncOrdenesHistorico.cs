
namespace WAConectorAPI.Models.ModelCliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EncOrdenesHistorico")]
    public partial class EncOrdenesHistorico
    {
        public int id { get; set; }
        public string orderid { get; set; }
        public DateTime creationDate { get; set; }
        public string idVtex { get; set; }
        public string Cedula { get; set; }
        public string clientName { get; set; }
        public string telefono { get; set; }
        public string Correo { get; set; }
        public string currencyCode { get; set; }
        public int totalItems { get; set; }
        public string Comments { get; set; }
        public bool ProcesadaSAP { get; set; }
        public decimal Envio { get; set; }
        public decimal Total { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Descuento { get; set; }
        public string CreditCardNumber { get; set; }
        public string VoucherNum { get; set; }
    }
}