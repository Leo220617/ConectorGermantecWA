using Newtonsoft.Json;
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
        Metodos metodo = new Metodos();
        public class Cliente
        {
            public string CardCode { get; set; }
        }

        public async System.Threading.Tasks.Task<HttpResponseMessage> GetAsync()
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();

                HttpClient cliente = new HttpClient();
                cliente.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", param.APP_KEY);
                cliente.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", param.APP_TOKEN);

                string path = param.urlOrdenesVTEX +"?f_creationDate=creationDate:[" + DateTime.Now.AddDays(-8).Year + "-" + (DateTime.Now.AddDays(-8).Month < 10 ? "0" + DateTime.Now.AddDays(-8).Month.ToString() : DateTime.Now.AddDays(-8).Month.ToString()) + "-" + (DateTime.Now.AddDays(-8).Day < 10 ? "0" + DateTime.Now.AddDays(-8).Day.ToString() : DateTime.Now.AddDays(-8).Day.ToString()) + "T01:00:00.000Z TO ";
                path += DateTime.Now.Year+ "-" + (DateTime.Now.Month < 10 ? "0"+ DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString())  + "-" + (DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString()) + "T23:59:59.999Z]";

                


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
                        try
                        {

                       
                            var registro = db.EncOrdenes.Where(a => a.orderid == item.orderId).FirstOrDefault();
                            var registroH = db.EncOrdenesHistorico.Where(a => a.orderid == item.orderId).FirstOrDefault();

                            if (registro == null && registroH == null)
                            {
                                HttpClient cliente2 = new HttpClient();
                                cliente2.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", param.APP_KEY);
                                cliente2.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", param.APP_TOKEN);

                                //string path2 = "https://germantecmex.vtexcommercestable.com.br/api/oms/pvt/orders/" + item.orderId;
                                string path2 = param.urlOrdenVTEX + item.orderId;
                                HttpResponseMessage response2 = await cliente2.GetAsync(path2);

                                detOrder detalle = new detOrder();
                                if (response2.IsSuccessStatusCode)
                                {
                                    detalle = await response2.Content.ReadAsAsync<detOrder>();

                                }

                                if (item.hostname != "germantecpan")
                                {
                                    EncOrdenes ordenes = new EncOrdenes();
                                    ordenes.orderid = item.orderId;
                                    ordenes.creationDate = item.creationDate;
                                    ordenes.clientName = item.clientName.TrimEnd(' '); ;
                                    ordenes.currencyCode = (item.currencyCode == "CRC" ? "COL" : item.currencyCode);
                                    ordenes.totalItems = item.totalItems;
                                    ordenes.telefono = detalle.clientProfileData.phone;
                                    ordenes.Correo = detalle.clientProfileData.email;
                                    ordenes.idVtex = detalle.clientProfileData.userProfileId;
                                    ordenes.Cedula = (detalle.customData == null ? "00000000":detalle.customData.customApps.Where(a => a.id == "profile-document").FirstOrDefault().fields.documentNew);
                                    ordenes.CreditCardNumber = detalle.paymentData.transactions[0].payments[0].lastDigits;
                                    ordenes.VoucherNum = detalle.paymentData.transactions[0].payments[0].connectorResponses.nsu;
                                    var SQL2 = "select top 1 CardCode from OCRD where E_Mail = '" + ordenes.Correo + "'";

                                    SqlConnection Cn2 = new SqlConnection(g.DevuelveCadena());
                                    SqlCommand Cmd2 = new SqlCommand(SQL2, Cn2);
                                    SqlDataAdapter Da2 = new SqlDataAdapter(Cmd2);
                                    DataSet Ds2 = new DataSet();
                                    Cn2.Open();
                                    Da2.Fill(Ds2, "Cliente");
                                    var CardCode = "";
                                    try
                                    {
                                        CardCode = Ds2.Tables["Cliente"].Rows[0]["CardCode"].ToString();
                                        var client = (SAPbobsCOM.BusinessPartners)G.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

                                        var encontrado = client.GetByKey(CardCode);
                                        if (encontrado)
                                        {
                                            client.CardName = ordenes.clientName;
                                            client.FederalTaxID = ordenes.Cedula;
                                            client.Phone1 = ordenes.telefono;


                                            client.Addresses.Add();
                                            client.Addresses.SetCurrentLine(0);

                                            client.City = detalle.shippingData.address.city;
                                            client.County = detalle.shippingData.address.country;
                                            client.Address = (detalle.shippingData.address.street.Length > 49 ? detalle.shippingData.address.street.Substring(0, 49) : detalle.shippingData.address.street);

                                            client.Addresses.AddressName = client.Address;
                                            client.Addresses.City = detalle.shippingData.address.city;
                                            client.Addresses.County = detalle.shippingData.address.country;
                                            client.Addresses.Street = (detalle.shippingData.address.street.Length > 99 ? detalle.shippingData.address.street.Substring(0, 99) : detalle.shippingData.address.street);
                                            client.Addresses.TypeOfAddress = "S";

                                            var respuesta = client.Update();
                                            if (respuesta != 0)
                                            {
                                                BitacoraErrores error = new BitacoraErrores();
                                                error.Descripcion = G.Company.GetLastErrorDescription();
                                                error.StackTrace = "Actualizacion del cliente en la factura " + ordenes.clientName;
                                                error.Fecha = DateTime.Now;
                                                db.BitacoraErrores.Add(error);
                                                db.SaveChanges();
                                                metodo.EnviarCorreo("Actualizacion del cliente en SAP", error.Descripcion, error.StackTrace);
                                            }
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                     
                                        try
                                        {
                                            var client = (SAPbobsCOM.BusinessPartners)G.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);
                                            client.CardName = ordenes.clientName;
                                            client.GroupCode = 130;
                                            client.EmailAddress = ordenes.Correo;
                                            client.Phone1 = ordenes.telefono;
                                            client.FederalTaxID = ordenes.Cedula;
                                            client.Series = 48;
                                            client.Valid = BoYesNoEnum.tYES;
                                            client.CardType = BoCardTypes.cCustomer;
                                            client.Currency = "##";


                                        
                                            client.Addresses.SetCurrentLine(0);

                                            client.City = detalle.shippingData.address.city;
                                            client.County = detalle.shippingData.address.country;
                                            client.Address = (detalle.shippingData.address.street.Length > 49 ? detalle.shippingData.address.street.Substring(0, 49) : detalle.shippingData.address.street);
                                    
                                            client.Addresses.AddressName = client.Address;
                                            client.Addresses.City = detalle.shippingData.address.city;
                                            client.Addresses.County = detalle.shippingData.address.country;
                                            client.Addresses.Street = (detalle.shippingData.address.street.Length > 99 ? detalle.shippingData.address.street.Substring(0, 99) : detalle.shippingData.address.street);
                                            client.Addresses.TypeOfAddress = "S";
                                            client.Addresses.Add();

                                            var respuest = client.Add();

                                            if (respuest != 0)
                                            {
                                                BitacoraErrores error = new BitacoraErrores();
                                                error.Descripcion = G.Company.GetLastErrorDescription();
                                                error.StackTrace = "Insercion del cliente en la factura " + ordenes.clientName;
                                                error.Fecha = DateTime.Now;
                                                db.BitacoraErrores.Add(error);
                                                db.SaveChanges();
                                                metodo.EnviarCorreo("Insercion del cliente en SAP", error.Descripcion, error.StackTrace);
                                            }

                                        }
                                        catch (Exception ex1)
                                        {
                                            BitacoraErrores error = new BitacoraErrores();
                                            error.Descripcion = ex1.Message;
                                            error.StackTrace = "Insercion del cliente en la factura " + ex1.StackTrace;
                                            error.Fecha = DateTime.Now;
                                            db.BitacoraErrores.Add(error);
                                            db.SaveChanges();
                                            metodo.EnviarCorreo("Insercion del cliente en SAP", error.Descripcion, error.StackTrace);
                                        }




                                    }
                                    Cn2.Close();

                                    ordenes.ProcesadaSAP = false;
                                    ordenes.Impuestos = ToDecimal(detalle.totals[3].value);
                                    ordenes.Descuento = Math.Abs((detalle.totals[1].value != 0 ? ToDecimal(detalle.totals[1].value) : 0));
                                    ordenes.Subtotal = ToDecimal(detalle.totals[0].value) - ordenes.Descuento;
                                    ordenes.Envio = ToDecimal(detalle.totals[2].value);
                                    ordenes.Total = ToDecimal(detalle.value);
                                    ordenes.Comments = detalle.shippingData.address.country + ", " + detalle.shippingData.address.city + ", " + detalle.shippingData.address.street + ", " + detalle.shippingData.address.complement;
                                    foreach (var item2 in detalle.items)
                                    {
                                        DetOrdenes detOrd = new DetOrdenes();
                                        detOrd.orderid = detalle.orderId;
                                        detOrd.Descuento = (item2.priceTags[0].value < 0 ? ToDecimal(Math.Abs(item2.priceTags[0].value)) : 0);
                                        detOrd.Impuestos = ToDecimal(item2.tax);
                                        var descont = (item2.priceTags[0].value < 0 ? Math.Abs(item2.priceTags[0].value) : 0);
                                        detOrd.SubTotal = ToDecimal((item2.quantity * item2.price) - Convert.ToDouble(descont));
                                        detOrd.Total = detOrd.Impuestos + detOrd.SubTotal;
                                        detOrd.TaxCode = (detOrd.Descuento > 0 ? item2.priceTags[1].value : item2.priceTags[0].value);
                                        if(detOrd.Impuestos == 0)
                                        {
                                            //tentativo
                                            //item2.price -= item2.price * (detOrd.TaxCode / 100);
                                            item2.price = item2.price / ((detOrd.TaxCode / 100) + 1);
                                            detOrd.SubTotal = detOrd.SubTotal / ((detOrd.TaxCode / 100) + 1);
                                            detOrd.Impuestos = detOrd.SubTotal * (detOrd.TaxCode / 100);
                                            //detOrd.SubTotal = detOrd.SubTotal - detOrd.Impuestos;
                                        }


                                        detOrd.itemid = item2.productId;
                                        detOrd.itemCode = item2.refId;
                                        detOrd.unitPrice = ToDecimal(item2.price);
                                        detOrd.quantity = item2.quantity;
                                    
                                        var SQL = " select top 1 U_BOD_VT from oitm where ItemCode like '%" + detOrd.itemCode + "%'";

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

                                        }
                                        catch (Exception ex)
                                        {
                                            BitacoraErrores error = new BitacoraErrores();
                                            error.Descripcion = "No existe bodega vtex en el articulo " + " => " + detOrd.itemCode;
                                            error.StackTrace = "Insercion de la orden en la parte de select";
                                            error.Fecha = DateTime.Now;
                                            db.BitacoraErrores.Add(error);
                                            db.SaveChanges();

                                            metodo.EnviarCorreo("Bodega VTEX", error.Descripcion, error.StackTrace);
                                        }

                                        Cn.Close();

                                        //if (String.IsNullOrEmpty(warehouse))
                                        //{
                                        //    throw new Exception("No se encontró la bodega");
                                        //}


                                        detOrd.WarehouseCode = warehouse;


                                        db.DetOrdenes.Add(detOrd);
                                        db.SaveChanges();

                                    }

                                    if (ordenes.Envio > 0)
                                    {
                                        DetOrdenes detOrd = new DetOrdenes();
                                        detOrd.orderid = detalle.orderId;
                                        detOrd.Descuento = 0;
                                        detOrd.Impuestos = ordenes.Envio * Convert.ToDecimal(0.13);
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
                        catch (Exception ex)
                        {

                            BitacoraErrores error = new BitacoraErrores();
                            error.Descripcion = ex.Message;
                            error.StackTrace = "Insercion de la orden "+ item.orderId + " " + ex.StackTrace.ToString();
                            error.Fecha = DateTime.Now;
                            db.BitacoraErrores.Add(error);
                            db.SaveChanges();
                            metodo.EnviarCorreo("Insercion de la orden", error.Descripcion, error.StackTrace);
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, product);
            }
            catch (Exception ex)
            {
                BitacoraErrores error = new BitacoraErrores();
                error.Descripcion = ex.Message;
                error.StackTrace = "Insercion de la orden" + " " + ex.StackTrace.ToString();
                error.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(error);
                db.SaveChanges();
                metodo.EnviarCorreo("Insercion de la orden", error.Descripcion, error.StackTrace);
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
                        var SQL = "select top 1 CardCode from OCRD where E_Mail = '" + fac.Correo +"'";

                    SqlConnection Cn = new SqlConnection(g.DevuelveCadena());


                    SqlCommand Cmd = new SqlCommand(SQL, Cn);
                    SqlDataAdapter Da = new SqlDataAdapter(Cmd);

                    DataSet Ds = new DataSet();



                    Cn.Open();

                    Da.Fill(Ds, "Cliente");

                    var CardCode = "";
                    try
                    {
                        CardCode = Ds.Tables["Cliente"].Rows[0]["CardCode"].ToString();
                    }
                    catch (Exception ex)
                    {

                        
                    }

                    



                    Cn.Close();

                    if(String.IsNullOrEmpty(CardCode))
                    {
                        throw new Exception("No se encontró el cliente");
                    }






                    var client = (Documents)G.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);
                    client.DocObjectCode = BoObjectTypes.oOrders;
                    client.CardCode = CardCode;
                    client.DocCurrency = (fac.currencyCode == "CRC" ? "COL": fac.currencyCode);
                    client.DocDate = fac.creationDate; //listo
                    client.DocDueDate = fac.creationDate; //listo
                    client.DocNum = 0; //automatico

                    //client.DiscountPercent = CalculaDescuento((fac.Subtotal + fac.Descuento),fac.Descuento);
                    

                        client.DocType = BoDocumentTypes.dDocument_Items;
                   
                        client.HandWritten = BoYesNoEnum.tNO;

                 

                    client.NumAtCard = fac.orderid; //orderid
                    
                        client.ReserveInvoice = BoYesNoEnum.tNO;
                

                    client.Series = 5; //79 quemado
                    client.TaxDate = fac.creationDate; //CreationDate
                 
                        client.UserFields.Fields.Item("U_SCGIEC").Value = fac.orderid;

                
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
                        client.Lines.Currency = client.DocCurrency; //

                        //double PorDesc = 0;
                        //if (item.Descuento > 0)
                        //{
                        //    PorDesc = CalculaDescuento((item.quantity * item.unitPrice), item.Descuento);
                        //}


                        client.Lines.DiscountPercent =  0;
                        client.Lines.ItemCode = item.itemCode;
                        client.Lines.Quantity = item.quantity;
                        client.Lines.TaxCode = "IVA-" + item.TaxCode.ToString();
                         
                    
                            client.Lines.TaxOnly = BoYesNoEnum.tNO;
             

                        client.Lines.UnitPrice = Convert.ToDouble(item.SubTotal / item.quantity);
                        //Base Intermedia pregunta la bodega VTEX a la que pertenece

                        if(string.IsNullOrEmpty(item.WarehouseCode))
                        {
                            var SQL2 = " select top 1 U_BOD_VT from oitm where ItemCode like '%" + item.itemCode + "%'";

                            SqlConnection Cn2 = new SqlConnection(g.DevuelveCadena());


                            SqlCommand Cmd2 = new SqlCommand(SQL2, Cn2);
                            SqlDataAdapter Da2 = new SqlDataAdapter(Cmd2);

                            DataSet Ds2 = new DataSet();



                            Cn2.Open();

                            Da2.Fill(Ds2, "warehouse");

                            var warehouse2 = "";
                            try
                            {
                                warehouse2 = Ds2.Tables["warehouse"].Rows[0]["U_BOD_VT"].ToString();

                            }
                            catch (Exception ex)
                            {
                                BitacoraErrores error = new BitacoraErrores();
                                error.Descripcion = ex.Message + " => " + item.itemCode;
                                error.StackTrace = "Insercion de la orden en la parte de select 2";
                                error.Fecha = DateTime.Now;
                                db.BitacoraErrores.Add(error);
                                db.SaveChanges();
                                metodo.EnviarCorreo("No existe bodega para el articulo " + item.itemCode, error.Descripcion, error.StackTrace);
                            }
                            client.Lines.WarehouseCode = warehouse2;
                           Cn.Close();
                        }
                        else
                        {
                            client.Lines.WarehouseCode = item.WarehouseCode;

                        }

                        // client.Lines.LineTotal = Convert.ToDouble((item.Quantity * item.UnitPrice) - ((item.Quantity * item.UnitPrice) * (item.DiscountPercent / 100)));
                        client.Lines.Add();
                        i++;
                    }


                    var respuesta = client.Add();

                    if(respuesta == 0)
                    {
                        db.Entry(fac).State = System.Data.Entity.EntityState.Modified;
                        fac.ProcesadaSAP = true;
                        db.SaveChanges();

                        var Pago = (SAPbobsCOM.Payments)G.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                        Pago.DocObjectCode = BoPaymentsObjectType.bopot_IncomingPayments;
                        Pago.CardCode = CardCode;
                        Pago.DocTypte = BoRcptTypes.rCustomer;
                        Pago.DocDate = DateTime.Now;
                        Pago.DocRate = 0;
                        Pago.HandWritten = 0;
                        Pago.DocCurrency = fac.currencyCode;
                        Pago.ApplyVAT = BoYesNoEnum.tYES;
                        Pago.Remarks = "PagoEcommerce";
                        Pago.JournalRemarks = "PagoEcommerce";
                        Pago.LocalCurrency = BoYesNoEnum.tYES;
                        Pago.UserFields.Fields.Item("U_SCGIEC").Value = fac.orderid;

                        Pago.CreditCards.SetCurrentLine(0);
                        Pago.CreditCards.CardValidUntil = fac.creationDate; //Fecha en la que se mete el pago 
                        Pago.CreditCards.CreditCard = 2; //Quemado
                        Pago.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                        Pago.CreditCards.PaymentMethodCode = 1; //Quemado
                        Pago.CreditCards.CreditCardNumber = fac.CreditCardNumber; // Ultimos 4 digitos
                        Pago.CreditCards.VoucherNum = fac.VoucherNum; // 
                        Pago.CreditCards.CreditSum = Convert.ToDouble(fac.Total);
                        Pago.CreditCards.Add();

                        var resp2 = Pago.Add();
                        if (resp2 != 0)
                        {
                            db.Entry(fac).State = System.Data.Entity.EntityState.Modified;
                            fac.PagoProcesado = false;
                            db.SaveChanges();

                            BitacoraErrores error = new BitacoraErrores();
                            error.Descripcion = G.Company.GetLastErrorDescription();
                            error.StackTrace = "Generacion del pago en la factura #: " + fac.orderid;
                            error.Fecha = DateTime.Now;
                            db.BitacoraErrores.Add(error);
                            db.SaveChanges();
                            metodo.EnviarCorreo("Generar Pago Factura", error.Descripcion, error.StackTrace);
                        }
                        else
                        {
                            db.Entry(fac).State = System.Data.Entity.EntityState.Modified;
                            fac.PagoProcesado = true;
                            db.SaveChanges();
                        }


                    }
                    else
                    {
                        BitacoraErrores error = new BitacoraErrores();
                        error.Descripcion = G.Company.GetLastErrorDescription();
                        error.StackTrace = "Generacion de la factura #: " + fac.orderid;
                        error.Fecha = DateTime.Now;
                        db.BitacoraErrores.Add(error);
                        db.SaveChanges();
                        metodo.EnviarCorreo("Generar Factura", error.Descripcion, error.StackTrace);
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
                metodo.EnviarCorreo("Insercion de la factura en SAP", error.Descripcion, error.StackTrace);
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
                var comp = str3 + ',' + str2;
                return  Convert.ToDecimal(comp);
            }
            catch (Exception ex)
            {

                //BitacoraErrores error = new BitacoraErrores();
                //error.Descripcion = ex.Message;
                //error.StackTrace = ex.StackTrace;
                //error.Fecha = DateTime.Now;

                //db.BitacoraErrores.Add(error);
                //db.SaveChanges();

                return 0;
            }

           
            
        }

        public double CalculaDescuento(decimal TotalLinea, decimal MontoDescuento)
        {
            try
            {
                double desc = Convert.ToDouble((MontoDescuento / TotalLinea).ToString());

                return desc * 100;
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