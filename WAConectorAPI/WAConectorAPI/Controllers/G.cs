using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using WAConectorAPI.Models.ModelCliente;

namespace WAConectorAPI.Controllers
{
    public class G
    {
        public readonly static G _instance = new G();
        public static Company _company = null;
        ModelCliente db = new ModelCliente();
        Metodos Metodos = new Metodos();
        private static G Instance
        {
            get
            {
                return _instance;
            }
        }

        public static Company Company
        {
            get
            {
                if (_company == null)
                    new G().DoSapConnection();

                var ins = Instance;
                return _company;
            }
        }

        private int DoSapConnection()
        {
            var Datos = db.Login.FirstOrDefault();
            _company = new Company
            {
                Server = Datos.ServerSQL,
                LicenseServer = Datos.ServerLicense,
                DbServerType = getBDType(Datos.SQLType),
                language = BoSuppLangs.ln_Spanish_La,
                CompanyDB = Datos.SQLBD,
                UserName = Datos.SAPUser,
                Password = Datos.SAPPass
            };

            var resp = _company.Connect();

            if (resp != 0)
            {
                var msg = _company.GetLastErrorDescription();
                Metodos.GuardarTxt("BitacoraConexion.txt", msg);

                return -1;
            }

            return resp;
        }


        private BoDataServerTypes getBDType(string sql)
        {
            switch (sql)
            {
                case "2005":
                    return BoDataServerTypes.dst_MSSQL2005;
                case "2008":
                    return BoDataServerTypes.dst_MSSQL2008;
                case "2012":
                    return BoDataServerTypes.dst_MSSQL2012;
                case "2014":
                    return BoDataServerTypes.dst_MSSQL2014;
                case "2016":
                    return BoDataServerTypes.dst_MSSQL2016;
                case "HANA":
                    return BoDataServerTypes.dst_HANADB;
                default:
                    return BoDataServerTypes.dst_MSSQL;
            }
        }

        public string DevuelveCadena()
        {
            var Datos = db.Login.FirstOrDefault();

            var sql = "server=" + Datos.ServerSQL + "; database=" + Datos.SQLBD + "; uid=" + Datos.SQLUser + "; pwd=" + Datos.SQLPass + ";";

            return sql;
        }



    }
}