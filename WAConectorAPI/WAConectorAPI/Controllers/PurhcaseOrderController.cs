using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WAConectorAPI.Models.Apis;
using WAConectorAPI.Models.ModelCliente;
using WAConectorAPI.Models.Vtex;

namespace WAConectorAPI.Controllers
{
    public class PurhcaseOrderController: ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();
        public class Cliente
        {
            public string CardCode { get; set; }
        }

        public async System.Threading.Tasks.Task<HttpResponseMessage> GetAsync()
        {
            try
            {
                HttpClient cliente = new HttpClient();
                cliente.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", "vtexappkey-germantecmex-GXWMYU");
                cliente.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", "EETXLUZWDLEEUAGTTQFWABFAOFUESFJPPZSMCIDEJXLNPHRZXGWDAYXCYTJGZUEXBPOUHTQKNANCQSLGNVGLIFORCQNJZXEOTOLZSXSEQMQZLUMGICTEOSOWAXRRGHKQ");

                string path = "https://germantecmex.vtexcommercestable.com.br/api/oms/pvt/orders?f_creationDate=creationDate:[2021-05-26T02:00:00.000Z TO 2021-05-31T01:59:59.999Z]";

                HttpResponseMessage response = await cliente.GetAsync(path);

                ListaOrdenes product = new ListaOrdenes();
                if (response.IsSuccessStatusCode)
                {
                    product = await response.Content.ReadAsAsync<ListaOrdenes>();

                }
                if (product.list.Count() > 0)
                {

                    foreach (var item in product.list)
                    {
                        var registro = db.EncOrdenes.Where(a => a.orderid == item.orderId).FirstOrDefault();
                        var registroH = db.EncOrdenesHistorico.Where(a => a.orderid == item.orderId).FirstOrDefault();

                        if (registro == null && registroH == null)
                        {
                            HttpClient cliente2 = new HttpClient();
                            cliente2.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", "vtexappkey-germantecmex-GXWMYU");
                            cliente2.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", "EETXLUZWDLEEUAGTTQFWABFAOFUESFJPPZSMCIDEJXLNPHRZXGWDAYXCYTJGZUEXBPOUHTQKNANCQSLGNVGLIFORCQNJZXEOTOLZSXSEQMQZLUMGICTEOSOWAXRRGHKQ");

                            string path2 = "https://germantecmex.vtexcommercestable.com.br/api/oms/pvt/orders/" + item.orderId;

                            HttpResponseMessage response2 = await cliente2.GetAsync(path2);

                            detOrder detalle = new detOrder();
                            if (response2.IsSuccessStatusCode)
                            {
                                detalle = await response2.Content.ReadAsAsync<detOrder>();

                            }


                            EncOrdenes ordenes = new EncOrdenes();
                            ordenes.orderid = item.orderId;
                            ordenes.creationDate = item.creationDate;
                            ordenes.clientName = item.clientName;
                            ordenes.currencyCode = item.currencyCode;
                            ordenes.totalItems = item.totalItems;
                            ordenes.telefono = detalle.clientProfileData.phone;
                            ordenes.Correo = detalle.clientProfileData.email;
                            ordenes.idVtex = detalle.clientProfileData.userProfileId;
                            ordenes.ProcesadaSAP = false;
                            ordenes.Impuestos = ToDecimal(detalle.totals[3].value);
                            ordenes.Descuento = Math.Abs( (detalle.totals[1].value != 0 ? ToDecimal(detalle.totals[1].value) : 0));
                            ordenes.Subtotal = ToDecimal(detalle.totals[0].value) - ordenes.Descuento;
                            ordenes.Envio = ToDecimal(detalle.totals[2].value);
                            ordenes.Total = ToDecimal(detalle.value) ;
                            ordenes.Comments = detalle.shippingData.address.country + ", " + detalle.shippingData.address.city + ", " + detalle.shippingData.address.street + ", " + detalle.shippingData.address.complement;
                            foreach (var item2 in detalle.items)
                            {
                                DetOrdenes detOrd = new DetOrdenes();
                                detOrd.orderid = detalle.orderId;
                                detOrd.Descuento = (item2.priceTags[0].value != 0 ? ToDecimal(Math.Abs(item2.priceTags[0].value)) : 0 );
                                detOrd.Impuestos = ToDecimal(item2.tax);
                                var descont = (item2.priceTags[0].value != 0 ? Math.Abs(item2.priceTags[0].value) : 0  );
                                detOrd.SubTotal = ToDecimal((item2.quantity * item2.costPrice) - Convert.ToDouble(descont));
                                detOrd.Total = detOrd.Impuestos + detOrd.SubTotal;
                                detOrd.TaxCode =  (detOrd.Descuento > 0 ? item2.priceTags[1].value : item2.priceTags[0].value);
                                detOrd.itemid = item2.productId;
                                detOrd.itemCode = item2.refId;
                                detOrd.unitPrice = ToDecimal(item2.costPrice);
                                detOrd.quantity = item2.quantity;

                                var SQL = " select top 1 U_BOD_VT from oitm where ItemCode ='" + detOrd.itemCode + "'";

                                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());


                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);

                                DataSet Ds = new DataSet();



                                Cn.Open();

                                Da.Fill(Ds, "warehouse");

                                var warehouse = "";
                                try
                                {
                                   warehouse = Ds.Tables["warehouse"].Rows[0]["U_BOD_VT"].ToString();

                                }catch(Exception ex)
                                {
                                    BitacoraErrores error = new BitacoraErrores();
                                    error.Descripcion = ex.Message;
                                    error.StackTrace = "Insercion de la orden";
                                    error.Fecha = DateTime.Now;
                                    db.BitacoraErrores.Add(error);
                                    db.SaveChanges();
                                }

                                Cn.Close();

                                if (String.IsNullOrEmpty(warehouse))
                                {
                                    throw new Exception("No se encontró la bodega");
                                }


                                detOrd.WarehouseCode = warehouse;


                                db.DetOrdenes.Add(detOrd);
                                db.SaveChanges();

                            }

                            if(ordenes.Envio > 0)
                            {
                                DetOrdenes detOrd = new DetOrdenes();
                                detOrd.orderid = detalle.orderId;
                                detOrd.Descuento = 0;
                                detOrd.Impuestos = ordenes.Envio * Convert.ToDecimal( 0.13);
                                detOrd.SubTotal = ordenes.Envio;
                                detOrd.Total = detOrd.Impuestos + detOrd.SubTotal;
                                detOrd.TaxCode = 13;
                                detOrd.itemid = "C0-000-056";
                                detOrd.itemCode = "C0-000-056";
                                detOrd.unitPrice = ordenes.Envio;
                                detOrd.quantity = 1;
                                

                                detOrd.WarehouseCode = "07";


                                db.DetOrdenes.Add(detOrd);
                                db.SaveChanges();
                            }

                            db.EncOrdenes.Add(ordenes);
                            db.SaveChanges();
                        }

                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, product);
            }
            catch (Exception ex)
            {
                BitacoraErrores error = new BitacoraErrores();
                error.Descripcion = ex.Message;
                error.StackTrace = "Insercion de la orden";
                error.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(error);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post()
        {
            var Error = "";
            object resp;
            try
            {

                PurchaseOrderViewModel cliente = new PurchaseOrderViewModel();
                var facturas = db.EncOrdenes.Where(a => a.ProcesadaSAP == false).ToList();

                foreach(var fac in facturas)
                {
                        var SQL = "select top 1 CardCode from OCRD where CardName like '%"+ fac.clientName +"%'";

                    SqlConnection Cn = new SqlConnection(g.DevuelveCadena());


                    SqlCommand Cmd = new SqlCommand(SQL, Cn);
                    SqlDataAdapter Da = new SqlDataAdapter(Cmd);

                    DataSet Ds = new DataSet();



                    Cn.Open();

                    Da.Fill(Ds, "Cliente");

                   var CardCode =  Ds.Tables["Cliente"].Rows[0]["CardCode"].ToString();
                    Cn.Close();

                    if(String.IsNullOrEmpty(CardCode))
                    {
                        throw new Exception("No se encontró el cliente");
                    }






                    var client = (Documents)G.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);
                    client.DocObjectCode = BoObjectTypes.oOrders;
                    client.CardCode = CardCode;
                    client.DocCurrency = fac.currencyCode == "CRC" ? "COL": fac.currencyCode ;
                    client.DocDate = fac.creationDate; //listo
                    client.DocDueDate = fac.creationDate; //listo
                    client.DocNum = 0; //automatico


                    //if (cliente.DocType == "I") //Quemado
                    //{

                        client.DocType = BoDocumentTypes.dDocument_Items;
                    //}
                    //else
                    //{
                    //    client.DocType = BoDocumentTypes.dDocument_Service;
                    //}

                    //if (cliente.HandWritten == "Y") //N siempre
                    //{
                    //    client.HandWritten = BoYesNoEnum.tYES;

                    //}
                    //else
                    //{
                        client.HandWritten = BoYesNoEnum.tNO;

                    //}

                    client.NumAtCard = fac.orderid; //orderid
                    //if (cliente.ReserveInvoice == "Y") // N quemado 
                    //{
                    //    client.ReserveInvoice = BoYesNoEnum.tYES;

                    //}
                    //else
                    //{
                        client.ReserveInvoice = BoYesNoEnum.tNO;
                  //  }

                    client.Series = 79; //79 quemado
                    client.TaxDate = fac.creationDate; //CreationDate
                    //if (!string.IsNullOrEmpty(cliente.U_SCGIE)) //orderID
                    //{
                        client.UserFields.Fields.Item("U_SCGIE").Value = fac.orderid;

                    //}
                    client.Comments = fac.Comments; //direccion
                    client.SalesPersonCode = 47; //Quemado 47


                    var detalle = db.DetOrdenes.Where(a => a.orderid == fac.orderid).ToList();
                     
                    int i = 0;

                    foreach (var item in detalle)
                    {
                        client.Lines.SetCurrentLine(i);
                        //5 -> E-C-01
                        client.Lines.CostingCode = "";
                        client.Lines.CostingCode2 = "";
                        client.Lines.CostingCode3 = "";
                        client.Lines.CostingCode4 = "";
                        client.Lines.CostingCode5 = "E-C-01";
                        client.Lines.Currency = fac.currencyCode; //

                        var PorDesc = 0;
                        if(item.Descuento > 0)
                        {
                           PorDesc = CalculaDescuento((item.quantity * item.unitPrice), item.Descuento);
                        }

                        client.Lines.DiscountPercent = Convert.ToDouble(PorDesc);
                        client.Lines.ItemCode = item.itemCode;
                        client.Lines.Quantity = item.quantity;
                        client.Lines.TaxCode = "IVA"+item.TaxCode.ToString();
                        //if (item.TaxOnly == "Y") //N quemmado
                        //{
                        //    client.Lines.TaxOnly = BoYesNoEnum.tYES;

                        //}
                        //else
                        //{
                            client.Lines.TaxOnly = BoYesNoEnum.tNO;
                       // }

                        client.Lines.UnitPrice = Convert.ToDouble(item.unitPrice);
                        //Base Intermedia pregunta la bodega VTEX a la que pertenece


                        client.Lines.WarehouseCode = item.WarehouseCode;
                        // client.Lines.LineTotal = Convert.ToDouble((item.Quantity * item.UnitPrice) - ((item.Quantity * item.UnitPrice) * (item.DiscountPercent / 100)));
                        client.Lines.Add();
                    }


                    var respuesta = client.Add();

                    if(respuesta == 0)
                    {
                        db.Entry(fac).State = System.Data.Entity.EntityState.Modified;
                        fac.ProcesadaSAP = true;
                        db.SaveChanges();

                    }
                    else
                    {
                        BitacoraErrores error = new BitacoraErrores();
                        error.Descripcion = G.Company.GetLastErrorDescription();
                        error.StackTrace = "Generacion de la factura #: " + fac.orderid;
                        error.Fecha = DateTime.Now;
                        db.BitacoraErrores.Add(error);
                        db.SaveChanges();
                    }

                }



                


             



 
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                resp = new
                {
                    //   Series = pedido.Series.ToString(),
                    Type = "oPurchaseOrders",
                    Status = 0,
                    Message = Error + " " + ex.Message + " ->" + ex.StackTrace,
                    User = G.Company.UserName
                };
                BitacoraErrores error = new BitacoraErrores();
                error.Descripcion = ex.Message;
                error.StackTrace = ex.StackTrace;
                error.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(error);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.InternalServerError, resp);
            }
        }


        public decimal ToDecimal(double text)
        {
            try
            {
                string str = text.ToString();

                var str2 = str.Substring(str.Length - 2);
                var str3 = str.Substring(0, str.Length - 2);
                var comp = str3 + '.' + str2;
                return  Convert.ToDecimal(comp);
            }
            catch (Exception ex)
            {

                BitacoraErrores error = new BitacoraErrores();
                error.Descripcion = ex.Message;
                error.StackTrace = ex.StackTrace;
                error.Fecha = DateTime.Now;

                db.BitacoraErrores.Add(error);
                db.SaveChanges();

                return 0;
            }

           
            
        }

        public int CalculaDescuento(decimal TotalLinea, decimal MontoDescuento)
        {
            try
            {
                int desc = int.Parse(Math.Round((MontoDescuento / TotalLinea), 2).ToString());

                return desc;
            }
            catch (Exception ex)
            {

                BitacoraErrores error = new BitacoraErrores();
                error.Descripcion = ex.Message + " -> " + TotalLinea;
                error.StackTrace = ex.StackTrace;
                error.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(error);
                db.SaveChanges();


                return 0;
            }
        }
    }


}