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
    public class ItemsController: ApiController
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
                sql += " t0.InvntItem, t0.ItemCode, t0.itemName, t0.ItmsGrpCod, t0.ManSerNum, t0.SUoMEntry, t0.validFor from OITM t0  ";

                

                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());


                SqlCommand Cmd = new SqlCommand(sql, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);

                DataSet Ds = new DataSet();

                Cn.Open();

                Da.Fill(Ds, "Items");

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