namespace WAConectorAPI.Models.ModelCliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Login")]
    public partial class Login
    {
        public int id { get; set; }

        [StringLength(4)]
        public string CodCliente { get; set; }

        [StringLength(100)]
        public string NomCliente { get; set; }

        [StringLength(50)]
        public string Usuario { get; set; }

        [StringLength(100)]
        public string Password { get; set; }

        public bool? Activo { get; set; }

        [StringLength(50)]
        public string SAPUser { get; set; }

        [StringLength(100)]
        public string SAPPass { get; set; }

        [StringLength(50)]
        public string SQLUser { get; set; }

        [StringLength(100)]
        public string ServerSQL { get; set; }

        [StringLength(50)]
        public string ServerLicense { get; set; }

        [StringLength(100)]
        public string SQLPass { get; set; }

        [StringLength(50)]
        public string SQLType { get; set; }

        [StringLength(50)]
        public string SQLBD { get; set; }
    }
}
