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

namespace WAConectorAPI.Controllers
{
    public class ClientController: ApiController
    {
        G g = new G();

        public HttpResponseMessage Get([FromUri] FiltroInventarios filtro)
        {
            try
            {
                string sql = " select ";
                if (filtro != null && filtro.top > 0)
                {
                    sql += " top " + filtro.top + " ";
                }
                sql += " Address, Balance, CardCode, CardName, City, County, Currency, Discount, E_Mail, GlblLocNum, GroupCode, LicTradNum, ListNum, Phone1, SlpCode, State1, StreetNo, validFor from OCRD  ";
                
                sql += " where CardType = 'C' "; // Este where nos trae solo los que tienen una bodega asignada

                //if (filtro != null)
                //{


                //    if (!string.IsNullOrEmpty(filtro.ItemCode))
                //    {
                //        sql += " and t0.ItemCode = '" + filtro.ItemCode + "' "/* + (!string.IsNullOrEmpty(filtro.WhsCodeList) ? " and ": "")*/;

                //    }




                //}

                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());


                SqlCommand Cmd = new SqlCommand(sql, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);

                DataSet Ds = new DataSet();

                Cn.Open();

                Da.Fill(Ds, "Clients");

                Cn.Close();

                return Request.CreateResponse(HttpStatusCode.OK, Ds);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post([FromBody] ClienteViewModel cliente)
        {
            var Error = "";
                object resp;
            try
            {
          
           
                var client = (SAPbobsCOM.BusinessPartners)G.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

                if (!string.IsNullOrEmpty(cliente.CardCode))
                {
                    client.CardCode = cliente.CardCode;
                }

                if (!string.IsNullOrEmpty(cliente.CardName))
                {
                    client.CardName = cliente.CardName;
                }

                if (!string.IsNullOrEmpty(cliente.Currency))
                {
                    client.Currency = cliente.Currency;
                }

                if (!string.IsNullOrEmpty(cliente.E_Mail))
                {
                    client.EmailAddress = cliente.E_Mail;
                }





                if (cliente.GroupCode > 0 && cliente.GroupCode != null)
                {
                    client.GroupCode = cliente.GroupCode;
                }

                if (!string.IsNullOrEmpty(cliente.Phone1))
                {
                    client.Phone1 = cliente.Phone1;
                }

                if (!string.IsNullOrEmpty(cliente.CardName))
                {
                    client.CardName = cliente.CardName;
                }

                if (cliente.Series > 0 && cliente.Series != null)
                {
                    client.Series = cliente.Series;
                }

                if (!string.IsNullOrEmpty(cliente.ValidFor))
                {
                    if (cliente.ValidFor == "Y")
                    {
                        client.Valid = BoYesNoEnum.tYES;
                    }
                    else
                    {
                        client.Valid = BoYesNoEnum.tNO;
                    }

                }

                client.CardType = BoCardTypes.cCustomer;
                client.FederalTaxID = cliente.LicTradNum;
                client.Addresses.SetCurrentLine(0);
                if (!string.IsNullOrEmpty(cliente.AddressName))
                {
                    client.Addresses.AddressName = cliente.AddressName;
                    client.Address = cliente.AddressName;
                }

                if (!string.IsNullOrEmpty(cliente.AddressName2))
                {
                    client.Addresses.AddressName2 = cliente.AddressName2;
                }

                if (!string.IsNullOrEmpty(cliente.City))
                {
                    client.Addresses.City = cliente.City;
                }

                if (!string.IsNullOrEmpty(cliente.County))
                {
                    client.Addresses.County = cliente.County;
                }

                if (!string.IsNullOrEmpty(cliente.Street))
                {
                    client.Addresses.Street = cliente.Street;
                }

                if (!string.IsNullOrEmpty(cliente.TypeOfAddress))
                {
                    client.Addresses.TypeOfAddress = cliente.TypeOfAddress;
                }

                client.Addresses.Add();






                var respuesta = client.Add();

                if (respuesta == 0)
                {
                    var docEntry = G.Company.GetNewObjectKey();
                     

                    resp = new 
                    {
                         
                        DocEntry = docEntry,
                        //  Series = pedido.Series.ToString(),
                        Type = "oBussinessPartners",
                        Status = 1,
                        Message = "Socio de negocio creado exitosamente",
                        User = G.Company.UserName
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }


                resp = new
                {
                    //   Series = pedido.Series.ToString(),
                    Type = "oBussinessPartners",
                    Status = 0,
                    Message = G.Company.GetLastErrorDescription(),
                    User = G.Company.UserName
                };
                return Request.CreateResponse(HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                resp = new
                {
                    //   Series = pedido.Series.ToString(),
                    Type = "oBussinessPartners",
                    Status = 0,
                    Message = Error + " " + ex.Message + " ->" + ex.StackTrace,
                    User = G.Company.UserName
                };
                return Request.CreateResponse(HttpStatusCode.InternalServerError, resp);
            }
        }





    }
}