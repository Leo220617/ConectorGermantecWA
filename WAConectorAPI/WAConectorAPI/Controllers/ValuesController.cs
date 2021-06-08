using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WAConectorAPI.Models.Vtex;

namespace WAConectorAPI.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public string Get()
        {
           

            try
            {
                int resp = G.Company.Connect();
                if (resp != 0)
                {
                    return G.Company.GetLastErrorDescription();
                }
                else
                {
                    return resp.ToString();
                }
            }
            catch (Exception ex)
            {

                return ex.Message;
            }

    
        }

        // GET api/values/5
        public async System.Threading.Tasks.Task<getOrder> GetAsync(int id)
        {
            WebClient client = new WebClient();
            client.Headers.Add("X-VTEX-API-AppKey", "vtexappkey-germantecmex-GXWMYU");
            client.Headers.Add("X-VTEX-API-AppToken", "EETXLUZWDLEEUAGTTQFWABFAOFUESFJPPZSMCIDEJXLNPHRZXGWDAYXCYTJGZUEXBPOUHTQKNANCQSLGNVGLIFORCQNJZXEOTOLZSXSEQMQZLUMGICTEOSOWAXRRGHKQ");

            HttpClient cliente = new HttpClient();
            cliente.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", "vtexappkey-germantecmex-GXWMYU");
            cliente.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", "EETXLUZWDLEEUAGTTQFWABFAOFUESFJPPZSMCIDEJXLNPHRZXGWDAYXCYTJGZUEXBPOUHTQKNANCQSLGNVGLIFORCQNJZXEOTOLZSXSEQMQZLUMGICTEOSOWAXRRGHKQ");

            string path = "https://germantecmex.vtexcommercestable.com.br/api/oms/pvt/orders/1135783407665-01";

            HttpResponseMessage response = await cliente.GetAsync(path);

            getOrder product = new getOrder();
            if (response.IsSuccessStatusCode)
            {
                product = await response.Content.ReadAsAsync<getOrder>();

            }





            return product;
        }

        // POST api/values
        public HttpResponseMessage Post()
        {
            try
            {
                var oInvoice = (SAPbobsCOM.Payments)G.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPaymentsDrafts);

                oInvoice.DocObjectCode = BoPaymentsObjectType.bopot_OutgoingPayments;

                oInvoice.Address = "";
                oInvoice.ApplyVAT = BoYesNoEnum.tYES;
                oInvoice.CardCode = "CN00022472";

                 
                 

                    oInvoice.CashAccount = "111-02-04";
                    oInvoice.CashSum = 1000;
                

                oInvoice.ContactPersonCode = 0;

                oInvoice.DocDate = DateTime.Now;
                oInvoice.DocRate = 0;
                oInvoice.DocTypte = BoRcptTypes.rSupplier;
                // oInvoice.DocTypte = BoRcptTypes.rAccount;
                oInvoice.HandWritten = 0;
                oInvoice.DocCurrency = "COL";
                oInvoice.LocalCurrency = BoYesNoEnum.tNO;


                //oInvoice.AccountPayments.AccountCode = coti.AccountCode;
                //oInvoice.AccountPayments.SumPaid = coti.SumPaid;
                //oInvoice.AccountPayments.Add();








                var respuesta = oInvoice.Add();

                if (respuesta == 0)
                {
                    var docEntry = G.Company.GetNewObjectKey();

                    var resp2 = new
                    {
                        DocEntry = docEntry,

                        Type = "oPaymentsDrafts",
                        Status = 1,
                        Message = "Pago Efectuado Preliminar creado exitosamente",
                        User = G.Company.UserName
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, resp2);

                }


                var resp = new
                {

                    Type = "oPaymentsDrafts",
                    Status = 0,
                    Message = G.Company.GetLastErrorDescription(),
                    User = G.Company.UserName
                };

                return Request.CreateResponse(HttpStatusCode.OK, resp);


            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
