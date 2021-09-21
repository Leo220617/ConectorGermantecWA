using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAConectorAPI.Models.Apis
{
    public class DevolucionIngo
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Marca { get; set; }
        public string Imagen { get; set; }
        public string Familia { get; set; }
        public int Precio { get; set; }
        public int Stock { get; set; }
    }
}