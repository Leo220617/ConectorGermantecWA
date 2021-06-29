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
    public class BodegasSKUController: ApiController
    {
        ModelCliente db = new ModelCliente();
        public class getBodegas
        {
            public int Id { get; set; }
            public string ProductRefId { get; set; }
            public List<ProductSpecifications> ProductSpecifications { get; set; }
            public AlternateIds AlternateIds { get; set; }
        }

        public class ProductSpecifications
        {
            public string FieldName { get; set; }
            public List<object> FieldValues { get; set; }
        }

        public class AlternateIds
        {
            public string RefId { get; set; }
        }

        public class FieldValues
        {
            public object campo { get; set; }
        }

        public async System.Threading.Tasks.Task<HttpResponseMessage> GetAsync()
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                HttpClient cliente2 = new HttpClient();
                cliente2.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", param.APP_KEY);
                cliente2.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", param.APP_TOKEN);

                 
                string path2 = "https://germantecmex.vtexcommercestable.com.br/api/catalog_system/pvt/sku/stockkeepingunitids?page=1&pagesize=1000";
                HttpResponseMessage response2 = await cliente2.GetAsync(path2);

                List<int> detalle = new List<int>();
                if (response2.IsSuccessStatusCode)
                {
                    detalle = await response2.Content.ReadAsAsync<List<int> >();

                }

                try
                {
                    await db.Database.ExecuteSqlCommandAsync("delete from InventariosBodegas where Bodega = 'N/A' or RefId is null or RefId = ''");

                }
                catch (Exception)
                {

                    
                }

                foreach(var item in detalle)
                {
                    getBodegas bodega = new getBodegas();
                    HttpClient cliente = new HttpClient();
                    cliente.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", param.APP_KEY);
                    cliente.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", param.APP_TOKEN);

                    string path = "https://germantecmex.vtexcommercestable.com.br/api/catalog_system/pvt/sku/stockkeepingunitbyid/"+ item;

                    var inventario = db.InventariosBodegas.Where(a => a.IdVtex == item).FirstOrDefault();
                    if (inventario == null)
                    {
                        HttpResponseMessage response = await cliente.GetAsync(path);


                        if (response.IsSuccessStatusCode)
                        {
                            bodega = await response.Content.ReadAsAsync<getBodegas>();
                            bodega.ProductSpecifications = bodega.ProductSpecifications.Where(a => a.FieldName == "Bodega").ToList();


                            InventariosBodegas inv = new InventariosBodegas();
                            inv.IdVtex = bodega.Id;
                            inv.RefId = bodega.AlternateIds.RefId;//bodega.ProductRefId;
                            inv.Bodega = (bodega.ProductSpecifications.Count() == 0 ? "N/A" : bodega.ProductSpecifications.FirstOrDefault().FieldValues.FirstOrDefault().ToString());
                            db.InventariosBodegas.Add(inv);
                            db.SaveChanges();





                        }
                    }
                }



                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch ( Exception ex )
            {

                throw ex;
            }
        }
    }
}