using Newtonsoft.Json;
using SAPbobsCOM;
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
    public class PagosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        Metodos metodo = new Metodos();
        G g = new G();
        object resp;

        [Route("api/Pagos/Insertar")]
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostPagos([FromBody] PagoViewModel pago)
        {
            try
            {
                var Pago = (SAPbobsCOM.Payments)G.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPaymentsDrafts);
                Pago.DocObjectCode = BoPaymentsObjectType.bopot_IncomingPayments;
                Pago.CardCode = pago.CardCode;
                Pago.DocTypte = BoRcptTypes.rCustomer;
                Pago.DocDate = DateTime.Now;
                Pago.DocRate = 0;
                Pago.HandWritten = 0;
                Pago.DocCurrency = pago.DocCurrency;
                Pago.ApplyVAT = BoYesNoEnum.tYES;
                Pago.Remarks = "PagoEcommerce";
                Pago.JournalRemarks = "PagoEcommerce";
                Pago.LocalCurrency = BoYesNoEnum.tYES;

              

                Pago.CreditCards.SetCurrentLine(0);
                Pago.CreditCards.CardValidUntil = new DateTime(pago.Year, pago.Month, 28); //Fecha en la que se mete el pago 
                Pago.CreditCards.CreditCard = pago.CreditCard; //Quemado
                Pago.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                Pago.CreditCards.PaymentMethodCode = pago.PaymentMethodCode; //Quemado
                Pago.CreditCards.CreditCardNumber = pago.CreditCardNumber; // Ultimos 4 digitos
                Pago.CreditCards.VoucherNum = pago.VoucherNum; // 
                Pago.CreditCards.CreditSum = pago.CreditSum;
                Pago.CreditCards.Add();

                var respuesta = Pago.Add();
                if(respuesta == 0)
                {
                    var docEntry = G.Company.GetNewObjectKey();
                     
                    resp = new
                    {
                     
                        DocEntry = docEntry,
                        //  Series = pedido.Series.ToString(),
                        Type = "oPaymentsDrafts",
                        Status = 1,
                        Message = "Pago Preliminar creado exitosamente",
                        User = G.Company.UserName
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }

                resp = new
                {
                    //   Series = pedido.Series.ToString(),
                    Type = "oPaymentsDrafts",
                    Status = 0,
                    Message = G.Company.GetLastErrorDescription(),
                    User = G.Company.UserName,

                };



                return Request.CreateResponse(HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
               resp = new {
                    //   Series = pedido.Series.ToString(),
                    Type = "oPaymentsDrafts",
                        Status = 0,
                        Message = "[Stack] -> " + ex.StackTrace + " -- [Message] --> " + ex.Message,
                        User = G.Company.UserName
                    };
                return Request.CreateResponse(HttpStatusCode.InternalServerError, resp);
            }
        }



        public HttpResponseMessage Get( )
        {
            try
            {
                var facturas = db.EncOrdenes.Where(a => a.PagoProcesado == false && a.ProcesadaSAP == true).ToList();

                foreach(var fac in facturas)
                {
                    var SQL = "select top 1 CardCode from OCRD where E_Mail = '" + fac.Correo + "'";

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

                    if (String.IsNullOrEmpty(CardCode))
                    {
                        throw new Exception("No se encontró el cliente");
                    }

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
                        error.StackTrace = "Generacion del pago en la contingencia, en la factura #: " + fac.orderid;
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

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {
                BitacoraErrores error = new BitacoraErrores();
                error.Descripcion = ex.Message;
                error.StackTrace = ex.StackTrace;
                error.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(error);
                db.SaveChanges();
                metodo.EnviarCorreo("Generar Pago Factura", error.Descripcion, error.StackTrace);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, error);
            }
        }

    }
}