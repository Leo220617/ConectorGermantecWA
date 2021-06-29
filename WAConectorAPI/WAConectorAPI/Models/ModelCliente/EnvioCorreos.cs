 
namespace WAConectorAPI.Models.ModelCliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EnvioCorreos")]
    public partial class EnvioCorreos
    {
        public int id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public bool UseSSL { get; set; }
        public int Port { get; set; }
    }
}