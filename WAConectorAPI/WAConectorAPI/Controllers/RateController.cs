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
    public class RateController: ApiController
    {
        G g = new G();

        public HttpResponseMessage Get([FromUri] FiltroInventarios filtro)
        {
            try
            {
                string sql = " select Currency, Rate, RateDate from ORTT ";


                if (filtro != null)
                {
                    DateTime time = new DateTime();

                    if (filtro.Date != time || !string.IsNullOrEmpty(filtro.Currency))
                    {

                        sql += " where ";
                    }

                    if(filtro.Date != time)
                    {
                        sql += " RateDate = '" + filtro.Date.Year + (filtro.Date.Month <10 ? "0"+ filtro.Date.Month.ToString() : filtro.Date.Month.ToString()) + (filtro.Date.Day < 10 ? "0" + filtro.Date.Day.ToString() : filtro.Date.Day.ToString()) + "' " + (!string.IsNullOrEmpty(filtro.Currency) ? " and " : "");
                    }

                    if (!string.IsNullOrEmpty(filtro.Currency))
                    {
                        sql += " Currency = '" + filtro.Currency + "' "/* + (!string.IsNullOrEmpty(filtro.WhsCodeList) ? " and ": "")*/;

                    }

 


                }

                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());


                SqlCommand Cmd = new SqlCommand(sql, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);

                DataSet Ds = new DataSet();

                Cn.Open();

                Da.Fill(Ds, "Rate");

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