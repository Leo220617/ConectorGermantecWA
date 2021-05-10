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
    public class InventariosController: ApiController
    {
        G g = new G();
        public HttpResponseMessage Get([FromUri] FiltroInventarios filtro)
        {
            try
            {
                string sql = " select ";
                if(filtro != null && filtro.top > 0)
                {
                    sql += " top " + filtro.top + " ";
                }
                sql += " t0.WhsCode, t0.ItemCode, t3.U_REFCOD, t0.OnHand - t0.IsCommited InStock from OITW t0  ";
                sql += " inner join OWHS t1 on t0.WhsCode = t1.WhsCode ";
                sql += " inner join OITM t3 on t0.ItemCode = t3.ItemCode where t0.WhsCode = T3.U_Bod_VT  "; // Este where nos trae solo los que tienen una bodega asignada

                if(filtro != null)
                {
                    //if(!string.IsNullOrEmpty(filtro.ItemCode) || !string.IsNullOrEmpty(filtro.WhsCodeList))
                    //{

                    //    sql += " where ";
                    //}


                    if(!string.IsNullOrEmpty(filtro.ItemCode))
                    {
                        sql += " and t0.ItemCode = '" + filtro.ItemCode + "' "/* + (!string.IsNullOrEmpty(filtro.WhsCodeList) ? " and ": "")*/; 

                    }


                    //if (!string.IsNullOrEmpty(filtro.WhsCodeList))
                    //{
                    //    sql += " t0.WhsCode = '" + filtro.WhsCodeList+ "' ";
                    //}


                }

                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());


                SqlCommand Cmd = new SqlCommand(sql, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);

                DataSet Ds = new DataSet();

                Cn.Open();

                Da.Fill(Ds, "Productos");

                Cn.Close();

                return Request.CreateResponse(HttpStatusCode.OK, Ds);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /////////////////////////////////PriceList///////////////////////////////////
        [Route("api/Inventarios/PriceList")]
        public HttpResponseMessage GetPriceList([FromUri] FiltroInventarios filtro)
        {
            try
            {
                string sql = " select ";
                if (filtro != null && filtro.top > 0)
                {
                    sql += " top " + filtro.top + " ";
                }
                sql += " t0.Currency, t0.ItemCode, t1.AvgPrice ItemCost, t0.Price, t0.PriceList PriceListId   from ITM1 t0  ";
                sql += " inner join OITM t1 on t0.ItemCode = t1.ItemCode where t1.U_Bod_VT is not null ";
            

                if (filtro != null)
                {
                    //if (!string.IsNullOrEmpty(filtro.PriceListCode))
                    //{

                    //    sql += " where ";
                    //}


                    if (!string.IsNullOrEmpty(filtro.PriceListCode))
                    {
                        sql += " and t0.PriceList = '" + filtro.PriceListCode + "' "  ;

                    }

 


                }

                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());


                SqlCommand Cmd = new SqlCommand(sql, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);

                DataSet Ds = new DataSet();

                Cn.Open();

                Da.Fill(Ds, "PriceList");

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