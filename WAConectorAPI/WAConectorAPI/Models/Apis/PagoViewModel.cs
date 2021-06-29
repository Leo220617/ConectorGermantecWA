using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAConectorAPI.Models.Apis
{
    public class PagoViewModel
    {
        public string CardCode { get; set; }
        public string DocCurrency { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int CreditCard { get; set; }
        public int PaymentMethodCode { get; set; }
        public string CreditCardNumber { get; set; }
        public string VoucherNum { get; set; }
        public double CreditSum { get; set; }
    }
}