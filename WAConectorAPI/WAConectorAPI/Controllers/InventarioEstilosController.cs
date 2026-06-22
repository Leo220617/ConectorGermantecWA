using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using WAConectorAPI.Models.Apis;
using WAConectorAPI.Models.ModelCliente;
using WAConectorAPI.Models.Vtex;

namespace WAConectorAPI.Controllers
{
    public class InventarioEstilosController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();
        Metodos metodo = new Metodos();

        [Route("api/InventarioEstilos/Insert")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetPriceMediumTableAsync()
        {
            try
            {
                DateTime time = DateTime.Now;
                Parametros param = db.Parametros.FirstOrDefault();

                var conexion = g.DevuelveCadena();
                var SQL = param.SQLInventarioEstilos;
                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Inventario");
                foreach (DataRow item in Ds.Tables["Inventario"].Rows)
                {

                    string itemCode = item["ItemCode"].ToString();
                    InventarioEstilos inventario = db.InventarioEstilos.Where(a => a.ItemCode == itemCode).FirstOrDefault();
                    try
                    {
                        if (inventario == null)
                        {
                            inventario = new InventarioEstilos();
                            inventario.ItemCode = item["ItemCode"].ToString();
                            inventario.ItemName = item["ItemName"].ToString();
                            inventario.WhsCode = item["WhsCode"].ToString();

                            //Aca nosotros encontramos cual es el skuid en vtex 
                            Inventario inv = db.Inventario.Where(a => a.ItemCode == inventario.ItemCode).FirstOrDefault();


                            inventario.skuid = inv != null ? inv.skuid : "";
                            //Aca terminamos de encontrar el skuid

                            if(!string.IsNullOrEmpty(inventario.skuid))
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
                                    var cont = 1;
                                    var cantidad = detalle2.Images.Count();
                                    foreach (var img in detalle2.Images)
                                    {
                                        inventario.Imagenes += img.ImageUrl + (cont < cantidad ? ";" : "");
                                        cont++;
                                    }

                                    cont = 1;
                                    cantidad = detalle2.ProductSpecifications.Count();
                                    foreach (var carac in detalle2.ProductSpecifications)
                                    {
                                        inventario.Caracteristicas += carac.FieldName + ": " + (carac.FieldValues == null ? "" : carac.FieldValues.FirstOrDefault()) + (cont < cantidad ? ";" : "");
                                    }
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



                            inventario.Descripcion = inv != null ? inv.Descripcion : "";
                            inventario.Marca = inv != null ? inv.Marca : "";
                            inventario.Imagen = inv != null ? inv.Imagen : "";

                            //Aca terminamos de encontrar la informacion del producto
                            inventario.Ingo = false;
                            inventario.Unimart = false;
                            inventario.Cabys = item["Cabys"].ToString();
                            db.InventarioEstilos.Add(inventario);
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Entry(inventario).State = System.Data.Entity.EntityState.Modified;


                            inventario.ItemName = item["ItemName"].ToString();
                            inventario.WhsCode = item["WhsCode"].ToString();

                            Inventario inv = db.Inventario.Where(a => a.ItemCode == inventario.ItemCode).FirstOrDefault();


                            inventario.skuid = inv != null ? inv.skuid : "";


                            inventario.Familia = item["ItmsGrpNam"].ToString();
                            inventario.OnHand = Convert.ToDecimal(item["OnHand"].ToString());
                            inventario.IsCommited = Convert.ToDecimal(item["IsCommited"].ToString());
                            inventario.Stock = (Convert.ToDecimal(item["Stock"].ToString()) < 0 ? 0 : Convert.ToDecimal(item["Stock"].ToString()));
                            inventario.Precio = Convert.ToDecimal(item["Price"].ToString());
                            inventario.Currency = item["Currency"].ToString();
                            inventario.TipoCambio = ((item["Rate"].ToString() == "" && inventario.Currency == "COL") ? 1 : (item["Rate"].ToString() == "" && inventario.Currency != "COL") ? inventario.TipoCambio : Convert.ToDecimal(item["Rate"].ToString()));

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

                                    var cont = 1;
                                    var cantidad = detalle2.Images.Count();
                                    inventario.Imagenes = "";
                                    foreach (var img in detalle2.Images)
                                    {
                                        inventario.Imagenes += img.ImageUrl + ( cont < cantidad ? ";" : "");
                                        cont++;
                                    }

                                    cont = 1;
                                    cantidad = detalle2.ProductSpecifications.Count();
                                    inventario.Caracteristicas = "";
                                    foreach (var carac in detalle2.ProductSpecifications)
                                    {
                                        inventario.Caracteristicas += carac.FieldName + ": " + (carac.FieldValues == null ? "" : carac.FieldValues.FirstOrDefault()) + (cont < cantidad ? ";" : "");
                                    }
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


                            inventario.Descripcion = inv != null ? inv.Descripcion : inventario.Descripcion;
                            inventario.Marca = inv != null ? inv.Marca : inventario.Marca;
                            inventario.Imagen = inv != null ? inv.Imagen : "";
                            inventario.Cabys = item["Cabys"].ToString();

                            //Aca terminamos de encontrar la informacion del producto

                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {

                        BitacoraErrores error = new BitacoraErrores();
                        error.Descripcion = ex.Message + " -> " + itemCode;
                        error.StackTrace = "Insercion del inventario en la tabla media estilos " + ex.StackTrace;
                        error.Fecha = DateTime.Now;
                        db.BitacoraErrores.Add(error);
                        db.SaveChanges();
                        metodo.EnviarCorreo("Insercion del inventario en la tabla media estilos", error.Descripcion, error.StackTrace);
                    }


                }

                Cn.Close();




                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                BitacoraErrores error = new BitacoraErrores();
                error.Descripcion = ex.Message;
                error.StackTrace = "Insercion del inventario en la tabla media estilos" + ex.Message;
                error.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(error);
                db.SaveChanges();
                metodo.EnviarCorreo("Insercion del inventario en la tabla media estilos", error.Descripcion, error.StackTrace);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        //////////////////////////OBTENER TOKEN/////////////////////////////////////
        public async System.Threading.Tasks.Task<string> ObtenerTokenAsync()
        {
            try
            {
                var Email = db.Parametros.FirstOrDefault().UsuarioEstilos;
                var Password = db.Parametros.FirstOrDefault().ClaveEstilos;

                var loginRequest = new LoginRequest
                {
                    email = Email,
                    password = Password
                };

                var json = JsonConvert.SerializeObject(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpClient cliente = new HttpClient();
                var urlLogin = db.Parametros.FirstOrDefault().urlLoginEstilos;

                using (var response = await cliente.PostAsync(urlLogin, content))
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error al obtener token. Status: {response.StatusCode}. Respuesta: {responseBody}");
                    }

                    var tokenResponse = JsonConvert.DeserializeObject<LoginResponse>(responseBody);

                    if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.access_token))
                    {
                        throw new Exception("No se recibió un access_token válido.");
                    }

                    return tokenResponse.access_token;
                }
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
                return "";

            }

        }


        /////////////////////////////////////////////////INSERTAR EN VTEX///////////////////////////////////
        ///

        [Route("api/InventarioEstilos/Insertestilos")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GETINSERTESTILOS()
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();

                DateTime time = DateTime.Now;
                time = time.AddHours(-DateTime.Now.Hour);
                time = time.AddMinutes(-DateTime.Now.Minute);
                time = time.AddSeconds(-(DateTime.Now.Second - 1));
                var Inventario = db.InventarioEstilos.Where(a => a.FechaActualizacion < time).Take(40).ToList();

                List<ProductosEstilosJSON> listado = new List<ProductosEstilosJSON>();


                foreach (var item in Inventario)
                {

                    ProductosEstilosJSON prod = new ProductosEstilosJSON();
                    prod.sku = item.ItemCode;
                    prod.stock = Convert.ToInt32(item.Stock);
                    prod.unit_price = item.Total;
                    prod.name = item.ItemName;
                    prod.brand = item.Marca;
                    prod.category = item.Familia;
                    prod.main_image = item.Imagen;
                    prod.features = new List<string>();
                    prod.cabys = item.Cabys;
                    prod.gtin = "";
                    prod.mpn = item.ItemCode;
                    prod.images = new List<string>();

                    foreach(var img in item.Imagenes.Split(';'))
                    {
                        if(img != "")
                        {
                            prod.images.Add(img);

                        }
                    }

                    foreach (var carac in item.Caracteristicas.Split(';'))
                    {
                        prod.features.Add(carac);
                    }
                    listado.Add(prod);

                    db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    item.FechaActualizacion = DateTime.Now;
                    db.SaveChanges();


                }

                if (listado == null || !listado.Any())
                    throw new Exception("La lista de productos está vacía.");

                string token = await ObtenerTokenAsync();

                var request = new SendProductRequest
                {
                    data = listado
                };

                var json = JsonConvert.SerializeObject(request);
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, db.Parametros.FirstOrDefault().urlInventarioEstilos);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpClient cliente = new HttpClient();

                using (var response = await cliente.SendAsync(httpRequest))
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error al enviar productos. Status: {response.StatusCode}. Respuesta: {responseBody}");
                    }

                    var result = JsonConvert.DeserializeObject<SendProductResponse>(responseBody);

                    if (result == null)
                    {
                        throw new Exception("La respuesta del API vino vacía o inválida.");
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, result);

                }





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

    }
    public class SendProductRequest
    {
        public List<ProductosEstilosJSON> data { get; set; }
    }

    internal class LoginResponse
    {
        public string access_token { get; set; }
    }

    internal class LoginRequest
    {
        public object email { get; set; }
        public object password { get; set; }
    }

    public class SendProductResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }
}