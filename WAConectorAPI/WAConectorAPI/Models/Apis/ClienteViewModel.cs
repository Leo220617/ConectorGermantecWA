using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WAConectorAPI.Models.Apis
{
    public class ClienteViewModel
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Currency { get; set; }
        public string E_Mail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [StringLength(12)]
        public string LicTradNum { get; set; }
        public int ListNum { get; set; }
        public string MiddleName { get; set; }
        public string Name { get; set; }
        public string Phone1 { get; set; }
        public int Series { get; set; }
        public string Street { get; set; }
        public string TypeOfAddress { get; set; }
        public string ValidFor { get; set; }
        public int GroupCode { get; set; }

        public string AddressName { get; set; }
        public string AddressName2 { get; set; }
        public string Block { get; set; }


       
    }
}