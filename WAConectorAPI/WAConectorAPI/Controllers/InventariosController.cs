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
    public class InventariosController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();
        Metodos metodo = new Metodos();
        public HttpResponseMessage Get([FromUri] FiltroInventarios filtro)
        {
            try
            {
                string sql = " select ";
                if (filtro != null && filtro.top > 0)
                {
                    sql += " top " + filtro.top + " ";
                }
                sql += " t0.WhsCode, t0.ItemCode, t3.U_REFCOD, t0.OnHand - t0.IsCommited InStock from OITW t0  ";
                sql += " inner join OWHS t1 on t0.WhsCode = t1.WhsCode ";
                sql += " inner join OITM t3 on t0.ItemCode = t3.ItemCode where t0.WhsCode = T3.U_Bod_VT  "; // Este where nos trae solo los que tienen una bodega asignada

                if (filtro != null)
                {
                    //if(!string.IsNullOrEmpty(filtro.ItemCode) || !string.IsNullOrEmpty(filtro.WhsCodeList))
                    //{

                    //    sql += " where ";
                    //}


                    if (!string.IsNullOrEmpty(filtro.ItemCode))
                    {
                        sql += " and t0.ItemCode = '" + filtro.ItemCode + "' "/* + (!string.IsNullOrEmpty(filtro.WhsCodeList) ? " and ": "")*/;

                    }


                    //if (!string.IsNullOrEmpty(filtro.WhsCodeList))
                    //{
                    //    sql += " t0.WhsCode = '" + filtro.WhsCodeList+ "' ";
                    //}


                }

                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());


                SqlCommand Cmd = new SqlCommand(sql, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);

                DataSet Ds = new DataSet();

                Cn.Open();

                Da.Fill(Ds, "Productos");

                Cn.Close();

                return Request.CreateResponse(HttpStatusCode.OK, Ds);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /////////////////////////////////PriceList///////////////////////////////////
        [Route("api/Inventarios/PriceList")]
        public HttpResponseMessage GetPriceList([FromUri] FiltroInventarios filtro)
        {
            try
            {
                string sql = " select ";
                if (filtro != null && filtro.top > 0)
                {
                    sql += " top " + filtro.top + " ";
                }
                sql += " t0.Currency, t0.ItemCode, t1.AvgPrice ItemCost, t0.Price, t0.PriceList PriceListId   from ITM1 t0  ";
                sql += " inner join OITM t1 on t0.ItemCode = t1.ItemCode where t1.U_Bod_VT is not null ";


                if (filtro != null)
                {
                    //if (!string.IsNullOrEmpty(filtro.PriceListCode))
                    //{

                    //    sql += " where ";
                    //}


                    if (!string.IsNullOrEmpty(filtro.PriceListCode))
                    {
                        sql += " and t0.PriceList = '" + filtro.PriceListCode + "' ";

                    }




                }

                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());


                SqlCommand Cmd = new SqlCommand(sql, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);

                DataSet Ds = new DataSet();

                Cn.Open();

                Da.Fill(Ds, "PriceList");

                Cn.Close();

                return Request.CreateResponse(HttpStatusCode.OK, Ds);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        /////////////////////////////////INSERTAR EN INVENTARIO////////////////////////////////////////
        ///

        [Route("api/Inventarios/Insert")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetPriceMediumTableAsync()
        {
            try
            {
                DateTime time = DateTime.Now;
                Parametros param = db.Parametros.FirstOrDefault();
                //var SQL = " select t0.ItemCode, t1.ItemName, t0.WhsCode, t4.ItmsGrpNam, t0.OnHand, t0.IsCommited, t0.OnHand - t0.IsCommited Stock, t2.Price, t2.Currency, t3.Rate from oitw t0 inner join oitm t1 on t0.ItemCode = t1.ItemCode and t0.WhsCode = t1.U_Bod_VT ";
                //SQL += " inner join itm1 t2 on t0.ItemCode = t2.ItemCode and t2.PriceList = '7' inner join oitb t4 on t4.ItmsGrpCod = t1.ItmsGrpCod ";
                //SQL += " left join Ortt t3 on t2.Currency = t3.Currency and t3.RateDate = '" + time.Year + (time.Month < 10 ? "0" + time.Month.ToString() : time.Month.ToString()) + (time.Day < 10 ? "0" + time.Day.ToString() : time.Day.ToString()) + "' ";

                var conexion = g.DevuelveCadena();
                var SQL = param.SQLInventario + "'" + time.Year + (time.Month < 10 ? "0" + time.Month.ToString() : time.Month.ToString()) + (time.Day < 10 ? "0" + time.Day.ToString() : time.Day.ToString()) + "'";
                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Inventario");
                foreach (DataRow item in Ds.Tables["Inventario"].Rows)
                {

                    string itemCode = item["ItemCode"].ToString();
                    Inventario inventario = db.Inventario.Where(a => a.ItemCode == itemCode).FirstOrDefault();
                    try
                    {
                        if (inventario == null)
                        {
                            inventario = new Inventario();
                            inventario.ItemCode = item["ItemCode"].ToString();
                            inventario.ItemName = item["ItemName"].ToString();
                            inventario.WhsCode = item["WhsCode"].ToString();

                            //Aca nosotros encontramos cual es el skuid en vtex 
                            HttpClient cliente2 = new HttpClient();
                            cliente2.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", param.APP_KEY);
                            cliente2.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", param.APP_TOKEN);


                            string path2 = param.urlTomarSKU + inventario.ItemCode;
                            HttpResponseMessage response2 = await cliente2.GetAsync(path2);

                            string detalle = "";
                            if (response2.IsSuccessStatusCode)
                            {
                                detalle = await response2.Content.ReadAsAsync<string>();

                            }


                            inventario.skuid = detalle;
                            //Aca terminamos de encontrar el skuid


                            inventario.Familia = item["ItmsGrpNam"].ToString();
                            inventario.OnHand = Convert.ToDecimal(item["OnHand"].ToString());
                            inventario.IsCommited = Convert.ToDecimal(item["IsCommited"].ToString());
                            inventario.Stock = (Convert.ToDecimal(item["Stock"].ToString()) < 0 ? 0 : Convert.ToDecimal(item["Stock"].ToString()));
                            inventario.Precio = Convert.ToDecimal(item["Price"].ToString());
                            inventario.Currency = (item["Currency"].ToString() == "" ? "COL" : item["Currency"].ToString());
                            inventario.TipoCambio = ((item["Rate"].ToString() == "" && inventario.Currency == "COL") ? 1 : Convert.ToDecimal(item["Rate"].ToString()));
                            inventario.FechaActPrec = time.AddDays(-1);
                            inventario.FechaActualizacion = time.AddDays(-1);
                            inventario.Total = inventario.Precio * inventario.TipoCambio;

                            //Aca nosotros encontramos la informacion del producto
                            if (!string.IsNullOrEmpty(inventario.skuid))
                            {
                                try
                                {
                                    HttpClient cliente3 = new HttpClient();
                                    cliente3.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", param.APP_KEY);
                                    cliente3.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", param.APP_TOKEN);


                                    string path3 = param.urlInventarioInfo + inventario.skuid;
                                    HttpResponseMessage response3 = await cliente3.GetAsync(path3);

                                    infoArt detalle2 = new infoArt();
                                    if (response3.IsSuccessStatusCode)
                                    {
                                        detalle2 = await response3.Content.ReadAsAsync<infoArt>();

                                    }


                                    inventario.Descripcion = detalle2.ProductDescription;
                                    inventario.Marca = detalle2.BrandName;
                                    inventario.Imagen = detalle2.Images.FirstOrDefault().ImageUrl == null ? "" : detalle2.Images.FirstOrDefault().ImageUrl;
                                }
                                catch (Exception ex)
                                {

                                    BitacoraErrores error = new BitacoraErrores();
                                    error.Descripcion = ex.Message + " -> " + itemCode;
                                    error.StackTrace = "Insercion del inventario en la tabla media informacion " + ex.StackTrace.ToString();
                                    error.Fecha = DateTime.Now;
                                    db.BitacoraErrores.Add(error);
                                    db.SaveChanges();
                                }
                            }
                            //Aca terminamos de encontrar la informacion del producto
                            inventario.Ingo = false;
                            inventario.Unimart = false;
                            db.Inventario.Add(inventario);
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Entry(inventario).State = System.Data.Entity.EntityState.Modified;
                            //if (string.IsNullOrEmpty(inventario.skuid))
                            //{
                                //Aca nosotros encontramos cual es el skuid en vtex 
                                HttpClient cliente2 = new HttpClient();
                                cliente2.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", param.APP_KEY);
                                cliente2.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", param.APP_TOKEN);


                                string path2 = param.urlTomarSKU + inventario.ItemCode;
                                HttpResponseMessage response2 = await cliente2.GetAsync(path2);

                                string detalle = "";
                                if (response2.IsSuccessStatusCode)
                                {
                                    detalle = await response2.Content.ReadAsAsync<string>();

                                }


                                inventario.skuid = detalle;
                                //Aca terminamos de encontrar el skuid
                            //}

                            inventario.Familia = item["ItmsGrpNam"].ToString();
                            inventario.OnHand = Convert.ToDecimal(item["OnHand"].ToString());
                            inventario.IsCommited = Convert.ToDecimal(item["IsCommited"].ToString());
                            inventario.Stock = (Convert.ToDecimal(item["Stock"].ToString()) < 0 ? 0 : Convert.ToDecimal(item["Stock"].ToString()));
                            inventario.Precio = Convert.ToDecimal(item["Price"].ToString());
                            inventario.Currency = item["Currency"].ToString();
                            inventario.TipoCambio = ((item["Rate"].ToString() == "" && inventario.Currency == "COL") ? 1 : (item["Rate"].ToString() == "" && inventario.Currency != "COL") ? inventario.TipoCambio : Convert.ToDecimal(item["Rate"].ToString()));

                            inventario.Total = inventario.Precio * inventario.TipoCambio;


                            //Aca nosotros encontramos la informacion del producto
                            if (!string.IsNullOrEmpty(inventario.skuid) && string.IsNullOrEmpty(inventario.Imagen))
                            {
                                try
                                {
                                    HttpClient cliente3 = new HttpClient();
                                    cliente3.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", param.APP_KEY);
                                    cliente3.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", param.APP_TOKEN);


                                    string path3 = param.urlInventarioInfo + inventario.skuid;
                                    HttpResponseMessage response3 = await cliente3.GetAsync(path3);

                                    infoArt detalle2 = new infoArt();
                                    if (response3.IsSuccessStatusCode)
                                    {
                                        detalle2 = await response3.Content.ReadAsAsync<infoArt>();

                                    }


                                    inventario.Descripcion = detalle2.ProductDescription;
                                    inventario.Marca = detalle2.BrandName;
                                    inventario.Imagen = detalle2.Images.FirstOrDefault() == null ? "" : detalle2.Images.FirstOrDefault().ImageUrl == null ? "" : detalle2.Images.FirstOrDefault().ImageUrl;
                                }
                                catch (Exception ex)
                                {

                                    BitacoraErrores error = new BitacoraErrores();
                                    error.Descripcion = ex.Message + " -> " + itemCode;
                                    error.StackTrace = "Insercion del inventario en la tabla media informacion " + ex.StackTrace;
                                    error.Fecha = DateTime.Now;
                                    db.BitacoraErrores.Add(error);
                                    db.SaveChanges();
                                }
                            }
                            //Aca terminamos de encontrar la informacion del producto

                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {

                        BitacoraErrores error = new BitacoraErrores();
                        error.Descripcion = ex.Message + " -> " + itemCode;
                        error.StackTrace = "Insercion del inventario en la tabla media " + ex.StackTrace;
                        error.Fecha = DateTime.Now;
                        db.BitacoraErrores.Add(error);
                        db.SaveChanges();
                        metodo.EnviarCorreo("Insercion del inventario en la tabla media", error.Descripcion, error.StackTrace);
                    }


                }

                Cn.Close();


                //Unimart

                SQL = param.SQLInventarioUnimart;
                Cn = new SqlConnection(conexion);
                Cmd = new SqlCommand(SQL, Cn);
                Da = new SqlDataAdapter(Cmd);
                Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Inventario");
                foreach (DataRow item in Ds.Tables["Inventario"].Rows)
                {

                    string itemCode = item["ItemCode"].ToString();
                    InventarioUnimart inventario = db.InventarioUnimart.Where(a => a.ItemCode == itemCode).FirstOrDefault();
                    try
                    {
                        if (inventario == null)
                        {
                            inventario = new InventarioUnimart();
                            inventario.ItemCode = item["ItemCode"].ToString();
                            inventario.ItemName = item["ItemName"].ToString();
                            inventario.WhsCode = item["WhsCode"].ToString();




                            inventario.skuid = "";


                            inventario.Familia = item["ItmsGrpNam"].ToString();
                            inventario.OnHand = Convert.ToDecimal(item["OnHand"].ToString());
                            inventario.IsCommited = Convert.ToDecimal(item["IsCommited"].ToString());
                            inventario.Stock = (Convert.ToDecimal(item["Stock"].ToString()) < 0 ? 0 : Convert.ToDecimal(item["Stock"].ToString()));
                            inventario.Precio = Convert.ToDecimal(item["Price"].ToString());
                            inventario.Currency = (item["Currency"].ToString() == "" ? "COL" : item["Currency"].ToString());
                            inventario.TipoCambio = ((item["Rate"].ToString() == "" && inventario.Currency == "COL") ? 1 : Convert.ToDecimal(item["Rate"].ToString()));
                            inventario.FechaActPrec = time.AddDays(-1);
                            inventario.FechaActualizacion = time.AddDays(-1);
                            inventario.Total = inventario.Precio * inventario.TipoCambio;

                            //Aca nosotros encontramos la informacion del producto

                            try
                            {



                                inventario.Descripcion = "";
                                inventario.Marca = "";
                                inventario.Imagen = "";
                            }
                            catch (Exception ex)
                            {

                                BitacoraErrores error = new BitacoraErrores();
                                error.Descripcion = ex.Message + " -> " + itemCode;
                                error.StackTrace = "Insercion del inventario en la tabla media informacion " + ex.StackTrace.ToString();
                                error.Fecha = DateTime.Now;
                                db.BitacoraErrores.Add(error);
                                db.SaveChanges();
                            }

                            //Aca terminamos de encontrar la informacion del producto
                            inventario.Ingo = false;
                            inventario.Unimart = true;
                            db.InventarioUnimart.Add(inventario);
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Entry(inventario).State = System.Data.Entity.EntityState.Modified;



                            inventario.skuid = "";


                            inventario.Familia = item["ItmsGrpNam"].ToString();
                            inventario.OnHand = Convert.ToDecimal(item["OnHand"].ToString());
                            inventario.IsCommited = Convert.ToDecimal(item["IsCommited"].ToString());
                            inventario.Stock = (Convert.ToDecimal(item["Stock"].ToString()) < 0 ? 0 : Convert.ToDecimal(item["Stock"].ToString()));
                            inventario.Precio = Convert.ToDecimal(item["Price"].ToString());
                            inventario.Currency = item["Currency"].ToString();
                            inventario.TipoCambio = ((item["Rate"].ToString() == "" && inventario.Currency == "COL") ? 1 : (item["Rate"].ToString() == "" && inventario.Currency != "COL") ? inventario.TipoCambio : Convert.ToDecimal(item["Rate"].ToString()));

                            inventario.Total = inventario.Precio * inventario.TipoCambio;


                            //Aca nosotros encontramos la informacion del producto

                            try
                            {


                                inventario.Descripcion = "";
                                inventario.Marca = "";
                                inventario.Imagen = "";
                            }
                            catch (Exception ex)
                            {

                                BitacoraErrores error = new BitacoraErrores();
                                error.Descripcion = ex.Message + " -> " + itemCode;
                                error.StackTrace = "Insercion del inventario en la tabla media informacion " + ex.StackTrace;
                                error.Fecha = DateTime.Now;
                                db.BitacoraErrores.Add(error);
                                db.SaveChanges();
                            }

                            //Aca terminamos de encontrar la informacion del producto

                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {

                        BitacoraErrores error = new BitacoraErrores();
                        error.Descripcion = ex.Message + " -> " + itemCode;
                        error.StackTrace = "Insercion del inventario en la tabla media " + ex.StackTrace;
                        error.Fecha = DateTime.Now;
                        db.BitacoraErrores.Add(error);
                        db.SaveChanges();
                        metodo.EnviarCorreo("Insercion del inventario en la tabla media", error.Descripcion, error.StackTrace);
                    }


                }

                Cn.Close();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                BitacoraErrores error = new BitacoraErrores();
                error.Descripcion = ex.Message;
                error.StackTrace = "Insercion del inventario en la tabla media " +ex.Message;
                error.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(error);
                db.SaveChanges();
                metodo.EnviarCorreo("Insercion del inventario en la tabla media", error.Descripcion, error.StackTrace);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        /////////////////////////////////////////////////INSERTAR EN VTEX///////////////////////////////////
        ///

        [Route("api/Inventarios/InsertVtex")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GETINSERTVTEX()
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();

                Unimart unimart = new Unimart();
                unimart.u_public_key = param.UnimartKEY;
                unimart.u_timestamp = metodo.timeSpan();
                var Concatenado = param.UnimartKEY + param.UnimartSecret + unimart.u_timestamp;
                unimart.u_signature = metodo.SHA24Metodo(Concatenado);
                unimart.u_products = new List<u_products>();
                DateTime time = DateTime.Now;
                time = time.AddHours(-DateTime.Now.Hour);
                time = time.AddMinutes(-DateTime.Now.Minute);
                time = time.AddSeconds(-(DateTime.Now.Second - 1));
                var Inventario = db.Inventario.Where(a => a.skuid != null && a.skuid != "" && a.FechaActualizacion < time).Take(40).ToList();

                foreach (var item in Inventario)
                {
                    if (item.Stock >= 0)
                    {
                        HttpClient cliente = new HttpClient();
                        cliente.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", param.APP_KEY);
                        cliente.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", param.APP_TOKEN);

                        string path = param.urlInventarioActualizar.Replace("skuId", item.skuid).Replace("warehouseId", "1_1");

                        putInventory change = new putInventory();
                        change.unlimitedQuantity = false;
                        change.quantity = Convert.ToInt32(item.Stock);
                        change.dateUtcOnBalanceSystem = DateTime.Now.Date.ToString();

                        var httpContent = new StringContent(JsonConvert.SerializeObject(change), Encoding.UTF8, "application/json");

                        try
                        {
                            HttpResponseMessage response = await cliente.PutAsync(path, httpContent);


                            if (response.IsSuccessStatusCode)
                            {
                                //product = await response.Content.ReadAsAsync<ListaOrdenes>();
                                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                item.FechaActualizacion = DateTime.Now;
                                db.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            BitacoraErrores error = new BitacoraErrores();
                            error.Descripcion = ex.Message;
                            error.StackTrace = "Actualizacion en VTEX del articulo " + item.skuid;
                            error.Fecha = DateTime.Now;
                            db.BitacoraErrores.Add(error);
                            db.SaveChanges();
                            metodo.EnviarCorreo("Actualizacion en VTEX", error.Descripcion, error.StackTrace);
                        }


                    }



                   



                }


                //UNIMART INVENTARIO

                var InventarioUnimart = db.InventarioUnimart.Where(a => a.FechaActualizacion < time).Take(40).ToList();

                foreach(var item in InventarioUnimart)
                {
                    if (item.Stock >= 0 && item.Unimart)
                    {

                        u_products prod = new u_products();
                        prod.sku = item.ItemCode;
                        prod.quantity = Convert.ToInt32(item.Stock);
                        prod.unit_cost = Convert.ToInt32(item.Precio).ToString();

                        unimart.u_products.Add(prod);

                        db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        item.FechaActualizacion = DateTime.Now;
                        db.SaveChanges();
                    }
                }


                ///ap i unimart
                ///
                HttpClient cliente2 = new HttpClient();

                string path2 = param.UrlUnimart;
                var httpContent2 = new StringContent(JsonConvert.SerializeObject(unimart), Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response2 = await cliente2.PostAsync(path2, httpContent2);


                    if (response2.IsSuccessStatusCode)
                    {


                    }
                }
                catch (Exception ex)
                {
                    BitacoraErrores error = new BitacoraErrores();
                    error.Descripcion = ex.Message;
                    error.StackTrace = "Actualizacion en UNIMART del articulo ";
                    error.Fecha = DateTime.Now;
                    db.BitacoraErrores.Add(error);
                    db.SaveChanges();
                    metodo.EnviarCorreo("Actualizacion en UNIMART", error.Descripcion, error.StackTrace);
                }


                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                BitacoraErrores error = new BitacoraErrores();
                error.Descripcion = ex.Message;
                error.StackTrace = "Actualizacion en VTEX " + ex.StackTrace;
                error.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(error);
                db.SaveChanges();
                metodo.EnviarCorreo("Actualizacion en VTEX", error.Descripcion, error.StackTrace);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        /////////////////////////////////////////////////UPDATE EN VTEX PRICE///////////////////////////////////
        ///

        [Route("api/Inventarios/UpdatePriceVtex")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetVTexPrice()
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                DateTime time = DateTime.Now;
                time = time.AddHours(-DateTime.Now.Hour);
                time = time.AddMinutes(-DateTime.Now.Minute);
                time = time.AddSeconds(-(DateTime.Now.Second - 1));
                var Inventario = db.Inventario.Where(a => a.skuid != null && a.skuid != "" && a.FechaActPrec < time/* && a.id == 668*/ ).Take(40).ToList();

                foreach (var item in Inventario)
                {

                    HttpClient cliente = new HttpClient();
                    cliente.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", param.APP_KEY);
                    cliente.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", param.APP_TOKEN);

                    string path = param.urlActualizarPrecio + item.skuid;

                    putPrice change = new putPrice();
                    change.markup = 0;
                    change.basePrice = float.Parse(item.Total.ToString());
                    //decimal imp = Convert.ToDecimal(0.13);
                    //decimal impuesto = item.Total * imp;
                    //change.basePrice = float.Parse((item.Total + impuesto).ToString());


                    var httpContent = new StringContent(JsonConvert.SerializeObject(change), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await cliente.PutAsync(path, httpContent);


                    if (response.IsSuccessStatusCode)
                    {
                        //product = await response.Content.ReadAsAsync<ListaOrdenes>();
                        db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        item.FechaActPrec = DateTime.Now;
                        db.SaveChanges();
                    }
                    else
                    {
                        BitacoraErrores error = new BitacoraErrores();
                        error.Descripcion = response.ReasonPhrase;
                        error.StackTrace = "Actualizacion de precios en VTEX";
                        error.Fecha = DateTime.Now;
                        db.BitacoraErrores.Add(error);
                        db.SaveChanges();
                        metodo.EnviarCorreo("Actualizacion de precios en VTEX", error.Descripcion, error.StackTrace);
                    }



                }



                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                BitacoraErrores error = new BitacoraErrores();
                error.Descripcion = ex.Message;
                error.StackTrace = "Actualizacion de precios en VTEX";
                error.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(error);
                db.SaveChanges();
                metodo.EnviarCorreo("Actualizacion de precios en VTEX", error.Descripcion, error.StackTrace);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}