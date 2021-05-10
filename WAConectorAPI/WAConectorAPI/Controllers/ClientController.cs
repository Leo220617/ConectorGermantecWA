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

    }
}