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

    }
}