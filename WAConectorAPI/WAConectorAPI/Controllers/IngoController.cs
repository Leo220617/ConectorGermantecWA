using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using WAConectorAPI.Models.Apis;
using WAConectorAPI.Models.ModelCliente;
using WAConectorAPI.Models.Vtex;

namespace WAConectorAPI.Controllers
{
    public class IngoController: ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();
        Metodos metodo = new Metodos();

        public HttpResponseMessage Get([FromUri] FiltroIngo filtro)
        {
            try
            {
                if(filtro.apikey == db.Parametros.FirstOrDefault().IngoToken)
                {
                    List<DevolucionIngo> di = new List<DevolucionIngo>();


                    var Inventario = db.Inventario.Where(a => !string.IsNullOrEmpty(a.Descripcion) && !string.IsNullOrEmpty(a.Imagen) && !string.IsNullOrEmpty(a.Familia) && a.Ingo == true).ToList();

                    foreach (var item in Inventario)
                    {
                        DevolucionIngo dev = new DevolucionIngo();

                        dev.Codigo = item.ItemCode;
                        dev.Nombre = item.ItemName;
                        dev.Descripcion = item.Descripcion;
                        dev.Marca = item.Marca;
                        dev.Imagen = item.Imagen;
                        dev.Familia = item.Familia;
                        dev.Precio = Convert.ToInt32(Math.Round(item.Precio * item.TipoCambio));
                        dev.Stock = Convert.ToInt32(item.Stock);

                        di.Add(dev);

                    }

                    return Request.CreateResponse(HttpStatusCode.OK, di);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Token incorrecto");
                }
               
            }
            catch (Exception ex)
            {

                BitacoraErrores error = new BitacoraErrores();
                error.Descripcion = ex.Message;
                error.StackTrace = "Ingo GET";
                error.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(error);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/Ingo/Stock")]
        public HttpResponseMessage GetStock([FromUri] FiltroIngo filtro)
        {
            try
            {
                if (filtro.apikey == db.Parametros.FirstOrDefault().IngoToken)
                {
                    var Stock = db.Inventario.Where(a => a.ItemCode == filtro.CodPro).FirstOrDefault().Stock;

                    var resp = new 
                    {
                        Stock = Convert.ToInt32(Stock)
                    };                    
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Token incorrecto");
                }
            }
            catch (Exception ex)
            {
                BitacoraErrores error = new BitacoraErrores();
                error.Descripcion = ex.Message;
                error.StackTrace = "Ingo GET Stock";
                error.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(error);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }


    }

}